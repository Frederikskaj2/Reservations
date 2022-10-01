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

[Authorize(Roles = nameof(Roles.Resident))]
public class PlaceMyOrderEndpoint : EndpointBaseAsync.WithRequest<PlaceMyOrderRequest>.WithActionResult<MyOrder>
{
    readonly ILogger logger;
    readonly Func<PlaceMyOrderCommand, EitherAsync<Failure, MyOrder>> placeOrder;
    readonly Func<PlaceMyOrderRequest, UserId, EitherAsync<Failure, PlaceMyOrderCommand>> validateRequest;

    public PlaceMyOrderEndpoint(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, ILogger<PlaceMyOrderEndpoint> logger,
        IOptionsSnapshot<OrderingOptions> options)
    {
        this.logger = logger;
        placeOrder = command => PlaceMyOrderHandler.Handle(contextFactory, dateProvider, emailService, options.Value, command);
        validateRequest = (request, userId) =>
            Validator.ValidatePlaceUserOrder(Apartments.IsValid, Resources.GetResourceType, () => dateProvider.Now, request, userId).ToAsync();
    }

    [HttpPost("orders/my")]
    public override Task<ActionResult<MyOrder>> HandleAsync(PlaceMyOrderRequest request, CancellationToken cancellationToken = default)
    {
        var either =
            from userId in User.Id().ToEitherAsync(Failure.New(HttpStatusCode.Unauthorized))
            from command in validateRequest(request, userId)
            from order in placeOrder(command)
            select order;
        return either.ToResultAsync(logger, HttpContext, true);
    }
}
