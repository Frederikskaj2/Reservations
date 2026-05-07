using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

[Authorize(Roles = nameof(Roles.Bookkeeping))]
public partial class PostingsPage
{
    static readonly Encoding encoding = Encoding.GetEncoding("iso-8859-1");
    IReadOnlyDictionary<PostingAccount, string>? accountNames;
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    int fromMonth;
    int fromYear;
    bool isInitialized;
    LocalDatePattern? monthPattern;
    IEnumerable<PostingDto>? postings;
    MonthRange? postingsRange;
    int toMonth;
    int toYear;

    [Inject] AuthenticatedApiClient ApiClient { get; set; } = null!;
    [Inject] ClientDataProvider DataProvider { get; set; } = null!;
    [Inject] CultureInfo CultureInfo { get; set; } = null!;
    [Inject] Formatter Formatter { get; set; } = null!;
    [Inject] IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        monthPattern = LocalDatePattern.Create("MMMM", CultureInfo);

        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;

        apartments = await DataProvider.GetApartments();
        if (apartments is not null)
        {
            accountNames = await DataProvider.GetAccountNames();
            if (accountNames is not null)
                await InitializeCalendar();
        }

        isInitialized = true;

        if (!TryParseQuery(NavigationManager.Uri, out var tuple))
        {
            await UpdatePostings();
            var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
            NavigationManager.NavigateTo(uriBuilder.Uri.ToString(), forceLoad: false, replace: true);
        }
        else
            await SetPeriod(tuple.From, tuple.To);
    }

    void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (TryParseQuery(e.Location, out var tuple) && (tuple.From.Year != fromYear || tuple.From.Month != fromMonth ||
                                                         tuple.To.Year != toYear || tuple.To.Month != toMonth))
            _ = SetPeriod(tuple.From, tuple.To);
    }

    async ValueTask InitializeCalendar()
    {
        var response = await ApiClient.Get<GetPostingsRangeResponse>("postings/range");
        if (!response.IsSuccess)
            return;
        postingsRange = response.Result!.MonthRange;
        fromYear = toYear = postingsRange.LatestMonth.Year;
        fromMonth = toMonth = postingsRange.LatestMonth.Month;
    }

    async ValueTask UpdatePostings()
    {
        if (postingsRange is null)
            return;
        var requestUri = $"postings?from={fromYear:0000}-{fromMonth:00}-01&to={toYear:0000}-{toMonth:00}-01";
        var response = await ApiClient.Get<GetPostingsResponse>(requestUri);
        if (response.IsSuccess)
            postings = response.Result!.Postings;
    }

    async Task FromYearChanged(int value)
    {
        if (fromYear == value)
            return;
        fromMonth = value > fromYear ? 1 : 12;
        fromYear = value;
        await UpdatePostings();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
    }

    async Task FromMonthChanged(int value)
    {
        if (fromMonth == value)
            return;
        fromMonth = value;
        await UpdatePostings();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
    }

    async Task ToYearChanged(int value)
    {
        if (toYear == value)
            return;
        toMonth = value > toYear ? 1 : 12;
        toYear = value;
        await UpdatePostings();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
    }

    async Task ToMonthChanged(int value)
    {
        if (toMonth == value)
            return;
        toMonth = value;
        await UpdatePostings();
        var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
        NavigationManager.NavigateTo(uriBuilder.Uri.ToString());
    }

    string Capitalize(string text) => CultureInfo.TextInfo.ToUpper(text[0]) + text[1..];

    string FormatQuery() =>
        $"fra={fromYear:0000}-{fromMonth:00}&til={toYear:0000}-{toMonth:00}";

    async Task SetPeriod(LocalDate from, LocalDate to)
    {
        if (from <= to && from >= postingsRange!.EarliestMonth && to <= postingsRange.LatestMonth)
        {
            fromYear = from.Year;
            fromMonth = from.Month;
            toYear = to.Year;
            toMonth = to.Month;
        }
        if (fromYear != from.Year || fromMonth != from.Month || toYear != to.Year || toMonth != to.Month)
        {
            var uriBuilder = new UriBuilder(NavigationManager.Uri) { Query = FormatQuery() };
            NavigationManager.NavigateTo(uriBuilder.Uri.ToString(), forceLoad: false, replace: true);
        }
        await UpdatePostings();
        StateHasChanged();
    }

    static bool TryParseQuery(string location, out (LocalDate From, LocalDate To) tuple)
    {
        var query = QueryParser.GetQuery(location);
        if (!TryParseMonth(query, "fra", out var from) || !TryParseMonth(query, "til", out var to))
        {
            tuple = default;
            return false;
        }
        tuple = (from, to);
        return true;
    }

    static bool TryParseMonth(ILookup<string, string> query, string name, out LocalDate month)
    {
        string? value = null;
        if (query.Contains(name))
            value = query[name].FirstOrDefault();
        if (value is null)
        {
            month = default;
            return false;
        }
        var match = MonthRegex.Match(value);
        if (!match.Success)
        {
            month = default;
            return false;
        }
        month = new(
            int.Parse(match.Groups["year"].Value, NumberStyles.None, CultureInfo.InvariantCulture),
            int.Parse(match.Groups["month"].Value, NumberStyles.None, CultureInfo.InvariantCulture),
            1);
        return true;
    }

    async Task DownloadPostings()
    {
        var stream = GetCsvFileStream();
        var fileName = $"Posteringer-{fromYear:0000}{fromMonth:00}-{toYear:0000}{toMonth:00}";
        using var streamReference = new DotNetStreamReference(stream);
        await JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, MediaTypeNames.Text.Csv, streamReference);
    }

    MemoryStream GetCsvFileStream()
    {
        var datePattern = LocalDatePattern.Create("yyyy-MM-dd", CultureInfo);
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, encoding, leaveOpen: true);
        writer.WriteLine("Nummer;Dato;Navn;Identifikation;Adresse;Bolig;Beskrivelse;Konto;Debet;Kredit");
        foreach (var posting in postings!)
        {
            var apartment = apartments!.GetValueOrDefault(posting.ApartmentId);
            var occupancy = apartment?.OccupancyType switch
            {
                OccupancyType.Tenant => "Lejer",
                OccupancyType.Owner => "Ejer",
                OccupancyType.Houseboat => "Husbåd",
                _ => "Ukendt",
            };
            var description = posting.Activity switch
            {
                Activity.PlaceOrder => $"Oprettelse af bestilling {posting.OrderId}",
                Activity.UpdateOrder => $"Ændring af bestilling {posting.OrderId}",
                Activity.SettleReservation => $"Opgørelse af reservation på bestilling {posting.OrderId}",
                Activity.PayIn => "Indbetaling",
                Activity.PayOut => "Udbetaling",
                Activity.Reimburse => "Godtgørelse",
                _ => "Ukendt",
            };
            foreach (var (account, amount) in posting.Amounts)
            {
                writer.Write($"{posting.TransactionId};");
                writer.Write($"{datePattern.Format(posting.Date)};");
                writer.Write($"{posting.FullName};");
                writer.Write($"{posting.PaymentId};");
                writer.Write($"{apartment?.ToString() ?? "Ukendt"};");
                writer.Write($"{occupancy};");
                writer.Write($"{description};");
                writer.Write($"{accountNames![account]};");
                writer.Write(amount > Amount.Zero ? $"{amount};" : ";");
                writer.WriteLine(amount < Amount.Zero ? $"{-amount};" : ";");
            }
        }
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [GeneratedRegex(@"^(?<year>\d{4})-(?<month>\d{2})$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    static partial Regex MonthRegex { get; }
}
