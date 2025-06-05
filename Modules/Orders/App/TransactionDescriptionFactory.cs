using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime.Text;
using System;
using System.Globalization;

namespace Frederikskaj2.Reservations.Orders;

static class TransactionDescriptionFactory
{
    static LocalDatePattern? datePattern;

    public static (OrderId? OrderId, string? Description) CreateDescription(CultureInfo cultureInfo, Transaction transaction) =>
        transaction.Description.Case switch
        {
            TransactionDescription description => CreateDescription(cultureInfo, description),
            _ => default,
        };

    static (OrderId? OrderId, string? Description) CreateDescription(CultureInfo cultureInfo, TransactionDescription description) =>
        description.Match(
            placeOrder => (placeOrder.OrderId, null),
            cancellation => GetCancellationDescription(cultureInfo, cancellation),
            settlement => GetSettlementDescription(cultureInfo, settlement),
            reservationsUpdate => GetReservationsUpdateDescription(cultureInfo, reservationsUpdate),
            reimbursement => ((OrderId?) null, (string?) reimbursement.Description),
            _ => throw new NotSupportedException());

    static (OrderId OrderId, string?) GetCancellationDescription(CultureInfo cultureInfo, Cancellation cancellation) =>
        (cancellation.OrderId,
            $"Afbestilling {GetReservationDescriptions(cultureInfo, cancellation.CancelledReservations)} (bestilling {cancellation.OrderId})");

    static (OrderId OrderId, string?) GetSettlementDescription(CultureInfo cultureInfo, Settlement settlement) =>
        (settlement.OrderId,
            (string?) ($"Opgørelse {GetReservationDescription(cultureInfo, settlement.Reservation)} (bestilling {settlement.OrderId})" +
                       $"{GetDamagesDescription(settlement.Damages)}"));

    static (OrderId OrderId, string?) GetReservationsUpdateDescription(CultureInfo cultureInfo, ReservationsUpdate reservationsUpdate) =>
        (reservationsUpdate.OrderId,
            (string?) $"Ændring {GetReservationDescriptions(cultureInfo, reservationsUpdate.UpdatedReservations)} (bestilling {reservationsUpdate.OrderId})");

    static string GetReservationDescriptions(CultureInfo cultureInfo, Seq<ReservedDay> reservations) =>
        string.Join(", ", reservations.Map(reservation => GetReservationDescription(cultureInfo, reservation)));

    static string GetReservationDescription(CultureInfo cultureInfo, ReservedDay reservation) =>
        $"{Resources.GetNameUnsafe(reservation.ResourceId)} {GetDatePattern(cultureInfo).Format(reservation.Date)}";

    static string GetDamagesDescription(Option<string> description) =>
        description.Case switch
        {
            string text => $". {description}{(text.Length > 0 && text[^1] is not '.' ? "." : "")}",
            _ => "",
        };

    static LocalDatePattern GetDatePattern(CultureInfo cultureInfo) =>
        datePattern ??= LocalDatePattern.Create("d'-'M'-'yyyy", cultureInfo);
}
