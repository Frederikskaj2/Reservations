using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

[Authorize(Roles = nameof(Roles.OrderHandling))]
public partial class Reports
{
    static readonly Regex yearRegex = new(@"^\d{4}$");

    int currentYear;
    bool isInitialized;
    YearlySummary? yearlySummary;
    YearlySummaryRange? yearlySummaryRange;

    [Inject] public AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;

        var response = await ApiClient.GetAsync<YearlySummaryRange>("reports/yearly-summary/range");
        if (!response.IsSuccess)
            return;
        yearlySummaryRange = response.Result!;
        currentYear = Math.Max(yearlySummaryRange.LatestYear - 1, yearlySummaryRange.EarliestYear);

        isInitialized = true;

        if (!TryParseQuery(NavigationManager.Uri, out var year))
        {
            await UpdateYearlySummary();
            var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
            NavigationManager.NavigateTo(uriBuilder.Uri.ToString(), false, true);
        }
        else
            await SetYear(year);
    }

    async ValueTask UpdateYearlySummary()
    {
        if (yearlySummaryRange is null)
            return;
        var requestUri = $"reports/yearly-summary?year={currentYear:0000}";
        var response = await ApiClient.GetAsync<YearlySummary>(requestUri);
        if (response.IsSuccess)
            yearlySummary = response.Result;
    }

    void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (TryParseQuery(e.Location, out var year) && year != currentYear)
#pragma warning disable CS4014
            SetYear(year);
#pragma warning restore CS4014
    }

    async Task YearChanged(int value)
    {
        currentYear = value;
        await UpdateYearlySummary();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
    }

    string FormatQuery() =>
        $"year={currentYear:0000}";

    async Task SetYear(int year)
    {
        if (yearlySummaryRange!.EarliestYear <= year && year <= yearlySummaryRange.LatestYear)
            currentYear = year;
        if (currentYear != year)
        {
            var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
            NavigationManager.NavigateTo(uriBuilder.Uri.ToString(), false, true);
        }
        await UpdateYearlySummary();
        StateHasChanged();
    }

    static bool TryParseQuery(string location, out int year)
    {
        var query = QueryParser.GetQuery(location);
        string? queryYear = null;
        if (query.Contains("month"))
            queryYear = query["year"].FirstOrDefault();
        if (queryYear is null)
        {
            year = default;
            return false;
        }
        var match = yearRegex.Match(queryYear);
        if (!match.Success)
        {
            year = default;
            return false;
        }
        year = int.Parse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture);
        return true;
    }

    static string GetResourceName(ResourceType resourceType) =>
        resourceType switch
        {
            ResourceType.BanquetFacilities => "Festlokale",
            ResourceType.Bedroom => "Soveværelser",
            _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
        };
}
