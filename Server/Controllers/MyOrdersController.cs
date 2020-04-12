using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Price = Frederikskaj2.Reservations.Shared.Price;
using Reservation = Frederikskaj2.Reservations.Shared.Reservation;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("my-orders")]
    [Authorize]
    [ApiController]
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "The framework ensures that the action arguments are non-null.")]
    public class MyOrdersController : Controller
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly OrderService orderService;
        private readonly ReservationsOptions reservationsOptions;

        public MyOrdersController(
            IClock clock, DateTimeZone dateTimeZone, ReservationsContext db, OrderService orderService,
            ReservationsOptions reservationsOptions)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
        }

        [HttpGet]
        public async Task<IEnumerable<MyOrder>> Get()
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return Enumerable.Empty<MyOrder>();

            var orders = await orderService.GetOrders(userId.Value);
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return orders.Select(order => CreateOrder(order, today)).ToList();
        }

        [HttpGet("{orderId:int}")]
        public async Task<OrderResponse<MyOrder>> Get(int orderId)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<MyOrder>();

            var order = await orderService.GetOrder(orderId);
            if (order == null || order.UserId != userId.Value)
                return new OrderResponse<MyOrder>();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            var myOrder = CreateOrder(order, today);
            myOrder.Totals = orderService.GetTotals(order);
            return new OrderResponse<MyOrder> { Order = myOrder };
        }

        [HttpPost]
        public async Task<PlaceOrderResponse> Post(PlaceOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue || request.Reservations.Count == 0)
                return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };

            var user = await db.Users.FindAsync(userId.Value);
            user.FullName = request.FullName.Trim();
            user.PhoneNumber = request.Phone.Trim();
            user.ApartmentId = request.ApartmentId;

            var now = clock.GetCurrentInstant();
            var tuple = await orderService.PlaceOrder(
                now, user.Id, request.ApartmentId, request.AccountNumber.Trim().ToUpperInvariant(),
                request.Reservations);
            var today = now.InZone(dateTimeZone).Date;
            var myOrder = CreateMyOrder(tuple.Order);
            return new PlaceOrderResponse { Result = tuple.Result, Order = myOrder };

            MyOrder? CreateMyOrder(Data.Order? order)
            {
                if (order == null)
                    return null;
                var myOrder = CreateOrder(order, today);
                myOrder.Totals = orderService.GetTotals(order);
                return myOrder;
            }
        }

        [Route("{orderId:int}")]
        public async Task<OrderResponse<MyOrder>> Patch(int orderId, UpdateOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<MyOrder>();

            var now = clock.GetCurrentInstant();
            var accountNumber = request.AccountNumber.Trim().ToUpperInvariant();
            var (isUserDeleted, order) = await orderService.UpdateOrder(
                now, orderId, accountNumber, request.CancelledReservations, userId.Value, false, userId.Value);
            if (order == null)
                return new OrderResponse<MyOrder>();

            var today = now.InZone(dateTimeZone).Date;
            var myOrder = CreateOrder(order, today);
            myOrder.Totals = orderService.GetTotals(order);
            return new OrderResponse<MyOrder> { Order = myOrder, IsUserDeleted = isUserDeleted };
        }

        private MyOrder CreateOrder(Data.Order order, LocalDate today)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            var canBeEdited = order.AccountNumber != null && reservations.Any(r => r.CanBeCancelled);
            var myOrder = new MyOrder
            {
                Id = order.Id,
                AccountNumber = order.AccountNumber!,
                CreatedTimestamp = order.CreatedTimestamp,
                Reservations = reservations,
                CanBeEdited = canBeEdited
            };
            myOrder.Totals = orderService.GetTotals(order);
            return myOrder;

            Reservation CreateReservation(Data.Reservation reservation) => new Reservation
            {
                Id = reservation.Id,
                ResourceId = reservation.ResourceId,
                Status = reservation.Status,
                UpdatedTimestamp = reservation.UpdatedTimestamp,
                Price = reservation.Price!.Adapt<Price>(),
                Date = reservation.Date,
                DurationInDays = reservation.DurationInDays,
                CanBeCancelled = reservation.CanBeCancelled(today, reservationsOptions)
            };
        }
    }
}