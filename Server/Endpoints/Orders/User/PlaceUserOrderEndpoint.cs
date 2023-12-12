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
public class PlaceUserOrderEndpoint : EndpointBaseAsync.WithRequest<PlaceUserOrderRequest>.WithActionResult<MyOrder>
{
    readonly ILogger logger;
    readonly Func<PlaceUserOrderCommand, EitherAsync<Failure, MyOrder>> placeOrder;
    readonly Func<PlaceUserOrderRequest, UserId, EitherAsync<Failure, PlaceUserOrderCommand>> validateRequest;

    public PlaceUserOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<PlaceUserOrderEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        placeOrder = command => PlaceUserOrderHandler.Handle(contextFactory, dateProvider, emailService, options.Value, command);
        validateRequest = (request, administratorUserId) =>
            Validator.ValidatePlaceUserOrder(Apartments.IsValid, Resources.GetResourceType, () => dateProvider.Now, request, administratorUserId).ToAsync();
    }

    [HttpPost("orders/user")]
    public override Task<ActionResult<MyOrder>> HandleAsync(PlaceUserOrderRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(request, userId)
            from order in placeOrder(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
