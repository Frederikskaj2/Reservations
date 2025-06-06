﻿using Frederikskaj2.Reservations.LockBox;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using static Frederikskaj2.Reservations.Orders.HighPricePolicy;
using static System.Linq.Enumerable;
using static System.Math;

namespace Frederikskaj2.Reservations.Orders;

public static class ReservationPolicies
{
    const int banquetFacilitiesHighPriceMinimumNights = 2;

    public static bool IsResourceAvailableToResident(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, IReadOnlySet<LocalDate> reservedDays, LocalDate today, LocalDate date,
        ResourceType resourceType) =>
        IsReservationDateWithinBounds(options, today, date) && IsReservationPossibleForResident(holidays, reservedDays, date, resourceType);

    public static bool IsResourceAvailableToOwner(OrderingOptions options, IEnumerable<LocalDate> reservedDays, LocalDate today, LocalDate date) =>
        IsReservationDateWithinBounds(options, today, date) && IsReservationPossibleForOwner(reservedDays, date);

    public static LocalDate GetReservationDateToAllowReservation(
        IReadOnlySet<LocalDate> holidays, IEnumerable<LocalDate> reservedDays, LocalDate date, ResourceType resourceType)
    {
        var minimumAllowedNights = GetMinimumAllowedNightsForResident(holidays, date, resourceType);
        var firstAllowedExtent = Range(0, minimumAllowedNights)
            .Select(i => new Extent(date.PlusDays(-i), minimumAllowedNights))
            .First(extent => reservedDays.All(reservedDate => !extent.Contains(reservedDate)));
        return firstAllowedExtent.Date;
    }

    public static bool IsReservationDateWithinBounds(OrderingOptions options, LocalDate today, LocalDate date)
    {
        var reservationsAreNotAllowedBefore = today.PlusDays(options.ReservationIsNotAllowedBeforeDaysFromNow);
        var reservationsAreNotAllowedAfter = today.PlusDays(options.ReservationIsNotAllowedAfterDaysFromNow);
        return reservationsAreNotAllowedBefore <= date && date <= reservationsAreNotAllowedAfter;
    }

    public static bool IsOwnerReservationDateWithinBounds(OrderingOptions options, LocalDate today, LocalDate date)
    {
        var reservationsAreNotAllowedBefore = today.PlusDays(1);
        var reservationsAreNotAllowedAfter = today.PlusDays(options.ReservationIsNotAllowedAfterDaysFromNow);
        return reservationsAreNotAllowedBefore <= date && date <= reservationsAreNotAllowedAfter;
    }

    public static bool IsResidentReservationDurationWithinBounds(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, Extent reservation, ResourceType resourceType) =>
        resourceType switch
        {
            ResourceType.Bedroom => IsStandardReservationDurationWithinBounds(options, reservation),
            ResourceType.BanquetFacilities => IsBanquetFacilitiesResidentReservationDurationWithinBounds(options, holidays, reservation),
            _ => throw new ArgumentException("Invalid resource type.", nameof(resourceType)),
        };

    public static bool IsOwnerReservationDurationWithinBounds(OrderingOptions options, Extent reservation, ResourceType resourceType) =>
        resourceType switch
        {
            ResourceType.Bedroom or ResourceType.BanquetFacilities => IsStandardReservationDurationWithinBounds(options, reservation),
            _ => throw new ArgumentException("Invalid resource type.", nameof(resourceType)),
        };

    public static (int MinimumNights, int MaximumNights) GetResidentAllowedNights(
        OrderingOptions options, IReadOnlySet<LocalDate> holidays, IEnumerable<LocalDate> reservedDays, LocalDate date, ResourceType resourceType) =>
        (GetMinimumAllowedNightsForResident(holidays, date, resourceType), GetMaximumAllowedNights(options, reservedDays, date));

    public static (int MinimumNights, int MaximumNights) GetOwnerAllowedNights(
        OrderingOptions options, IEnumerable<LocalDate> reservedDays, LocalDate date) =>
        (1, GetMaximumAllowedNights(options, reservedDays, date));

    public static bool CanReservationBeCancelled(
        OrderingOptions options, LocalDate date, ReservationStatus status, Extent extent, bool alwaysAllowCancellation) =>
        CanReservationBeCancelledByAdministrator(date, status, extent, alwaysAllowCancellation) ||
        CanReservationBeCancelledByResident(options, date, status, extent);

