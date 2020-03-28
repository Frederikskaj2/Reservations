using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Order = Frederikskaj2.Reservations.Shared.Order;
using Price = Frederikskaj2.Reservations.Server.Data.Price;
using Reservation = Frederikskaj2.Reservations.Shared.Reservation;
using ReservedDay = Frederikskaj2.Reservations.Server.Data.ReservedDay;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [Authorize]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IClock clock;
        private readonly IDataProvider dataProvider;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly IReservationPolicyProvider reservationPolicyProvider;
        private readonly ReservationsOptions reservationsOptions;

        public OrdersController(
            ReservationsContext db, IDataProvider dataProvider, ReservationsOptions reservationsOptions,
            IReservationPolicyProvider reservationPolicyProvider, IClock clock, DateTimeZone dateTimeZone)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.reservationPolicyProvider = reservationPolicyProvider
                                             ?? throw new ArgumentNullException(nameof(reservationPolicyProvider));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return Enumerable.Empty<Order>();

            var orders = await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Resource)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Where(order => order.UserId == userId.Value)
                .OrderByDescending(order => order.CreatedTimestamp)
                .ToListAsync();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return orders.Select(order => CreateOrder(order, today)).ToList();
        }

        [HttpGet("{orderId:int}")]
        public async Task<Order> Get(int orderId)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return Order.EmptyOrder;

            var order = await GetOrder(orderId, userId.Value);
            if (order == null)
                return Order.EmptyOrder;

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return CreateOrder(order, today);
        }

        [HttpPost]
        public async Task<PlaceOrderResponse> Post(PlaceOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };

            var user = await db.Users.FindAsync(userId.Value);
            user.FullName = request.FullName;
            user.PhoneNumber = request.Phone;
            user.ApartmentId = request.ApartmentId;

            var resources = await dataProvider.GetResources();
            var now = clock.GetCurrentInstant();
            var order = new Data.Order
            {
                UserId = userId.Value,
                ApartmentId = request.ApartmentId,
                AccountNumber = request.AccountNumber.Trim().ToUpperInvariant(),
                CreatedTimestamp = now,
                Reservations = new List<Data.Reservation>()
            };
            foreach (var reservation in request.Reservations)
            {
                var r = await CreateReservation(reservation);
                if (r == null)
                    return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };
                order.Reservations.Add(r);
            }

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is SqliteException sqliteException
                    && sqliteException.SqliteErrorCode == 19)
                    return new PlaceOrderResponse { Result = PlaceOrderResult.ReservationConflict };
                return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };
            }

            return new PlaceOrderResponse();

            async Task<Data.Reservation?> CreateReservation(ReservationRequest reservation)
            {
                if (!resources.TryGetValue(reservation.ResourceId, out var resource))
                    return null;
                var policy = reservationPolicyProvider.GetPolicy(resource.Type);
                if (!await IsReservationDurationValid(reservation, policy))
                    return null;

                var days = Enumerable
                    .Range(0, reservation.DurationInDays)
                    .Select(
                        i => new ReservedDay
                        {
                            Date = reservation.Date.PlusDays(i),
                            ResourceId = reservation.ResourceId
                        })
                    .ToList();
                var price = (await policy.GetPrice(reservation.Date, reservation.DurationInDays)).Adapt<Price>();
                return new Data.Reservation
                {
                    Order = order,
                    ResourceId = resource.Id,
                    Status = ReservationStatus.Reserved,
                    UpdatedTimestamp = now,
                    Days = days,
                    Price = price
                };
            }

            async Task<bool> IsReservationDurationValid(ReservationRequest reservation, IReservationPolicy policy)
            {
                var (minimumDays, maximumDays) =
                    await policy.GetReservationAllowedNumberOfDays(reservation.ResourceId, reservation.Date);
                return minimumDays <= reservation.DurationInDays && reservation.DurationInDays <= maximumDays;
            }
        }

        [Route("{orderId:int}")]
        public async Task<OperationResponse> Patch(int orderId, UpdateOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OperationResponse { Result = OperationResult.GeneralError };

            var order = await GetOrder(orderId, userId.Value);
            if (order == null)
                return new OperationResponse { Result = OperationResult.GeneralError };
            if (order.UserId != userId.Value)
                return new OperationResponse { Result = OperationResult.GeneralError };

            var now = clock.GetCurrentInstant();
            var today = now.InZone(dateTimeZone).Date;
            order.AccountNumber = request.AccountNumber;
            foreach (var reservationId in request.CancelledReservations)
            {
                var reservation = order.Reservations!.Find(r => r.Id == reservationId);
                if (reservation == null || !CanReservationCanBeCancelled(reservation, today))
                    continue;
                var previousStatus = reservation.Status;
                reservation.Status = ReservationStatus.Cancelled;
                reservation.UpdatedTimestamp = now;
                reservation.Price = previousStatus == ReservationStatus.Confirmed
                    ? new Price
                    {
                        Deposit = reservation.Price!.Deposit,
                        CancellationFee = reservationsOptions.CancellationFee
                    }
                    : new Price();
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new OperationResponse { Result = OperationResult.GeneralError };
            }

            return new OperationResponse { Result = OperationResult.Success };
        }

        private async Task<Data.Order> GetOrder(int orderId, int userId)
            => await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Resource)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .FirstOrDefaultAsync(order => order.UserId == userId && order.Id == orderId);

        private Order CreateOrder(Data.Order order, LocalDate today)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            var canBeEdited = reservations.Any(r => r.CanBeCancelled);
            return new Order
            {
                Id = order.Id,
                AccountNumber = order.AccountNumber!,
                CreatedTimestamp = order.CreatedTimestamp,
                Reservations = reservations,
                CanBeEdited = canBeEdited
            };

            Reservation CreateReservation(Data.Reservation reservation) => new Reservation
            {
                Id = reservation.Id,
                ResourceId = reservation.ResourceId,
                Status = reservation.Status,
                UpdatedTimestamp = reservation.UpdatedTimestamp,
                Price = reservation.Price!.Adapt<Shared.Price>(),
                Date = reservation.Days![0].Date,
                DurationInDays = reservation.Days.Count,
                CanBeCancelled = CanReservationCanBeCancelled(reservation, today)
            };
        }

        private bool CanReservationCanBeCancelled(Data.Reservation reservation, LocalDate today)
            => reservation.Status == ReservationStatus.Reserved
               || reservation.Status == ReservationStatus.Confirmed
               && today.PlusDays(reservationsOptions.MinimumCancellationNoticeInDays)
               <= reservation.Days.Min(day => day.Date);
    }
}