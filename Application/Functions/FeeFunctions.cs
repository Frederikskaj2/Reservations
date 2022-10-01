using System.Net;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

static class FeeFunctions
{
    public static EitherAsync<Failure, bool> ValidateUserWaivingFee(Order order, Instant timestamp, bool waiveFee) =>
        waiveFee ? ValidateUserWaivingFee(order, timestamp) : false;

    static EitherAsync<Failure, bool> ValidateUserWaivingFee(Order order, Instant timestamp) =>
        timestamp <= order.User!.NoFeeCancellationIsAllowedBefore
            ? true
            : Failure.New(HttpStatusCode.Forbidden, "User is not allowed to cancel reservation without paying fee.");
}