    static bool CanReservationBeCancelledByAdministrator(LocalDate date, ReservationStatus status, Extent extent, bool alwaysAllowCancellation) =>
        date < extent.Ends() && alwaysAllowCancellation && status is ReservationStatus.Reserved or ReservationStatus.Confirmed;

    static bool CanReservationBeCancelledByResident(OrderingOptions options, LocalDate date, ReservationStatus status, Extent extent) =>
        status is ReservationStatus.Reserved ||
        status is ReservationStatus.Confirmed && date.PlusDays(options.MinimumCancellationNoticeInDays) <= extent.Date;

    public static bool CanOwnerReservationBeCancelled(LocalDate date, ReservationStatus status, Extent extent) =>
        status == ReservationStatus.Confirmed && date < extent.Ends();

    static bool IsReservationPossibleForResident(
        IReadOnlySet<LocalDate> holidays, IEnumerable<LocalDate> reservedDays, LocalDate date, ResourceType resourceType) =>
        IsReservationPossibleForResident(reservedDays, date, GetMinimumAllowedNightsForResident(holidays, date, resourceType));

    static bool IsReservationPossibleForResident(IEnumerable<LocalDate> reservedDays, LocalDate date, int minimumAllowedNights) =>
        Range(0, minimumAllowedNights)
            .Select(i => new Extent(date.PlusDays(-i), minimumAllowedNights))
            .Any(extent => reservedDays.All(reservedDate => !extent.Contains(reservedDate)));

    static bool IsReservationPossibleForOwner(IEnumerable<LocalDate> reservedDays, LocalDate date)
    {
        const int minimumAllowedNights = 1;
        return Range(0, minimumAllowedNights)
            .Select(i => new Extent(date.PlusDays(-i), minimumAllowedNights))
            .Any(extent => reservedDays.All(reservedDate => !extent.Contains(reservedDate)));
    }

    static bool IsStandardReservationDurationWithinBounds(OrderingOptions options, Extent reservation)
    {
        const int minimumNights = 1;
        var maximumNights = options.MaximumAllowedReservationNights;
        return minimumNights <= reservation.Nights && reservation.Nights <= maximumNights;
    }

    static bool IsBanquetFacilitiesResidentReservationDurationWithinBounds(OrderingOptions options, IReadOnlySet<LocalDate> holidays, Extent reservation)
    {
        var minimumNights = GetBanquetFacilitiesResidentMinimumNights(holidays, reservation);
        var maximumNights = options.MaximumAllowedReservationNights;
        return minimumNights <= reservation.Nights && reservation.Nights <= maximumNights;
    }

    static int GetBanquetFacilitiesResidentMinimumNights(IReadOnlySet<LocalDate> holidays, Extent reservation)
    {
        var isHighPriceDayGovernedByMinimumRule = Range(0, reservation.Nights)
            .Select(i => reservation.Date.PlusDays(i))
            .Any(day => IsHighPriceDayGovernedByMinimumRule(holidays, day));
        return isHighPriceDayGovernedByMinimumRule ? banquetFacilitiesHighPriceMinimumNights : 1;
    }

    static int GetMinimumAllowedNightsForResident(IReadOnlySet<LocalDate> holidays, LocalDate date, ResourceType resourceType) =>
        resourceType switch
        {
            ResourceType.Bedroom => 1,
            ResourceType.BanquetFacilities => IsHighPriceDayGovernedByMinimumRule(holidays, date) ? banquetFacilitiesHighPriceMinimumNights : 1,
            _ => throw new ArgumentException("Invalid resource type.", nameof(resourceType)),
        };

    static bool IsHighPriceDayGovernedByMinimumRule(IReadOnlySet<LocalDate> holidays, LocalDate date) =>
        IsHighPriceDay(date, holidays) && IsHighPriceDay(date.PlusDays(1), holidays);

    static int GetMaximumAllowedNights(OrderingOptions options, IEnumerable<LocalDate> reservedDays, LocalDate date) =>
        GetMaximumAllowedNights(
            options,
            date,
            reservedDays.Order().FirstOrDefault(reservedDay => reservedDay > date));

    static int GetMaximumAllowedNights(OrderingOptions options, LocalDate date, LocalDate nextReservedDay) =>
        nextReservedDay != default
            ? Min((nextReservedDay - date).Days, options.MaximumAllowedReservationNights)
            : options.MaximumAllowedReservationNights;
}
