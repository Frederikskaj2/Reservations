using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using NodaTime.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Frederikskaj2.Reservations.Emails.Messages;

public abstract record EmailModel(string To, string From, Uri FromUrl, CultureInfo CultureInfo)
{
    const char nonBreakingSpace = '\xA0';

    LocalDatePattern? longDatePattern;
    LocalDateTimePattern? longTimePattern;
    LocalDatePattern? monthPattern;
    LocalDatePattern? shortDatePattern;
    LocalDatePattern? sortableDatePattern;

    LocalDatePattern LongDatePattern => longDatePattern ??= LocalDatePattern.Create("dddd 'den' d. MMMM yyyy", CultureInfo);
    LocalDateTimePattern LongTimePattern => longTimePattern ??= LocalDateTimePattern.Create("dddd 'den' d. MMMM yyyy 'kl.' HH:mm", CultureInfo);
    LocalDatePattern MonthPattern => monthPattern ??= LocalDatePattern.Create("yyyy-MM", CultureInfo);
    LocalDatePattern ShortDatePattern => shortDatePattern ??= LocalDatePattern.Create("d. MMMM yyyy", CultureInfo);
    LocalDatePattern SortableDatePattern => sortableDatePattern ??= LocalDatePattern.Create("yyyy-MM-dd", CultureInfo);

    public string FormatAmount(Amount amount) => $"{amount.ToDecimal().ToString("N0", CultureInfo)}{nonBreakingSpace}kr.";

    public string FormatLongDate(LocalDate date) => LongDatePattern.Format(date);

    public string FormatLongTime(LocalDateTime dateTime)
    {
        var formatted = LongTimePattern.Format(dateTime);
        return CultureInfo.TextInfo.ToUpper(formatted[0]) + formatted[1..];
    }

    public string FormatMoney(Amount amount) => amount > Amount.Zero ? amount.ToDecimal().ToString("N0", CultureInfo) : "";

    public string FormatMonth(LocalDate date) => MonthPattern.Format(date);

    public string FormatDuration(Duration duration)
    {
        var stringBuilder = new StringBuilder();
        if (duration.Days > 0)
            stringBuilder.Append(CultureInfo, $"{duration.Days} dag{(duration.Days > 1 ? "e" : "")}");
        if (duration is { Days: > 0, Hours: > 0 })
            stringBuilder.Append(duration.Minutes > 0 ? ", " : " og ");
        if (duration.Hours > 0)
            stringBuilder.Append(CultureInfo, $"{duration.Hours} time{(duration.Hours > 1 ? "r" : "")}");
        if (duration.Minutes > 0 && (duration.Days > 0 || duration.Hours > 0))
            stringBuilder.Append(" og ");
        if (duration.Minutes > 0)
            stringBuilder.Append(CultureInfo, $"{duration.Minutes} minut{(duration.Minutes > 1 ? "ter" : "")}");
        return stringBuilder.ToString();
    }

    [SuppressMessage(
        "Performance", "CA1822:Mark members as static",
        Justification = "The member is accessed from a Razor template via a member.")]
    [SuppressMessage(
        "Sonar", "S2325:Methods and properties that don't access instance data should be static",
        Justification = "The member is accessed from a Razor template via a member.")]
    public string FormatPeriod(Period period) => Humanizer.FormatPeriod(period);

    public string FormatShortDate(LocalDate date) => ShortDatePattern.Format(date);

    public string FormatSortableDate(LocalDate date) => SortableDatePattern.Format(date);

    public Uri GetUrl(string path)
    {
        var builder = new UriBuilder(FromUrl);
        builder.Path += path;
        return builder.Uri;
    }

    public Uri GetResidentOrderUrl(OrderId orderId) => GetUrl($"{UrlPath.MyOrders}/{orderId}");

    public Uri GetOrderUrl(OrderId orderId) => GetUrl($"{UrlPath.Orders}/{orderId}");

    public Uri GetRulesUrl(ResourceId resourceId) =>
        GetUrl(Resources.GetResourceType(resourceId).Case switch
        {
            ResourceType.BanquetFacilities => UrlPath.RulesBanquetFacilities,
            ResourceType.Bedroom => UrlPath.RulesBedrooms,
            _ => throw new ArgumentException($"Invalid resource ID {resourceId}.", nameof(resourceId)),
        });
}

public record EmailModel<T>(T Data, string To, string From, Uri FromUrl, CultureInfo CultureInfo) : EmailModel(To, From, FromUrl, CultureInfo);
