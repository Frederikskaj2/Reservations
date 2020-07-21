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
using LockBoxCode = Frederikskaj2.Reservations.Server.Data.LockBoxCode;
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
        private readonly LockBoxCodeService lockBoxCodeService;
        private readonly OrderService orderService;
        private readonly ReservationsOptions reservationsOptions;

        public MyOrdersController(
            IClock clock, DateTimeZone dateTimeZone, ReservationsContext db, LockBoxCodeService lockBoxCodeService,
            OrderService orderService, ReservationsOptions reservationsOptions)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.lockBoxCodeService = lockBoxCodeService ?? throw new ArgumentNullException(nameof(lockBoxCodeService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
        }

        [HttpGet]
        public async Task<IEnumerable<MyOrder>> Get()
        {
            var userId = User.Id();
            var orders = await orderService.GetAllOrders(userId!.Value);
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return orders.Select(order => CreateOrder(order, today)).ToList();
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> Get(int orderId)
        {
            var userId = User.Id();
            var order = await orderService.GetOrder(orderId);
            if (order == null || order.UserId != userId!.Value)
                return NotFound();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            var lockBoxCodes = await GetLockBoxCodes(order, today);
            var myOrder = CreateOrder(order, today, lockBoxCodes);
            myOrder.Totals = orderService.GetTotals(order);
            return Ok(myOrder);
        }

        [HttpPost]
        public async Task<IActionResult> Post(PlaceOrderRequest request)
        {
            if (request.Reservations.Count == 0)
                return BadRequest();
            var userId = User.Id();

            var user = await db.Users.FindAsync(userId!.Value);
            user.FullName = request.FullName.Trim();
            user.PhoneNumber = request.Phone.Trim();
            user.ApartmentId = request.ApartmentId;

            var now = clock.GetCurrentInstant();
            var order = await orderService.PlaceOrder(
                now, user.Id, request.ApartmentId, request.AccountNumber.Trim().ToUpperInvariant(),
                request.Reservations);

            var today = now.InZone(dateTimeZone).Date;
            var myOrder = CreateOrder(order, today);
            myOrder.Totals = orderService.GetTotals(order);
            var url = Url.Action(nameof(Get), new { orderId = order.Id });
            return Created(new Uri(url, UriKind.Relative), myOrder);
        }

        [Route("{orderId:int}")]
        public async Task<OrderResponse<MyOrder>> Patch(int orderId, UpdateOrderRequest request)
        {
            var userId = User.Id();

            var now = clock.GetCurrentInstant();
            var accountNumber = request.AccountNumber.Trim().ToUpperInvariant();
            var (isUserDeleted, order) = await orderService.UpdateOrder(
                now, orderId, accountNumber, request.CancelledReservations, userId!.Value, false, userId.Value);

            var today = now.InZone(dateTimeZone).Date;
            var lockBoxCodes = await GetLockBoxCodes(order, today);
            var myOrder = CreateOrder(order, today, lockBoxCodes);
            myOrder.Totals = orderService.GetTotals(order);
            return new OrderResponse<MyOrder> { Order = myOrder, IsDeleted = isUserDeleted };
        }

        private async Task<Dictionary<int, List<DatedLockBoxCode>>> GetLockBoxCodes(Order order, LocalDate today)
        {
            var reservationsNeedingLockBoxCodes = order.Reservations
                .Where(
                    reservation =>
                        reservation.Status == ReservationStatus.Confirmed
                        && reservation.Date.PlusDays(-reservationsOptions.RevealLockBoxCodeDaysBeforeReservationStart)
                        <= today
                        && today <= reservation.Date.PlusDays(reservation.DurationInDays))
                .ToList();
            var lockBoxCodes = reservationsNeedingLockBoxCodes.Count > 0
                ? await lockBoxCodeService.GetLockBoxCodes(today)
                : Enumerable.Empty<LockBoxCode>();
            return reservationsNeedingLockBoxCodes.ToDictionary(
                reservation => reservation.Id,
                reservation => GetDatedLockBoxCodes(reservation).ToList());

            IEnumerable<DatedLockBoxCode> GetDatedLockBoxCodes(Data.Reservation reservation)
            {
                var previousMonday = GetPreviousMonday(reservation.Date);
                var firstLockBoxCode = lockBoxCodes.FirstOrDefault(lockBoxCode => lockBoxCode.ResourceId == reservation.ResourceId && lockBoxCode.Date == previousMonday);
                if (firstLockBoxCode == null)
                    yield break;
                yield return new DatedLockBoxCode { Date = reservation.Date, Code = firstLockBoxCode.Code };
                var nextMonday = previousMonday.PlusWeeks(1);
                if (reservation.Date.PlusDays(reservation.DurationInDays) < nextMonday)
                    yield break;
                var nextLockBoxCode = lockBoxCodes.FirstOrDefault(lockBoxCode => lockBoxCode.ResourceId == reservation.ResourceId && lockBoxCode.Date == nextMonday);
                if (nextLockBoxCode == null)
                    yield break;
                yield return new DatedLockBoxCode { Date = nextMonday, Code = nextLockBoxCode.Code };
            }

            static LocalDate GetPreviousMonday(LocalDate d)
            {
                var daysAfterMonday = ((int) d.DayOfWeek - 1)%7;
                return d.PlusDays(-daysAfterMonday);
            }
        }

        private MyOrder CreateOrder(Order order, LocalDate today, Dictionary<int, List<DatedLockBoxCode>>? lockBoxCodes = null)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            var isHistoryOrder = order.Flags.HasFlag(OrderFlags.IsHistoryOrder);
            var canBeEdited = !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && reservations.Any(r => r.CanBeCancelled);
            var myOrder = new MyOrder
            {
                Id = order.Id,
                IsHistoryOrder = isHistoryOrder,
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
                CanBeCancelled = reservation.CanBeCancelledUser(today, reservationsOptions),
                LockBoxCodes = lockBoxCodes != null && lockBoxCodes.TryGetValue(reservation.Id, out var datedLockBoxCodes) ? datedLockBoxCodes : null
            };
        }
    }
}