using Ardalis.ApiEndpoints;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.OrderHandling))]
public class UpdateUserReservationsEndpoint : EndpointBaseAsync.WithRequest<UpdateUserReservationsServerRequest>.WithoutResult
{
    readonly ILogger logger;
    readonly Func<UpdateUserReservationsCommand, EitherAsync<Failure, Unit>> updateOrder;
    readonly Func<OrderId, UpdateUserReservationsRequest?, UserId, EitherAsync<Failure, UpdateUserReservationsCommand>> validateRequest;

    public UpdateUserReservationsEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<UpdateUserReservationsEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (orderId, request, updatedByUserid) =>
            Validator.ValidateUpdateUserReservations(dateProvider, orderId, request, updatedByUserid).ToAsync();
        updateOrder = command => UpdateUserReservationsHandler.Handle(contextFactory, dateProvider, emailService, options.Value, command);
    }

    [HttpPatch("orders/user/{orderId:int}/reservations")]
    public override Task<ActionResult> HandleAsync([FromRoute] UpdateUserReservationsServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(OrderId.FromInt32(request.OrderId), request.Body, userId)
            from order in updateOrder(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
