using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Orders.ReservationPolicies;

namespace Frederikskaj2.Reservations.Orders;

public abstract class ReservationValidator
{
    public abstract bool IsDateWithinBounds(LocalDate today, LocalDate reservationDate);
    public abstract bool IsDurationWithinBounds(ReservationModel reservation);

    public static ReservationValidator ValidateByResident(IReadOnlySet<LocalDate> holidays, OrderingOptions options) =>
        new ResidentReservationValidator(holidays, options);

    public static ReservationValidator ValidateByAdministrator(OrderingOptions options) =>
        new AdministratorReservationValidator(options);

    class ResidentReservationValidator(IReadOnlySet<LocalDate> holidays, OrderingOptions options) : ReservationValidator
    {
        public override bool IsDateWithinBounds(LocalDate today, LocalDate reservationDate) =>
            IsReservationDateWithinBounds(options, today, reservationDate);

        public override bool IsDurationWithinBounds(ReservationModel reservation) =>
            IsResidentReservationDurationWithinBounds(options, holidays, reservation.Extent, reservation.ResourceType);
    }

    class AdministratorReservationValidator(OrderingOptions options) : ReservationValidator
    {
        public override bool IsDateWithinBounds(LocalDate today, LocalDate reservationDate) =>
            IsOwnerReservationDateWithinBounds(options, today, reservationDate);

        public override bool IsDurationWithinBounds(ReservationModel reservation) =>
            IsOwnerReservationDurationWithinBounds(options, reservation.Extent, reservation.ResourceType);
    }
}
