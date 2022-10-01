using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using NodaTime.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Frederikskaj2.Reservations.Client;

public class Formatter
{
    const char noBreakSpace = '\x00A0';
    const char softHyphen = '\x00AD';
    readonly CultureInfo cultureInfo;
    readonly DateTimeZone dateTimeZone;
    readonly LocalDatePattern longDatePattern;
    readonly LocalDateTimePattern longTimePattern;
    readonly LocalDateTimePattern shortTimePattern;

    public Formatter(CultureInfo cultureInfo, DateTimeZone dateTimeZone)
    {
        this.cultureInfo = cultureInfo;
        this.dateTimeZone = dateTimeZone;
        longTimePattern = LocalDateTimePattern.Create("dddd 'den' d. MMMM yyyy 'kl.' HH':'mm", cultureInfo);
        shortTimePattern = LocalDateTimePattern.Create("d'-'M'-'yyyy HH':'mm", cultureInfo);
        longDatePattern = LocalDatePattern.Create("d. MMMM yyyy", cultureInfo);
    }

    public string FormatMoneyLong(Amount value) => value.ToString("C0", cultureInfo);

    public string FormatMoneyShort(Amount value) => value.ToString("N0", cultureInfo);

    public string FormatCheckInTimeLong(OrderingOptions options, LocalDate date) => FormatTimeLong(date + options.CheckInTime);

    public string FormatCheckOutTimeLong(OrderingOptions options, LocalDate date) => FormatTimeLong(date + options.CheckOutTime);

    public string FormatCheckInTimeShort(OrderingOptions options, LocalDate date) => FormatTimeShort(date + options.CheckInTime);

    public string FormatCheckOutTimeShort(OrderingOptions options, LocalDate date) => FormatTimeShort(date + options.CheckOutTime);

    public string FormatDate(LocalDate date) => longDatePattern.Format(date);

    public string FormatDate(Instant instant) => longDatePattern.Format(instant.InZone(dateTimeZone).Date);

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This member is accessed through an injected instance.")]
    public string? FormatEmail(EmailAddress email) => email.ToString().Replace("@", softHyphen + "@", StringComparison.Ordinal);

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This member is accessed through an injected instance.")]
    public string? FormatPhone(string? phone) => phone?.Replace(' ', noBreakSpace);

    public string FormatTimeLong(LocalDateTime time)
    {
        var formatted = longTimePattern.Format(time);
        return cultureInfo.TextInfo.ToUpper(formatted[0]) + formatted[1..];
    }

    string FormatTimeShort(LocalDateTime time) => shortTimePattern.Format(time);

    public string FormatHumanizedFutureTime(Instant now, Instant future)
    {
        var duration = future - now;
        return duration switch
        {
            { TotalSeconds: < 120 } => "det næste minut",
            { TotalMinutes: < 60 } => $"de næste {GetNumber(duration.Minutes)} minutter",
            { TotalMinutes: < 90 } => "den næste time",
            { TotalMinutes: < 120 } => "den næste halvanden time",
            { TotalHours: < 24 } => $"de næste {GetNumber(duration.Hours)} timer",
            { TotalHours: < 48 } => (future.InZone(dateTimeZone).Date - now.InZone(dateTimeZone).Date).Days is 1 ? $"senest i morgen inden kl. {future.InZone(dateTimeZone).Date}" : "de næste to dage",
            _ => $"de næste {GetNumber(duration.Days + (duration.TotalDays - duration.Days > 0.5 ? 1 : 0))} dage"
        };
    }

    public string FormatHumanizedPastTime(Instant now, Instant past)
    {
        var duration = now - past;
        return duration switch
        {
            { TotalSeconds: < 120 } => "For et minut siden",
            { TotalMinutes: < 60 } => $"For {GetNumber(duration.Minutes)} minutter siden",
            { TotalMinutes: < 90 } => "For en time siden",
            { TotalMinutes: < 120 } => "For halvanden time siden",
            { TotalHours: < 24 } => $"For {GetNumber(duration.Hours)} timer siden",
            { TotalHours: < 48 } => (past.InZone(dateTimeZone).Date - now.InZone(dateTimeZone).Date).Days is 1 ? "I dag" : "I går",
            { TotalDays: < 7 } => $"For {GetNumber(duration.Days)} dage siden",
            { TotalDays: < 10 } => $"For en uge siden",
            { TotalDays: < 14 } => $"For halvanden uge siden",
            { TotalDays: < 31 } => $"For {GetNumber(duration.Days/7)} uger siden",
            _ => FormatHumanizedPastTimeWhenMoreThanOneMonth(now, past)
        };
    }

    string FormatHumanizedPastTimeWhenMoreThanOneMonth(Instant now, Instant past)
    {
        var nowDate = now.InZone(dateTimeZone).Date;
        var pastDate = past.InZone(dateTimeZone).Date;
        var period = Period.Between(pastDate, nowDate, PeriodUnits.Months);
        if (nowDate.Year == pastDate.Year || period.Months <= 6)
            return period.Months == 1 ? "For en måned siden" : $"For {period.Months} måneder siden";
        return nowDate.Year - 1 == pastDate.Year ? "Sidste år" : $"I {pastDate.Year}";
    }

    string GetNumber(int number) =>
        number switch
        {
            2 => "to",
            3 => "tre",
            4 => "fire",
            5 => "fem",
            6 => "seks",
            7 => "syv",
            8 => "otte",
            9 => "ni",
            10 => "ti",
            _ => number.ToString(cultureInfo)
        };

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This member is accessed through an injected instance.")]
    public string FormatHumanizedPeriod(Period period) => Humanizer.FormatPeriod(period);
}
