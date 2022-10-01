using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public partial class PostingsPage
{
    static readonly Regex monthRegex = new(@"^(?<year>\d{4})-(?<month>\d{2})$");

    Dictionary<PostingAccount, string>? accountNames;
    int currentMonth;
    int currentYear;
    EmailAddress email;
    bool isInitialized;
    LocalDatePattern? monthPattern;
    List<int>? months;
    IEnumerable<Posting>? postings;
    PostingsRange? postingsRange;
    bool showErrorAlert;
    bool showSuccessAlert;
    List<int>? years;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] public CultureInfo CultureInfo { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        monthPattern = LocalDatePattern.Create("MMMM", CultureInfo);

        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;

        accountNames = (await DataProvider.GetAccountNamesAsync())?.ToDictionary(accountName => accountName.Account, accountName => accountName.Name);
        if (accountNames is not null)
            await InitializeCalendar();

        isInitialized = true;

        if (!TryParseQuery(NavigationManager.Uri, out var tuple))
        {
            await UpdatePostings();
            var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
            NavigationManager.NavigateTo(uriBuilder.Uri.ToString(), false, true);
        }
        else
            await SetMonth(tuple.Year, tuple.Month);
    }

    void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (TryParseQuery(e.Location, out var tuple) && (tuple.Year != currentYear || tuple.Month != currentMonth))
#pragma warning disable CS4014
            SetMonth(tuple.Year, tuple.Month);
#pragma warning restore CS4014
    }

    async ValueTask InitializeCalendar()
    {
        var response = await ApiClient.GetAsync<PostingsRange>("postings/range");
        if (!response.IsSuccess)
            return;
        postingsRange = response.Result;
        var earliestYear = postingsRange!.EarliestMonth.Year;
        var latestYear = postingsRange.LatestMonth.Year;
        years = Enumerable.Range(earliestYear, latestYear - earliestYear + 1).ToList();
        currentYear = years[^1];
        UpdateMonths();
    }

    async ValueTask UpdatePostings()
    {
        if (postingsRange is null)
            return;
        var requestUri = $"postings?month={currentYear:0000}-{currentMonth:00}-01";
        var response = await ApiClient.GetAsync<IEnumerable<Posting>>(requestUri);
        if (response.IsSuccess)
            postings = response.Result;
    }

    async Task YearChanged(int value)
    {
        currentYear = value;
        UpdateMonths();
        await UpdatePostings();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
    }

    async Task MonthChanged(int value)
    {
        currentMonth = value;
        await UpdatePostings();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
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
            if (postingsRange!.EarliestMonth <= date && date <= postingsRange.LatestMonth)
                yield return month;
        }
    }

    string Capitalize(string text) => CultureInfo.TextInfo.ToUpper(text[0]) + text[1..];

    async Task Send()
    {
        DismissAllAlerts();

        var response = await ApiClient.PostAsync<EmailAddress>($"postings/send?month={currentYear:0000}-{currentMonth:00}-01");
        if (response.IsSuccess)
        {
            email = response.Result;
            showSuccessAlert = true;
        }
        else
            showErrorAlert = true;
    }

    void DismissSuccessAlert() => showSuccessAlert = false;

    void DismissErrorAlert() => showErrorAlert = false;

    void DismissAllAlerts()
    {
        DismissSuccessAlert();
        DismissErrorAlert();
    }

    string FormatQuery() =>
        $"month={currentYear:0000}-{currentMonth:00}";

    async Task SetMonth(int year, int month)
    {
        if (years!.Contains(year))
        {
            currentYear = year;
            UpdateMonths();
            if (months!.Contains(month))
                currentMonth = month;
        }
        if (currentYear != year || currentMonth != month)
        {
            var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
            NavigationManager.NavigateTo(uriBuilder.Uri.ToString(), false, true);
        }
        await UpdatePostings();
        StateHasChanged();
    }

    static bool TryParseQuery(string location, out (int Year, int Month) tuple)
    {
        var query = QueryParser.GetQuery(location);
        string? queryMonth = null;
        if (query.Contains("month"))
            queryMonth = query["month"].FirstOrDefault();
        if (queryMonth is null)
        {
            tuple = default;
            return false;
        }
        var match = monthRegex.Match(queryMonth);
        if (!match.Success)
        {
            tuple = default;
            return false;
        }
        tuple = (
            int.Parse(match.Groups["year"].Value, NumberStyles.None, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["month"].Value, NumberStyles.None, CultureInfo.InvariantCulture));
        return true;
    }
}
