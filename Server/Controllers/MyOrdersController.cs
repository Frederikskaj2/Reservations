using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using KeyCode = Frederikskaj2.Reservations.Server.Data.KeyCode;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
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
        private readonly KeyCodeService keyCodeService;
        private readonly OrderService orderService;
        private readonly ReservationsOptions reservationsOptions;

        public MyOrdersController(
            IClock clock, DateTimeZone dateTimeZone, ReservationsContext db, KeyCodeService keyCodeService,
            OrderService orderService, ReservationsOptions reservationsOptions)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.keyCodeService = keyCodeService ?? throw new ArgumentNullException(nameof(keyCodeService));
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

            var orders = await orderService.GetAllOrders(userId.Value);
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
            var keyCodes = await GetKeyCodes(order, today);
            var myOrder = CreateOrder(order, today, keyCodes);
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
            if (tuple.Order == null)
                return new PlaceOrderResponse { Result = tuple.Result };

            var today = now.InZone(dateTimeZone).Date;
            var myOrder = CreateOrder(tuple.Order, today);
            myOrder.Totals = orderService.GetTotals(tuple.Order);
            return new PlaceOrderResponse { Result = PlaceOrderResult.Success, Order = myOrder };
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
            var keyCodes = await GetKeyCodes(order, today);
            var myOrder = CreateOrder(order, today, keyCodes);
            myOrder.Totals = orderService.GetTotals(order);
            return new OrderResponse<MyOrder> { Order = myOrder, IsDeleted = isUserDeleted };
        }

        private async Task<Dictionary<int, List<DatedKeyCode>>> GetKeyCodes(Order order, LocalDate today)
        {
            var reservationsNeedingKeyCodes = order.Reservations
                .Where(
                    reservation =>
                        reservation.Status == ReservationStatus.Confirmed
                        && reservation.Date.PlusDays(-reservationsOptions.RevealKeyCodeDaysBeforeReservationStart)
                        <= today
                        && today <= reservation.Date.PlusDays(reservation.DurationInDays))
                .ToList();
            var keyCodes = reservationsNeedingKeyCodes.Count > 0
                ? await keyCodeService.GetKeyCodes(today)
                : Enumerable.Empty<KeyCode>();
            return reservationsNeedingKeyCodes.ToDictionary(
                reservation => reservation.Id,
                reservation => GetDatedKeyCodes(reservation).ToList());

            IEnumerable<DatedKeyCode> GetDatedKeyCodes(Data.Reservation reservation)
            {
                var previousMonday = GetPreviousMonday(reservation.Date);
                var firstKeyCode = keyCodes.FirstOrDefault(keyCode => keyCode.ResourceId == reservation.ResourceId && keyCode.Date == previousMonday);
                if (firstKeyCode == null)
                    yield break;
                yield return new DatedKeyCode { Date = reservation.Date, Code = firstKeyCode.Code };
                var nextMonday = previousMonday.PlusWeeks(1);
                if (reservation.Date.PlusDays(reservation.DurationInDays) < nextMonday)
                    yield break;
                var nextKeyCode = keyCodes.FirstOrDefault(keyCode => keyCode.ResourceId == reservation.ResourceId && keyCode.Date == nextMonday);
                if (nextKeyCode == null)
                    yield break;
                yield return new DatedKeyCode { Date = nextMonday, Code = nextKeyCode.Code };
            }

            static LocalDate GetPreviousMonday(LocalDate d)
            {
                var daysAfterMonday = ((int) d.DayOfWeek - 1)%7;
                return d.PlusDays(-daysAfterMonday);
            }
        }

        private MyOrder CreateOrder(Order order, LocalDate today, Dictionary<int, List<DatedKeyCode>>? keyCodes = null)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            var canBeEdited = order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && reservations.Any(r => r.CanBeCancelled);
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
                CanBeCancelled = reservation.CanBeCancelled(today, reservationsOptions),
                KeyCodes = keyCodes != null && keyCodes.TryGetValue(reservation.Id, out var datedKeyCodes) ? datedKeyCodes : null
            };
        }
    }
}