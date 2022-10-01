using System;
using System.Net;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using NodaTime;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

public static partial class Validator
{
    static readonly System.Collections.Generic.HashSet<ReservationIndex> noReservations = new();

    public static Either<Failure, PlaceMyOrderCommand> ValidatePlaceUserOrder(
        Func<ApartmentId, bool> isValidApartmentId, Func<ResourceId, Option<ResourceType>> getResourceType, Func<Instant> getTimestamp,
        PlaceMyOrderRequest request, UserId userId)
    {
        var either =
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from apartmentId in ValidateApartmentId(isValidApartmentId, request.ApartmentId)
            from accountNumber in ValidateAccountNumber(request.AccountNumber)
            from reservations in ValidateReservations(getResourceType, request.Reservations.ToSeq())
            select new PlaceMyOrderCommand(getTimestamp(), userId, fullName, phone, apartmentId, accountNumber, reservations);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, PlaceOwnerOrderCommand> ValidatePlaceOwnerOrder(
        Func<ResourceId, Option<ResourceType>> getResourceType, Func<Instant> getTimestamp, PlaceOwnerOrderRequest request, UserId userId)
    {
        var either =
            from description in ValidateOwnerOrderDescription(request.Description)
            from reservations in ValidateReservations(getResourceType, request.Reservations.ToSeq())
            select new PlaceOwnerOrderCommand(getTimestamp(), userId, description, reservations, request.IsCleaningRequired);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, UpdateMyOrderCommand> ValidateUpdateMyOrder(
        IDateProvider dateProvider, OrderId orderId, UpdateMyOrderRequest? body, UserId userId)
    {
        var either =
            from request in HasValue(body, "Request body")
            from accountNumber in ValidateOptionalAccountNumber(request.AccountNumber)
            select new UpdateMyOrderCommand(
                dateProvider.Now,
                userId,
                orderId,
                accountNumber,
                toHashSet(request.CancelledReservations ?? noReservations),
                request.WaiveFee);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, UpdateUserOrderCommand> ValidateUpdateUserOrder(
        IDateProvider dateProvider, OrderId orderId, UpdateUserOrderRequest? body, UserId updatedByUserId)
    {
        var either =
            from request in HasValue(body, "Request body")
            from accountNumber in ValidateOptionalAccountNumber(request.AccountNumber)
            select new UpdateUserOrderCommand(
                dateProvider.Now,
                updatedByUserId,
                request.UserId,
                orderId,
                accountNumber,
                toHashSet(request.CancelledReservations ?? noReservations),
                request.WaiveFee,
                request.AllowCancellationWithoutFee);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, UpdateOwnerOrderCommand> ValidateUpdateOwnerOrder(
        IDateProvider dateProvider, OrderId orderId, UpdateOwnerOrderRequest? body, UserId userId)
    {
        var either =
            from request in HasValue(body, "Request body")
            from description in ValidateOptionalOwnerOrderDescription(request.Description)
            select new UpdateOwnerOrderCommand(
                dateProvider.Now,
                userId,
                orderId,
                description,
                toHashSet(request.CancelledReservations ?? noReservations),
                request.IsCleaningRequired.ToOption());
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, PayInCommand> ValidatePayIn(
        IDateProvider dateProvider, string? paymentId, PayInRequest? body, UserId createdByUserId)
    {
        var now = dateProvider.Now;
        var either =
            from request in HasValue(body, "Request body")
            from id in ValidatePaymentId(paymentId)
            from date in IsNotFutureDate(dateProvider.GetDate(now), request.Date, "Date")
            from amount in ValidateAmount(request.Amount, ValidationRules.MinimumAmount, ValidationRules.MaximumAmount, "Amount")
            select new PayInCommand(now, createdByUserId, id, date, amount);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, SettleReservationCommand> ValidateSettleReservation(
        IDateProvider dateProvider, OrderId orderId, SettleReservationRequest? body, UserId userId)
    {
        var either =
            from request in HasValue(body, "Request body")
            from amount in ValidateAmount(request.Damages, Amount.Zero, ValidationRules.MaximumAmount, "Damages")
            from description in ValidateDamagesDescription(request.Damages, request.Description)
            select new SettleReservationCommand(dateProvider.Now, userId, request.UserId, orderId, request.ReservationId, amount, description);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, PayOutCommand> ValidatePayOut(
        IDateProvider dateProvider, UserId userId, PayOutRequest? body, UserId administratorUserId)
    {
        var now = dateProvider.Now;
        var either =
            from request in HasValue(body, "Request body")
            from date in IsNotFutureDate(dateProvider.GetDate(now), request.Date, "Date")
            from amount in ValidateAmount(request.Amount, ValidationRules.MinimumAmount, ValidationRules.MaximumAmount, "Amount")
            select new PayOutCommand(now, administratorUserId, userId, date, amount);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, Option<string>> ValidateOptionalAccountNumber(string? accountNumber) =>
        accountNumber is null ? Option<string>.None : Some(ValidateAccountNumber(accountNumber)).Sequence();

    static Either<string, string> ValidateAccountNumber(string? accountNumber) =>
        from accountNumberNotNull in IsNotNullOrEmpty(accountNumber, "Account number")
        let trimmedAccountNumber = accountNumberNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedAccountNumber, ValidationRules.MaximumAccountNumberLength, "Account number")
        from _2 in IsMatching(trimmedAccountNumber, ValidationRules.AccountNumberRegex, "Account number")
        select trimmedAccountNumber;

    static Either<string, Seq<ReservationModel>> ValidateReservations(
        Func<ResourceId, Option<ResourceType>> getResourceType, Seq<ReservationRequest> reservations) =>
        from seq in IsBoundedCollection(reservations, 1, ValidationRules.MaximumReservationsPerOrder, "Reservations")
        from models in ValidateAll(seq, request => ValidateReservation(getResourceType, request))
        from _ in ValidateNoConflicts(GetAllCombinations(seq))
        select models;

    static Either<string, ReservationModel> ValidateReservation(Func<ResourceId, Option<ResourceType>> getResourceType, ReservationRequest request) =>
        from resourceType in ValidateResourceId(getResourceType, request.ResourceId)
        from _ in IsNotLessThan(request.Extent.Nights, 1, "Nights")
        select new ReservationModel(request.ResourceId, resourceType, request.Extent);

    static Either<string, ResourceType> ValidateResourceId(Func<ResourceId, Option<ResourceType>> getResourceType, ResourceId resourceId) =>
        getResourceType(resourceId).Case switch
        {
            ResourceType resourceType => resourceType,
            _ => "Resource ID is invalid."
        };

    static Either<string, PaymentId> ValidatePaymentId(string? paymentId) =>
        PaymentIdEncoder.IsValid(paymentId) ? PaymentId.FromString(paymentId) : Left("Invalid payment ID.");

    static Seq<Seq<ReservationRequest>> GetAllCombinations(Seq<ReservationRequest> reservations) =>
        reservations.Match(
            () => Empty,
            seq => seq.Cons(GetAllCombinations(seq.Tail)));

    static Either<string, Seq<Unit>> ValidateNoConflicts(Seq<Seq<ReservationRequest>> combinations) =>
        combinations.Map(ValidateNoConflicts).Sequence();

    static Either<string, Unit> ValidateNoConflicts(Seq<ReservationRequest> reservations) =>
        reservations.Match(() => unit, (head, tail) => ValidateNoConflicts(head, tail));

    static Either<string, Unit> ValidateNoConflicts(ReservationRequest reservation, Seq<ReservationRequest> reservations) =>
        reservations.Any(otherReservation => reservation.ResourceId == otherReservation.ResourceId && reservation.Extent.Overlaps(otherReservation.Extent))
            ? "Two or more reservations are in conflict."
            : unit;

    static Either<string, Option<string>> ValidateDamagesDescription(Amount damages, string? description) =>
        damages > Amount.Zero ? IsValidDamagesDescription(description) : Option<string>.None;

    static Either<string, Option<string>> IsValidDamagesDescription(string? description) =>
        from nonEmptyString in IsNotNullOrEmpty(description, "Description")
        from validDescription in IsNotLongerThan(nonEmptyString, ValidationRules.MaximumDamagesDescriptionLength, "Description")
        select Some(validDescription);
}
