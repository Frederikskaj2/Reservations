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
public class PlaceOwnerOrderEndpoint : EndpointBaseAsync.WithRequest<PlaceOwnerOrderRequest>.WithActionResult<OrderDetails>
{
    readonly ILogger logger;
    readonly Func<PlaceOwnerOrderCommand, EitherAsync<Failure, OrderDetails>> placeOwnerOrder;
    readonly Func<PlaceOwnerOrderRequest, UserId, EitherAsync<Failure, PlaceOwnerOrderCommand>> validateRequest;

    public PlaceOwnerOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, ILogger<PlaceOwnerOrderEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        validateRequest = (request, userId) =>
            Validator.ValidatePlaceOwnerOrder(Resources.GetResourceType, () => dateProvider.Now, request, userId).ToAsync();
        placeOwnerOrder = command => PlaceOwnerOrderHandler.Handle(contextFactory, dateProvider, options.Value, command);
    }

    [HttpPost("orders/owner")]
    public override Task<ActionResult<OrderDetails>> HandleAsync(PlaceOwnerOrderRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from model in validateRequest(request, userId)
            from order in placeOwnerOrder(model)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
