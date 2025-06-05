using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record GetBankTransactionsQuery(
    Option<LocalDate> StartDate,
    Option<LocalDate> EndDate,
    bool IncludeUnknown,
    bool IncludeIgnored,
    bool IncludeReconciled);
