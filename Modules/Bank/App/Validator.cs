using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using NodaTime;
using NodaTime.Text;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Core.Validator;
using static Frederikskaj2.Reservations.Orders.Validator;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class Validator
{
    static readonly LocalDatePattern datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
    static readonly Encoding iso8859Encoding = Encoding.GetEncoding("iso-8859-1");

    public static EitherAsync<Failure<ImportError>, ImportBankTransactionsCommand> ValidateFormFile(
        HttpContext httpContext, CancellationToken cancellationToken) =>
        httpContext.Request.Form.Files.Count is 1
            ? GetFile(httpContext.Request.Form.Files[0], cancellationToken)
            : Failure.New(HttpStatusCode.UnprocessableEntity, ImportError.InvalidRequest, "None or too many files are provided.");

    static EitherAsync<Failure<ImportError>, ImportBankTransactionsCommand> GetFile(IFormFile file, CancellationToken cancellationToken)
    {
        return Run().ToAsync();

        async Task<Either<Failure<ImportError>, ImportBankTransactionsCommand>> Run()
        {
            await using var memoryStream = new MemoryStream((int) file.Length);
            await file.CopyToAsync(memoryStream, cancellationToken);
            return new ImportBankTransactionsCommand(iso8859Encoding.GetString(memoryStream.ToArray()));
        }
    }

    public static Either<Failure<Unit>, GetBankTransactionsQuery> ValidateGetBankTransactions(
        DateOnly? startDate, DateOnly? endDate, bool? includeUnknown, bool? includeIgnored, bool? includeReconciled)
    {
        var either =
            from _ in ValidateDates(startDate, endDate)
            select new GetBankTransactionsQuery(
                GetDateOption(startDate), GetDateOption(endDate), includeUnknown ?? false, includeIgnored ?? false, includeReconciled ?? false);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, Unit> ValidateDates(DateOnly? startDate, DateOnly? endDate) =>
        startDate is null || endDate is null || startDate.Value < endDate.Value ? unit : "The start date is not before the end date";

    static Option<LocalDate> GetDateOption(DateOnly? date) =>
        date.HasValue ? LocalDate.FromDateOnly(date.Value) : None;

    public static Either<Failure<Unit>, UpdateBankTransactionCommand> ValidateUpdateBankTransaction(int bankTransactionId, UpdateBankTransactionRequest request)
    {
        var either =
            from status in ValidateBankTransactionStatus(request.Status, "Status")
            select new UpdateBankTransactionCommand(bankTransactionId, status);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, BankTransactionStatus> ValidateBankTransactionStatus(BankTransactionStatus status, string context) =>
        status is BankTransactionStatus.Unknown or BankTransactionStatus.Ignored
            ? status
            : $"{context} with value '{status}' is not valid.";

    public static Either<Failure<Unit>, CreatePayOutCommand> ValidateCreatePayOut(CreatePayOutRequest request, Instant timestamp)
    {
        var either =
            from amount in ValidateAmount(request.Amount, ValidationRule.MinimumAmount, ValidationRule.MaximumAmount, "Amount")
            select new CreatePayOutCommand(timestamp, request.ResidentId, amount);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, ETag?> ValidateETag(string? eTag) =>
        eTag is not null && ETag.IsValid(eTag) ? ETag.FromString(eTag) : null;

    public static Either<Failure<Unit>, ReconcileCommand> ValidateReconcile(
        Instant timestamp, UserId createdByUserId, int bankTransactionId, string? paymentId)
    {
        var either =
            from userId in ValidatePaymentId(paymentId)
            select new ReconcileCommand(timestamp, createdByUserId, bankTransactionId, userId);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, UserId> ValidatePaymentId(string? paymentId) =>
        IsValid(paymentId) ? ToUserId(PaymentId.FromString(paymentId)) : Left("Invalid payment ID.");

    public static Either<Failure<Unit>, LocalDate> ValidateMonth(string? month)
    {
        var either = from nonEmptyMonth in IsNotNullOrEmpty(month, "Month")
            from date in ValidateDate(nonEmptyMonth, "Month")
            from _ in IsFirstDayOfMonth(date, "Month")
            select date;
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, LocalDate> ValidateDate(string date, string context) =>
        datePattern.Parse(date).TryGetValue(default, out var value) ? value : $"{context} is not a valid date.";

    static Either<string, Unit> IsFirstDayOfMonth(LocalDate date, string context) =>
        date.Day == 1 ? unit : $"{context} is not first day of month.";
}
