using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UserReservedDaysCommand : ReservedDaysCommand
{
    public UserReservedDaysCommand(Option<LocalDate> fromDate, Option<LocalDate> toDate, UserId userId) : base(fromDate, toDate) =>
        UserId = userId;

    public UserId UserId { get; }
}
