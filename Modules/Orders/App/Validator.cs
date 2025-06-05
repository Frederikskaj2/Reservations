using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using static Frederikskaj2.Reservations.Core.Validator;
using static Frederikskaj2.Reservations.Users.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

public static class Validator
{
    public static Either<Failure<Unit>, GetOrdersQuery> ValidateGetOrders(LocalDate today, IEnumerable<OrderType> orderTypes, OrderId[] orderIds) =>
        orderTypes.Any() && orderIds.Length > 0
            ? Left(Failure.New(HttpStatusCode.UnprocessableEntity, "Specifying both order types and order IDs are not supported."))
            : orderIds.Length > 0
                ? ValidateGetOrders(today, orderIds)
                : ValidateGetOrders(today, orderTypes);

    static Either<Failure<Unit>, GetOrdersQuery> ValidateGetOrders(LocalDate today, OrderId[] orderIds) =>
        new GetOrdersQuery(today, false, false, orderIds.ToSeq());

    static Either<Failure<Unit>, GetOrdersQuery> ValidateGetOrders(LocalDate today, IEnumerable<OrderType> orderTypes) =>
        from query in orderTypes.Fold(Right<Failure<Unit>, GetOrdersQuery>(new(today, false, false, [])), GetOrdersQueryFolder)
        from _ in NotAnEmptyQuery(query)
        select query;

    static Either<Failure<Unit>, GetOrdersQuery> GetOrdersQueryFolder(Either<Failure<Unit>, GetOrdersQuery> either, OrderType orderType) =>
        orderType switch
        {
            OrderType.Resident => either.Map(query => query with { IncludeResidentOrders = true }),
            OrderType.Owner => either.Map(query => query with { IncludeOwnerOrders = true }),
            _ => Failure.New(HttpStatusCode.BadRequest, $"Invalid order type '{orderType}'."),
        };

    static Either<Failure<Unit>, GetOrdersQuery> NotAnEmptyQuery(GetOrdersQuery query) =>
        query.IncludeResidentOrders || query.IncludeOwnerOrders ? query : Failure.New(HttpStatusCode.UnprocessableEntity, "Missing order type.");

