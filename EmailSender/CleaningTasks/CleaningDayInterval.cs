using NodaTime;

namespace Frederikskaj2.Reservations.EmailSender;

record CleaningDayInterval(LocalDate Date, bool IsFirstDay, bool IsLastDay);