using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("owner-orders")]
    [Authorize(Roles = Roles.OrderHandling)]
    [ApiController]
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "The framework ensures that the action arguments are non-null.")]
    public class OwnerOrdersController : Controller
    {
        private readonly IClock clock;
        private readonly OrderService orderService;
        private readonly UserManager<User> userManager;

        public OwnerOrdersController(IClock clock, OrderService orderService, UserManager<User> userManager)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<IEnumerable<OwnerOrder>> Get()
        {
            var now = clock.GetCurrentInstant();
            var orders = await orderService.GetOwnerOrders(now);
            return orders.Select(order => CreateOrder(order, order.User));
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> Get(int orderId)
        {
            var order = await orderService.GetOwnerOrder(orderId);
            if (order == null)
                return NotFound();
            return Ok(CreateOrder(order, order.User!));
        }

        [HttpPost]
        public async Task<IActionResult> Post(PlaceOwnerOrderRequest request)
        {
            if (request.Reservations.Count == 0)
                return BadRequest();

            var userId = User.Id();
            var user = await userManager.FindByIdAsync(userId!.Value.ToString(CultureInfo.InvariantCulture));

            var now = clock.GetCurrentInstant();
            var order = await orderService.PlaceOwnerOrder(now, userId.Value, request.Reservations, request.IsCleaningRequired);

            var ownerOrder = CreateOrder(order, user);
            var url = Url.Action(nameof(Get), new { orderId = order.Id });
            return Created(new Uri(url, UriKind.Relative), ownerOrder);
        }

        [HttpPatch("{orderId:int}")]
        public async Task<OrderResponse<OwnerOrder>> Patch(int orderId, UpdateOwnerOrderRequest request)
        {
            var order = await orderService.UpdateOwnerOrder(orderId, request.CancelledReservations, request.IsCleaningRequired);
            if (order == null)
                return new OrderResponse<OwnerOrder> { IsDeleted = true };
            return new OrderResponse<OwnerOrder> { Order = CreateOrder(order, order.User!) };
        }

        private static OwnerOrder CreateOrder(Data.Order order, User? user)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            return new OwnerOrder
            {
                Id = order.Id,
                CreatedTimestamp = order.CreatedTimestamp,
                Reservations = reservations,
                IsCleaningRequired = order.Flags.HasFlag(OrderFlags.IsCleaningRequired),
                CreatedByEmail = user?.Email,
                CreatedByName = user?.FullName
            };

            static Reservation CreateReservation(Data.Reservation reservation) => new Reservation
            {
                Id = reservation.Id,
                ResourceId = reservation.ResourceId,
                Status = reservation.Status,
                UpdatedTimestamp = reservation.UpdatedTimestamp,
                Date = reservation.Date,
                DurationInDays = reservation.DurationInDays,
                CanBeCancelled = true
            };
        }
    }
}