    public static Either<Failure<Unit>, PlaceMyOrderCommand> ValidatePlaceMyOrder(Instant now, PlaceMyOrderRequest request, UserId userId)
    {
        var either =
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from apartmentId in ValidateApartmentId(request.ApartmentId)
            from accountNumber in ValidateAccountNumber(request.AccountNumber)
            from reservations in ValidateReservations(request.Reservations.ToSeq())
            select new PlaceMyOrderCommand(now, userId, fullName, phone, apartmentId, accountNumber, reservations);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, PlaceResidentOrderCommand> ValidatePlaceResidentOrder(
        Instant now, PlaceResidentOrderRequest request, UserId createdByUserId)
    {
        var either =
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from apartmentId in ValidateApartmentId(request.ApartmentId)
            from accountNumber in ValidateAccountNumber(request.AccountNumber)
            from reservations in ValidateReservations(request.Reservations.ToSeq())
            select new PlaceResidentOrderCommand(now, createdByUserId, request.UserId, fullName, phone, apartmentId, accountNumber, reservations);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, PlaceOwnerOrderCommand> ValidatePlaceOwnerOrder(Instant now, PlaceOwnerOrderRequest request, UserId userId)
    {
        var either =
            from description in ValidateOwnerOrderDescription(request.Description)
            from reservations in ValidateReservations(request.Reservations.ToSeq())
            select new PlaceOwnerOrderCommand(now, userId, description, reservations, request.IsCleaningRequired);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, UpdateMyOrderCommand> ValidateUpdateMyOrder(Instant now, OrderId orderId, UpdateMyOrderRequest request, UserId userId)
    {
        var either =
            from accountNumber in ValidateOptionalAccountNumber(request.AccountNumber)
            select new UpdateMyOrderCommand(
                now,
                userId,
                orderId,
                accountNumber,
                toHashSet(request.CancelledReservations ?? ReadOnlySet<ReservationIndex>.Empty));
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, UpdateOwnerOrderCommand> ValidateUpdateOwnerOrder(
        Instant now, OrderId orderId, UpdateOwnerOrderRequest request, UserId userId)
    {
        var either =
            from description in ValidateOptionalOwnerOrderDescription(request.Description)
            select new UpdateOwnerOrderCommand(
                now,
                userId,
                orderId,
                description,
                toHashSet(request.CancelledReservations ?? ReadOnlySet<ReservationIndex>.Empty),
                request.IsCleaningRequired.ToOption());
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, UpdateResidentOrderCommand> ValidateUpdateResidentOrder(
        Instant now, OrderId orderId, UpdateResidentOrderRequest request, UserId updatedByUserId)
    {
        var either =
            from accountNumber in ValidateOptionalAccountNumber(request.AccountNumber)
            select new UpdateResidentOrderCommand(
                now,
                updatedByUserId,
                orderId,
                accountNumber,
                toHashSet(request.CancelledReservations ?? []),
                request.WaiveFee,
                request.AllowCancellationWithoutFee);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, UpdateResidentReservationsCommand> ValidateUpdateResidentReservations(
        Instant timestamp, OrderId orderId, UpdateResidentReservationsRequest request, UserId userId)
    {
        var either =
            from reservations in ValidateReservationUpdates(request.Reservations.ToSeq())
            select new UpdateResidentReservationsCommand(
                timestamp,
                userId,
                orderId,
                reservations);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<Unit>, ReimburseCommand> ValidateReimburse(
        ITimeConverter timeConverter, Instant timestamp, UserId userId, ReimburseRequest request, UserId administratorUserId)
    {
        var either =
            from date in IsNotFutureDate(timeConverter.GetDate(timestamp), request.Date, "Date")
            from description in ValidateReimburseDescription(request.Description)
            from account in IsValidEnumValue(request.AccountToDebit, "Account to debit")
            from amount in ValidateAmount(request.Amount, ValidationRule.MinimumAmount, ValidationRule.MaximumAmount, "Amount")
            select new ReimburseCommand(timestamp, administratorUserId, userId, date, account, description, amount);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, Option<string>> ValidateOptionalAccountNumber(string? accountNumber) =>
        accountNumber is null ? Option<string>.None : Some(ValidateAccountNumber(accountNumber)).Sequence();

    static Either<string, string> ValidateAccountNumber(string? accountNumber) =>
        from accountNumberNotNull in IsNotNullOrEmpty(accountNumber, "Account number")
        let trimmedAccountNumber = accountNumberNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedAccountNumber, ValidationRule.MaximumAccountNumberLength, "Account number")
        from _2 in IsMatching(trimmedAccountNumber, ValidationRule.AccountNumberRegex, "Account number")
        select trimmedAccountNumber;

    static Either<string, Seq<ReservationModel>> ValidateReservations(Seq<ReservationRequest> reservations) =>
        from seq in IsBoundedCollection(reservations, 1, ValidationRule.MaximumReservationsPerOrder, "Reservations")
        from models in ValidateAll(seq, ValidateReservation)
        from _ in ValidateNoConflicts(GetAllSubsets(GetReservationExtents(seq)))
        select models;

    static Either<string, Seq<ReservationUpdate>> ValidateReservationUpdates(Seq<ReservationUpdateRequest> reservations) =>
        from seq in IsBoundedCollection(reservations, 1, ValidationRule.MaximumReservationsPerOrder, "Reservations")
        from _ in ValidateNoConflicts(GetAllSubsets(GetReservationExtents(seq)))
        from models in ValidateAll(seq, ValidateReservationUpdate)
        select models;

    static Seq<ReservationExtent<ResourceId>> GetReservationExtents(Seq<ReservationRequest> seq) =>
        seq.Map(reservationRequest => ReservationExtent.Create(reservationRequest.ResourceId, reservationRequest.Extent));

    static Seq<ReservationExtent<ReservationIndex>> GetReservationExtents(Seq<ReservationUpdateRequest> seq) =>
        seq.Map(reservationUpdateRequest => ReservationExtent.Create(reservationUpdateRequest.ReservationIndex, reservationUpdateRequest.Extent));

    static Either<string, ReservationUpdate> ValidateReservationUpdate(ReservationUpdateRequest request) =>
        from _ in IsNotLessThan(request.Extent.Nights, 1, "Nights")
        select new ReservationUpdate(request.ReservationIndex, request.Extent);

    static Either<string, Option<string>> ValidateOptionalOwnerOrderDescription(string? description) =>
        description is null ? Option<string>.None : Some(ValidateOwnerOrderDescription(description)).Sequence();

    static Either<string, string> ValidateOwnerOrderDescription(string? description) =>
        from descriptionNotNull in IsNotNullOrEmpty(description, "Description")
        let trimmedDescription = descriptionNotNull.Trim()
        from _ in IsNotLongerThan(trimmedDescription, ValidationRule.MaximumOwnerOrderDescriptionLength, "Description")
        select trimmedDescription;

    public static Either<Failure<Unit>, SettleReservationCommand> ValidateSettleReservation(
        Instant timestamp, OrderId orderId, SettleReservationRequest request, UserId userId)
    {
        var either =
            from amount in ValidateAmount(request.Damages, Amount.Zero, ValidationRule.MaximumAmount, "Damages")
            from description in ValidateDamagesDescription(request.Damages, request.Description)
            select new SettleReservationCommand(timestamp, userId, orderId, request.ReservationIndex, amount, description);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<string, Amount> ValidateAmount(Amount amount, Amount minimumAmount, Amount maximumAmount, string context)
    {
        if (amount < minimumAmount)
            return $"{context} is too small.";
        if (amount > maximumAmount)
            return $"{context} is too large.";
        return amount;
    }

    public static Either<Failure<Unit>, GetYearlySummaryQuery> ValidateGetYearlySummary(int year)
    {
        var either =
            from validYear in IsValidYear(year, "Year")
            select new GetYearlySummaryQuery(validYear);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, Option<string>> ValidateDamagesDescription(Amount damages, string? description) =>
        damages > Amount.Zero ? IsValidDamagesDescription(description) : Option<string>.None;

    static Either<string, Option<string>> IsValidDamagesDescription(string? description) =>
        from nonEmptyString in IsNotNullOrEmpty(description, "Description")
        from validDescription in IsNotLongerThan(nonEmptyString, ValidationRule.MaximumDamagesDescriptionLength, "Description")
        select Some(validDescription);

    static Either<string, Seq<T>> IsBoundedCollection<T>(IEnumerable<T> collection, int minimumItems, int maximumItems, string context)
    {
        var seq = collection.ToSeq();
        if (seq.Count < minimumItems)
            return $"{context} has too few items.";
        if (seq.Count > maximumItems)
            return $"{context} has too many items.";
        return seq;
    }

    static Either<string, Seq<TResult>> ValidateAll<TSource, TResult>(Seq<TSource> seq, Func<TSource, Either<string, TResult>> validator) =>
        seq.Map(validator).Sequence();

    static Either<string, ReservationModel> ValidateReservation(ReservationRequest request) =>
        from resourceType in ValidateResourceId(request.ResourceId)
        from _ in IsNotLessThan(request.Extent.Nights, 1, "Nights")
        select new ReservationModel(request.ResourceId, resourceType, request.Extent);

    static Either<string, ResourceType> ValidateResourceId(ResourceId resourceId) =>
        Resources.GetResourceType(resourceId).Case switch
        {
            ResourceType resourceType => resourceType,
            _ => "Resource ID is invalid.",
        };

    static Either<string, LocalDate> IsNotFutureDate(LocalDate today, LocalDate date, string context) =>
        date <= today ? date : $"{context} is in the future.";

    static Either<string, string> ValidateReimburseDescription(string? description) =>
        from nonEmptyString in IsNotNullOrEmpty(description, "Description")
        from validDescription in IsNotLongerThan(nonEmptyString, ValidationRule.MaximumReimburseDescriptionLength, "Description")
        select validDescription;

    static Either<string, T> IsValidEnumValue<T>(T value, string context) where T : struct, Enum =>
        !Equals(value, default(T)) && Enum.IsDefined(value) ? value : $"{context} is not from the set of valid values.";

    static Either<string, int> IsNotLessThan(int value, int lowerBound, string context) =>
        value >= lowerBound ? value : $"{context} is too small.";

    static Either<string, int> IsValidYear(int year, string context) =>
        year is >= 1 and <= 9999 ? year : $"{context} is not a valid year.";

    static Either<string, Seq<Unit>> ValidateNoConflicts<T>(Seq<Seq<ReservationExtent<T>>> combinations) =>
        combinations.Map(ValidateNoConflicts).Sequence();

    static Either<string, Unit> ValidateNoConflicts<T>(Seq<ReservationExtent<T>> reservations) =>
        reservations.Match(() => unit, (head, tail) => ValidateNoConflicts(head, tail));

    static Either<string, Unit> ValidateNoConflicts<T>(ReservationExtent<T> reservation, Seq<ReservationExtent<T>> otherReservations) =>
        otherReservations
            .Any(otherReservation => Equals(reservation.Key, otherReservation.Key) && reservation.Extent.Overlaps(otherReservation.Extent))
            ? "Two or more reservations are in conflict."
            : unit;

    static Seq<Seq<T>> GetAllSubsets<T>(Seq<T> items) =>
        items.Match(
            () => Empty,
            seq => seq.Cons(GetAllSubsets(seq.Tail)));
}
