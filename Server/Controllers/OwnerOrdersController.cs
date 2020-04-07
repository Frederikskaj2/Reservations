using System;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("owner-orders")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
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

        [HttpPost]
        public async Task<PlaceOwnerOrderResponse> Post(PlaceOwnerOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue || request.Reservations.Count == 0)
                return new PlaceOwnerOrderResponse { Result = PlaceOrderResult.GeneralError };
            var user = await userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return new PlaceOwnerOrderResponse { Result = PlaceOrderResult.GeneralError };

            var now = clock.GetCurrentInstant();
            var tuple = await orderService.PlaceOwnerOrder(now, userId.Value, request.Reservations);

            var ownerOrder = tuple.Order != null ? CreateOrder(tuple.Order, user) : null;
            return new PlaceOwnerOrderResponse { Result = tuple.Result, Order = ownerOrder };
        }

        private static OwnerOrder CreateOrder(Data.Order order, User user)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            return new OwnerOrder
            {
                Id = order.Id,
                CreatedTimestamp = order.CreatedTimestamp,
                Reservations = reservations,
                CreatedByEmail = user.Email,
                CreatedByName = user.FullName
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