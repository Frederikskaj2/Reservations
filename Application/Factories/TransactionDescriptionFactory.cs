using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System;

namespace Frederikskaj2.Reservations.Application;

static class TransactionDescriptionFactory
{
    public static string? CreateDescription(IFormatter formatter, OrderId? orderId, TransactionDescription? description) =>
        description is not null ? CreateDescriptionCore(formatter, orderId!.Value, description) : null;

    static string CreateDescriptionCore(IFormatter formatter, OrderId orderId, TransactionDescription description) =>
        description.Type switch
        {
            TransactionDescriptionType.Cancellation =>
                $"Afbestilling {GetReservationDescriptions(formatter, description.Cancellation!.CancelledReservations)} (bestilling {orderId})",
            TransactionDescriptionType.Settlement =>
                $"Opgørelse {GetReservationDescription(formatter, description.Settlement!.Reservation)} (bestilling {orderId}){GetDamagesDescription(description.Settlement.Damages)}",
            TransactionDescriptionType.ReservationsUpdate =>
                $"Ændring {GetReservationDescriptions(formatter, description.ReservationsUpdate!.UpdatedReservations)} (bestilling {orderId})",
            _ => throw new ArgumentException($"Invalid transaction description type {description.Type}.", nameof(description))
        };

    static string GetReservationDescriptions(IFormatter formatter, Seq<ReservedDay> reservations) =>
        string.Join(", ", reservations.Map(reservation => GetReservationDescription(formatter, reservation)));

    static string GetReservationDescription(IFormatter formatter, ReservedDay reservation) =>
        $"{Resources.Name(reservation.ResourceId)} {formatter.FormatDateShort(reservation.Date)}";

    static string GetDamagesDescription(string? description) =>
        description switch
        {
            null => "",
            _ => $". {description}{(description.Length > 0 && description[^1] is not '.' ? "." : "")}"
        };
}
