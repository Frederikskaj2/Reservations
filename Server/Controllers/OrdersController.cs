using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [Authorize(Roles = Roles.OrderHandling + "," + Roles.Payment + "," + Roles.Settlement)]
    [ApiController]
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "The framework ensures that the action arguments are non-null.")]
    public class OrdersController : Controller
    {
        private readonly IClock clock;
        private readonly OrderService orderService;

        public OrdersController(IClock clock, OrderService orderService)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            var orders = await orderService.GetOrders();
            return orders.Select(CreateOrder);
        }

        [HttpGet("{orderId:int}")]
        public async Task<OrderResponse<Order>> Get(int orderId)
        {
            var order = await orderService.GetOrder(orderId);
            if (order == null)
                return new OrderResponse<Order>();
            return new OrderResponse<Order> { Order = CreateOrder(order) };
        }

        [HttpPatch("{orderId:int}")]
        public async Task<OrderResponse<Order>> Patch(int orderId, UpdateOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse<Order>();

            var now = clock.GetCurrentInstant();
            var accountNumber = request.AccountNumber.Trim().ToUpperInvariant();
            var (_, order) = await orderService.UpdateOrder(
                now, orderId, accountNumber, request.CancelledReservations, userId.Value, request.WaiveFee);
            if (order == null)
                return new OrderResponse<Order>();

            return new OrderResponse<Order> { Order = CreateOrder(order) };
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

            return new OrderResponse<Order> { Order = CreateOrder(order) };
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

            return new OrderResponse<Order> { Order = CreateOrder(order) };
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

            return new OrderResponse<Order> { Order = CreateOrder(order) };
        }

        private Order CreateOrder(Data.Order order)
        {
            return new Order
            {
                Id = order.Id,
                CreatedTimestamp = order.CreatedTimestamp,
                Email = order.User?.Email,
                FullName = order.User?.FullName,
                Phone = order.User?.PhoneNumber,
                AccountNumber = order.AccountNumber,
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
                DurationInDays = reservation.DurationInDays
            };
        }
    }
}