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
using static Frederikskaj2.Reservations.Application.SettleReservationHandler;
using static Frederikskaj2.Reservations.Server.Validator;

namespace Frederikskaj2.Reservations.Server;

[Authorize(Roles = nameof(Roles.OrderHandling))]
public class SettleReservationEndpoint : EndpointBaseAsync.WithRequest<SettleReservationServerRequest>.WithActionResult<OrderDetails>
{
    readonly ILogger logger;
    readonly Func<SettleReservationCommand, EitherAsync<Failure, OrderDetails>> settleReservation;
    readonly Func<OrderId, SettleReservationRequest?, UserId, EitherAsync<Failure, SettleReservationCommand>> validateRequest;

    public SettleReservationEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<SettleReservationEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (orderId, request, userId) => ValidateSettleReservation(dateProvider, orderId, request, userId).ToAsync();
        settleReservation = command => Handle(contextFactory, dateProvider, emailService, options.Value, command);
    }

    [HttpPost("orders/user/{orderId:int}/settle-reservation")]
    public override Task<ActionResult<OrderDetails>> HandleAsync([FromRoute] SettleReservationServerRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(OrderId.FromInt32(request.OrderId), request.Body, userId)
            from order in settleReservation(command)
            select order;
        return either.ToResultAsync(logger, HttpContext);
    }
}
