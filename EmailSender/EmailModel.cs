using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using NodaTime.Text;
using System;
using System.Globalization;

namespace Frederikskaj2.Reservations.EmailSender;

public record EmailModel<T>(T Data, string From, Uri FromUrl, CultureInfo CultureInfo)
{
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

    public string FormatAmount(Amount amount) => amount.ToInt32().ToString("C0", CultureInfo);

    public string FormatLongDate(LocalDate date) => LongDatePattern.Format(date);

    public string FormatLongTime(LocalDateTime dateTime)
    {
        var formatted = LongTimePattern.Format(dateTime);
        return CultureInfo.TextInfo.ToUpper(formatted[0]) + formatted[1..];
    }

    public string FormatMoney(Amount amount) => amount > Amount.Zero ? amount.ToInt32().ToString("N0", CultureInfo) : "";

    public string FormatMonth(LocalDate date) => MonthPattern.Format(date);

    public string FormatPeriod(Period period) => Humanizer.FormatPeriod(period);

    public string FormatShortDate(LocalDate date) => ShortDatePattern.Format(date);

    public string FormatSortableDate(LocalDate date) => SortableDatePattern.Format(date);

    public Uri GetUrl(string path)
    {
        var builder = new UriBuilder(FromUrl);
        builder.Path += path;
        return builder.Uri;
    }
}
