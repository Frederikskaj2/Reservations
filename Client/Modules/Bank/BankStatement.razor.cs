using Blazorise;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

[Authorize(Policy = Policy.ViewOrders)]
partial class BankStatement
{
    static readonly LocalDatePattern datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
    static readonly Encoding encoding = Encoding.GetEncoding("iso-8859-1");

    ConfirmReconciliationDialog confirmReconciliationDialog = null!;
    int currentMonth;
    Period currentPeriod;
    int currentYear;
    DateRange? dateRange;
    FindResidentDialog findResidentDialog = null!;
    bool includeIgnored;
    bool includeReconciled = true;
    bool includeUnknown = true;
    bool isInitialized;
    LocalDate? latestImportStartDate;
    LocalDate? latestTransactionDate;
    List<int>? months;
    LocalDatePattern? monthPattern;
    IEnumerable<PayOutDto>? payOuts;
    IEnumerable<ResidentDto>? residents;
    IFileEntry? selectedFile;
    bool showImportErrorAlert;
    bool showImportSuccessAlert;
    bool showReconcileErrorAlert;
    bool showReconcileSuccessAlert;
    bool showStatusUpdateErrorAlert;
    bool showStatusUpdateSuccessAlert;
    IEnumerable<BankTransactionDto>? transactions;
    List<int>? years;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public CultureInfo CultureInfo { get; set; } = null!;
    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        monthPattern = LocalDatePattern.Create("MMMM", CultureInfo);
        var response = await ApiClient.Get<GetBankTransactionsRangeResponse>("bank/transactions/range");
        if (response.IsSuccess)
        {
            var getBankTransactionsRangeResponse = response.Result!;
            UpdateUi(getBankTransactionsRangeResponse.LatestImportStartDate, getBankTransactionsRangeResponse.DateRange);
            await Update();
        }
        isInitialized = true;
    }

    void UpdateUi(LocalDate? importStartDate, DateRange? range)
    {
        latestImportStartDate = importStartDate;
        if (latestImportStartDate is not null)
            currentPeriod =  Period.LatestImport;
        dateRange = range;
        if (dateRange is null)
            return;
        latestTransactionDate = dateRange.LatestDate;
        var earliestYear = dateRange.EarliestDate.Year;
        var latestYear = dateRange.LatestDate.Year;
        years = Enumerable.Range(earliestYear, latestYear - earliestYear + 1).ToList();
        currentYear = years[^1];
        UpdateMonths();
    }

    void UpdateMonths()
    {
        months = GetMonths(currentYear).ToList();
        currentMonth = months[^1];
    }

    IEnumerable<int> GetMonths(int year)
    {
        for (var month = 1; month <= 12; month += 1)
        {
            var date = new LocalDate(year, month, 1);
            if (GetMonthStart(dateRange!.EarliestDate) <= date && date <= GetMonthStart(dateRange.LatestDate))
                yield return month;
        }
    }

    static LocalDate GetMonthStart(LocalDate date) =>
        date.PlusDays(-(date.Day - 1));

    string Capitalize(string text) => CultureInfo.TextInfo.ToUpper(text[0]) + text[1..];

    async Task Update()
    {
        await Task.WhenAll(UpdateResidents(), UpdatePayOuts(), UpdateTransactions());
        StateHasChanged();
    }

    async Task UpdateResidents()
    {
        var response = await ApiClient.Get<GetResidentsResponse>("residents");
        residents = response.IsSuccess ? response.Result!.Residents : null;
    }

    async Task UpdatePayOuts()
    {
        var response = await ApiClient.Get<GetPayOutsResponse>("bank/pay-outs");
        payOuts = response.IsSuccess ? response.Result!.PayOuts : null;
    }

    async Task UpdateTransactions()
    {
        var requestUri = $"bank/transactions{Url.FormatQuery(GetQueryParameters())}";
        var response = await ApiClient.Get<GetBankTransactionsResponse>(requestUri);
        transactions = response.IsSuccess ? response.Result!.Transactions : null;
        StateHasChanged();
    }

    IEnumerable<(string Name, string Value)> GetQueryParameters()
    {
        if (currentPeriod is Period.LatestImport)
            yield return ("startDate", datePattern.Format(latestImportStartDate!.Value));
        else if (currentPeriod is Period.Latest30Days)
        {
            var startDate = DateProvider.Today.PlusDays(-30);
            yield return ("startDate", datePattern.Format(startDate));
        }
        else
        {
            var startDate = new LocalDate(currentYear, currentMonth, 1);
            var endDate = startDate.PlusMonths(1);
            yield return ("startDate", datePattern.Format(startDate));
            yield return ("endDate", datePattern.Format(endDate));
        }

        if (includeUnknown)
            yield return ("includeUnknown", "true");
        if (includeReconciled)
            yield return ("includeReconciled", "true");
        if (includeIgnored)
            yield return ("includeIgnored", "true");
    }

    void SelectFile(FileChangedEventArgs args) => selectedFile = args.Files.SingleOrDefault();

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    async Task Upload()
    {
        DismissAllAlerts();
        try
        {
            using var streamReader = new StreamReader(selectedFile!.OpenReadStream(), encoding);
            var csvContent = await streamReader.ReadToEndAsync();
            using var byteArrayContent = new ByteArrayContent(encoding.GetBytes(csvContent));
            byteArrayContent.Headers.ContentType = new("text/csv");
            using var multipartFormDataContent = new MultipartFormDataContent();
            multipartFormDataContent.Add(byteArrayContent, "csv", selectedFile?.Name ?? "file.csv");
            var response = await ApiClient.Post<ImportBankTransactionsResponse>("/bank/transactions/import", multipartFormDataContent);
            if (response.IsSuccess)
            {
                showImportSuccessAlert = true;
                var importBankTransactionsResponse = response.Result!;
                UpdateUi(importBankTransactionsResponse.LatestImportStartDate, importBankTransactionsResponse.DateRange);
                latestImportStartDate = response.Result!.LatestImportStartDate;
                if (latestImportStartDate is not null)
                    currentPeriod =  Period.LatestImport;
                await UpdateTransactions();
            }
            else
                showImportErrorAlert = true;
        }
        catch (Exception exception)
        {
            DismissAllAlerts();
            showImportErrorAlert = true;
            Console.WriteLine(exception);
        }
    }

    Task PeriodChanged(Period period)
    {
        currentPeriod = period;
        return UpdateTransactions();
    }

    Task YearChanged(int value)
    {
        currentYear = value;
        UpdateMonths();
        return UpdateTransactions();
    }

    Task MonthChanged(int value)
    {
        currentMonth = value;
        return UpdateTransactions();
    }

    Task ToggleIncludeUnknown()
    {
        includeUnknown = !includeUnknown;
        return UpdateTransactions();
    }

    Task ToggleIncludeReconciled()
    {
        includeReconciled = !includeReconciled;
        return UpdateTransactions();
    }

    Task ToggleIncludeIgnored()
    {
        includeIgnored = !includeIgnored;
        return UpdateTransactions();
    }

    Task SelectResident((BankTransactionDto Transaction, ResidentDto Resident) tuple) =>
        confirmReconciliationDialog.Show(tuple.Transaction, tuple.Resident);

    async Task Reconcile((BankTransactionDto Transaction, ResidentDto Resident) tuple)
    {
        DismissAllAlerts();
        var requestUri = $"bank/transactions/{tuple.Transaction.BankTransactionId}/reconcile/{tuple.Resident.PaymentId}";
        var response = await ApiClient.Post<ReconcileResponse>(requestUri);
        if (response.IsSuccess)
        {
            showReconcileSuccessAlert = true;
            await Update();
        }
        else
            showReconcileErrorAlert = true;
    }

    async Task ReconcilePayOut((BankTransactionDto Transaction, PayOutDto PayOut) tuple)
    {
        DismissAllAlerts();
        var requestUri = $"bank/transactions/{tuple.Transaction.BankTransactionId}/reconcile-pay-out/{tuple.PayOut.PayOutId}";
        var response = await ApiClient.Post<ReconcilePayOutResponse>(requestUri);
        if (response.IsSuccess)
        {
            showReconcileSuccessAlert = true;
            await Update();
        }
        else
            showReconcileErrorAlert = true;
    }

    async Task SetTransactionStatus((BankTransactionId TransactionId, BankTransactionStatus Status) tuple)
    {
        DismissAllAlerts();
        var request = new UpdateBankTransactionRequest(tuple.Status);
        var response = await ApiClient.Patch($"bank/transactions/{tuple.TransactionId}", request);
        if (response.IsSuccess)
        {
            showStatusUpdateSuccessAlert = true;
            await UpdateTransactions();
        }
        else
            showStatusUpdateErrorAlert = true;
    }

    void DismissImportSuccessAlert() => showImportSuccessAlert = false;

    void DismissImportErrorAlert() => showImportErrorAlert = false;

    void DismissReconcileSuccessAlert() => showReconcileSuccessAlert = false;

    void DismissReconcileErrorAlert() => showReconcileErrorAlert = false;

    void DismissStatusUpdateSuccessAlert() => showStatusUpdateSuccessAlert = false;

    void DismissStatusUpdateErrorAlert() => showStatusUpdateErrorAlert = false;

    void DismissAllAlerts()
    {
        DismissImportSuccessAlert();
        DismissImportErrorAlert();
        DismissReconcileSuccessAlert();
        DismissReconcileErrorAlert();
        DismissStatusUpdateSuccessAlert();
        DismissStatusUpdateErrorAlert();
    }

    enum Period
    {
        Latest30Days,
        LatestImport,
        Month,
    }
}
