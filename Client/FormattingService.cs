using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Frederikskaj2.Reservations.Shared;
using NodaTime;
using NodaTime.Text;

namespace Frederikskaj2.Reservations.Client
{
    public class FormattingService
    {
        private readonly CultureInfo cultureInfo;
        private readonly DateTimeZone dateTimeZone;
        private readonly LocalDatePattern longDatePattern;
        private readonly LocalDateTimePattern longTimePattern;
        private readonly ReservationsOptions options;
        private readonly LocalDateTimePattern shortTimePattern;

        public FormattingService(CultureInfo cultureInfo, DateTimeZone dateTimeZone, ReservationsOptions options)
        {
            this.cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            longTimePattern = LocalDateTimePattern.Create("dddd 'den' d. MMMM yyyy 'kl.' HH:mm", cultureInfo);
            shortTimePattern = LocalDateTimePattern.Create("d/M/yyyy HH:mm", cultureInfo);
            longDatePattern = LocalDatePattern.Create("d. MMMM yyyy", cultureInfo);
        }

        public string FormatMoneyLong(int value) => value.ToString("C0", cultureInfo);

        public string FormatMoneyShort(int value) => value.ToString("N0", cultureInfo);

        public string FormatCheckInTimeLong(LocalDate date) => FormatTimeLong(date + options.CheckInTime);

        public string FormatCheckOutTimeLong(LocalDate date) => FormatTimeLong(date + options.CheckOutTime);

        public string FormatCheckInTimeShort(LocalDate date) => FormatTimeShort(date + options.CheckInTime);

        public string FormatCheckOutTimeShort(LocalDate date) => FormatTimeShort(date + options.CheckOutTime);

        public string FormatDate(LocalDate date) => longDatePattern.Format(date);

        public string FormatDate(Instant instant) => longDatePattern.Format(instant.InZone(dateTimeZone).Date);

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This member is accessed through an injected instance.")]
        public string? FormatEmail(string? email) => email?.Replace("@", "\x00AD@", StringComparison.Ordinal);

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This member is accessed through an injected instance.")]
        public string? FormatPhone(string? phone) => phone?.Replace(' ', '\x00A0');

        private string FormatTimeLong(LocalDateTime time)
        {
            var formatted = longTimePattern.Format(time);
            return cultureInfo.TextInfo.ToUpper(formatted[0]) + formatted.Substring(1);
        }

        private string FormatTimeShort(LocalDateTime time) => shortTimePattern.Format(time);
    }
}