using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using NodaTime.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class BankExtensions
{
    static readonly LocalDatePattern csvDatePattern = LocalDatePattern.CreateWithInvariantCulture("dd.MM.yyyy");
    static readonly LocalDatePattern queryDatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
    static readonly CultureInfo cultureInfo = CultureInfo.GetCultureInfo("da-DK");

    public static async ValueTask<BankTransactionDto> ImportBankTransaction(this SessionFixture session, LocalDate paymentDate, UserId userId, Amount amount)
    {
        var transactions = await session.GetBankTransactions(
            startDate: null, endDate: null, includeUnknown: true, includeIgnored: true, includeReconciled: true);
        var previousBalance = transactions.Transactions.LastOrDefault()?.Balance ?? Amount.Zero;
        var date = csvDatePattern.Format(paymentDate);
        var paymentId = PaymentIdEncoder.FromUserId(userId);
        var amountString = amount.ToDecimal().ToString("N2", cultureInfo);
        var balanceString = (previousBalance + amount).ToDecimal().ToString("N2", cultureInfo);
        var csvContent =
            $"""
             "Bogført dato";"Rentedato";"Tekst";"Antal";"Beløb i DKK";"Bogført saldo i DKK";"Status";"Bankens arkivreference"
             "{date}";"{date}";"{paymentId}";"";"{amountString}";"{balanceString}";"Udført";"0000      0000000000"
             """;
        var importBankTransactionsResponse = await session.ImportBankTransactions(csvContent);
        var getBankTransactionsResponse = await session.GetBankTransactions(
            importBankTransactionsResponse.LatestImportStartDate, endDate: null, includeUnknown: true, includeIgnored: true, includeReconciled: true);
        return getBankTransactionsResponse.Transactions.Last();
    }

    public static async ValueTask<ImportBankTransactionsResponse> ImportBankTransactions(this SessionFixture session, string csvContent)
    {
        using var byteArrayContent = new ByteArrayContent(Encoding.GetEncoding("iso-8859-1").GetBytes(csvContent));
        byteArrayContent.Headers.ContentType = new("text/csv");
        using var multipartFormDataContent = new MultipartFormDataContent();
        multipartFormDataContent.Add(byteArrayContent, "csv", "file.csv");
        return await session.Deserialize<ImportBankTransactionsResponse>(
            await session.AdministratorPostContent("/bank/transactions/import", multipartFormDataContent));
    }

    public static async ValueTask<GetBankTransactionsResponse> GetBankTransactions(
        this SessionFixture session, LocalDate? startDate, LocalDate? endDate, bool includeUnknown, bool includeIgnored, bool includeReconciled)
    {
        var queryParameters = new List<string>();
        if (startDate.HasValue)
            queryParameters.Add($"startDate={queryDatePattern.Format(startDate.Value)}");
        if (endDate.HasValue)
            queryParameters.Add($"endDate={queryDatePattern.Format(endDate.Value)}");
        if (includeUnknown)
            queryParameters.Add("includeUnknown=true");
        if (includeIgnored)
            queryParameters.Add("includeIgnored=true");
        if (includeReconciled)
            queryParameters.Add("includeReconciled=true");
        var queryString = string.Join('&', queryParameters);
        return await session.Deserialize<GetBankTransactionsResponse>(await session.AdministratorGet($"bank/transactions?{queryString}"));
    }

    public static async ValueTask<UpdateBankTransactionResponse> UpdateBankTransaction(
        this SessionFixture session, BankTransactionId bankTransactionId, BankTransactionStatus status) =>
        await session.Deserialize<UpdateBankTransactionResponse>(
            await session.AdministratorPatch($"bank/transactions/{bankTransactionId}", new UpdateBankTransactionRequest(status)));

    public static async ValueTask<GetPayOutsResponse> GetPayOuts(this SessionFixture session) =>
        await session.Deserialize<GetPayOutsResponse>(await session.AdministratorGet("bank/pay-outs"));

    public static async ValueTask<GetPayOutResponse> GetPayOut(this SessionFixture session, PayOutId payOutId) =>
        await session.Deserialize<GetPayOutResponse>(await session.AdministratorGet($"bank/pay-outs/{payOutId}"));

    public static async ValueTask<CreatePayOutResponse> CreatePayOut(this SessionFixture session, UserId userId, string accountNumber, Amount amount)
    {
        var httpResponseMessage = await session.CreatePayOutRaw(userId, accountNumber, amount);
        return await session.Deserialize<CreatePayOutResponse>(httpResponseMessage);
    }

    public static ValueTask<HttpResponseMessage> CreatePayOutRaw(this SessionFixture session, UserId userId, string accountNumber, Amount amount) =>
        session.AdministratorPost("/bank/pay-outs", new CreatePayOutRequest(userId, accountNumber, amount));

    public static async ValueTask<AddPayOutNoteResponse> AddPayOutNote(this SessionFixture session, PayOutId payOutId, string text)
    {
        var httpResponseMessage = await session.AdministratorPost($"bank/pay-outs/{payOutId}/notes", new AddPayOutNoteRequest(text));
        return await session.Deserialize<AddPayOutNoteResponse>(httpResponseMessage);
    }

    public static async ValueTask<AddPayOutNoteResponse> UpdatePayOutAccountNumber(this SessionFixture session, PayOutId payOutId, string accountNumber)
    {
        var httpResponseMessage = await session.AdministratorPatch($"bank/pay-outs/{payOutId}", new UpdatePayOutAccountNumberRequest(accountNumber));
        return await session.Deserialize<AddPayOutNoteResponse>(httpResponseMessage);
    }

    public static async ValueTask<CancelPayOutResponse> CancelPayOut(this SessionFixture session, PayOutId payOutId)
    {
        var httpResponseMessage = await session.AdministratorPost($"bank/pay-outs/{payOutId}/cancel");
        return await session.Deserialize<CancelPayOutResponse>(httpResponseMessage);
    }

    public static async ValueTask<ReconcilePayOutResponse> ReconcilePayOut(
        this SessionFixture session, BankTransactionId bankTransactionId, PayOutId payOutId) =>
        await session.Deserialize<ReconcilePayOutResponse>(
            await session.AdministratorPost($"bank/transactions/{bankTransactionId}/reconcile-pay-out/{payOutId}"));

    public static async ValueTask<ReconcileResponse> Reconcile(this SessionFixture session, BankTransactionId bankTransactionId, PaymentId paymentId) =>
        await session.Deserialize<ReconcileResponse>(await session.AdministratorPost($"bank/transactions/{bankTransactionId}/reconcile/{paymentId}"));

    public static async ValueTask<ReconcileResponse> PayIn(this SessionFixture session, PaymentId paymentId, Amount amount)
    {
        var transaction = await session.ImportBankTransaction(session.CurrentDate, PaymentIdEncoder.ToUserId(paymentId), amount);
        return await session.Reconcile(transaction.BankTransactionId, paymentId);
    }

    public static async ValueTask<ReconcilePayOutResponse> PayOut(this SessionFixture session, UserId userId, string accountNumber, Amount amount)
    {
        var transaction = await session.ImportBankTransaction(session.CurrentDate, userId, -amount);
        var payOut = await session.CreatePayOut(userId, accountNumber, amount);
        return await session.ReconcilePayOut(transaction.BankTransactionId, payOut.PayOut.PayOutId);
    }
}
