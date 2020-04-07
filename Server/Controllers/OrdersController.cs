using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly OrderService orderService;
        private readonly ReservationsOptions reservationsOptions;

        public OrdersController(
            IClock clock, DateTimeZone dateTimeZone, OrderService orderService, ReservationsOptions reservationsOptions)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            var orders = await orderService.GetOrders();
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return orders.Select(order => CreateOrder(order, today));
        }

        [HttpGet("{orderId:int}")]
        public async Task<OrderResponse<Order>> Get(int orderId)
        {
            var order = await orderService.GetOrder(orderId);
            if (order == null)
                return new OrderResponse<Order>();
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return new OrderResponse<Order> { Order = CreateOrder(order, today) };
        }

        [Route("{orderId:int}")]
        public async Task<OrderResponse<Order>> Patch(int orderId, UpdateOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<Order>();

            var now = clock.GetCurrentInstant();
            var accountNumber = request.AccountNumber.Trim().ToUpperInvariant();
            var order = await orderService.UpdateOrder(
                now, orderId, accountNumber, request.CancelledReservations, userId.Value);
            if (order == null)
                return new OrderResponse<Order>();

            var today = now.InZone(dateTimeZone).Date;
            return new OrderResponse<Order> { Order = CreateOrder(order, today) };
        }

        [HttpPost("{orderId:int}/pay-in")]
        public async Task<OrderResponse<Order>> PayIn(int orderId, PaymentRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<Order>();

            var now = clock.GetCurrentInstant();
            var order = await orderService.PayIn(now, orderId, userId.Value, request.Amount);
            if (order == null)
                return new OrderResponse<Order>();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return new OrderResponse<Order> { Order = CreateOrder(order, today) };
        }

        [HttpPost("{orderId:int}/settle")]
        public async Task<OrderResponse<Order>> Settle(int orderId, SettleReservationRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<Order>();

            var description = request.Description?.Trim();
            if (description?.Length > 100)
                return new OrderResponse<Order>();

            var now = clock.GetCurrentInstant();
            var order = await orderService.Settle(now, orderId, userId.Value, request.ReservationId, request.Damages, description);
            if (order == null)
                return new OrderResponse<Order>();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return new OrderResponse<Order> { Order = CreateOrder(order, today) };
        }

        [HttpPost("{orderId:int}/pay-out")]
        public async Task<OrderResponse<Order>> PayOut(int orderId, PaymentRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<Order>();

            var now = clock.GetCurrentInstant();
            var order = await orderService.PayOut(now, orderId, userId.Value, request.Amount);
            if (order == null)
                return new OrderResponse<Order>();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return new OrderResponse<Order> { Order = CreateOrder(order, today) };
        }

        private Order CreateOrder(Data.Order order, LocalDate today)
        {
            return new Order
            {
                Id = order.Id,
                CreatedTimestamp = order.CreatedTimestamp,
                Email = order.User!.Email,
                FullName = order.User.FullName,
                Phone = order.User.PhoneNumber,
                AccountNumber = order.AccountNumber!,
                Reservations = order.Reservations.Select(CreateReservation),
                Totals = orderService.GetTotals(order)
            };

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