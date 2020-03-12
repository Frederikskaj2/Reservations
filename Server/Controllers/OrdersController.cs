using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [Authorize]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IClock clock;
        private readonly ReservationsContext db;
        private readonly IDataProvider dataProvider;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsOptions reservationsOptions;
        private readonly IReservationPolicyProvider reservationPolicyProvider;

        public OrdersController(
            ReservationsContext db, IDataProvider dataProvider, ReservationsOptions reservationsOptions,
            IReservationPolicyProvider reservationPolicyProvider, IClock clock, DateTimeZone dateTimeZone)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.reservationsOptions = reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.reservationPolicyProvider = reservationPolicyProvider ?? throw new ArgumentNullException(nameof(reservationPolicyProvider));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
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

            // Fix object graph.
            foreach (var order in orders)
            {
                order.Price = new Price();
                foreach (var reservation in order.Reservations!)
                {
                    // Remove cycles in object graph.
                    reservation.Order = null;

                    // Simplify date and duration.
                    reservation.Date = reservation.Days.Min(day => day.Date);
                    reservation.DurationInDays = reservation.Days.Count();
                    reservation.Days = null;
                    reservation.CanBeCancelled =
                        (reservation.Status == ReservationStatus.Reserved
                         || reservation.Status == ReservationStatus.Confirmed)
                        && today.PlusDays(reservationsOptions.MinimumCancellationNoticeInDays) <= reservation.Date;

                    // Update total price.
                    if (reservation.Status == ReservationStatus.Cancelled)
                        continue;
                    order.Price.Rent += reservation.Price!.Rent;
                    order.Price.CleaningFee += reservation.Price!.CleaningFee;
                    order.Price.Deposit += reservation.Price!.Deposit;
                }
            }

            return orders;
        }

        [HttpPost]
        public async Task<PlaceOrderResponse> Post(PlaceOrderRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };

            var user = await db.Users.FindAsync(userId.Value);
            user.ApartmentId = request.ApartmentId;

            var now = clock.GetCurrentInstant();
            var order = new Order
            {
                UserId = userId.Value,
                ApartmentId = request.ApartmentId,
                AccountNumber = request.AccountNumber.Trim().ToUpperInvariant(),
                CreatedTimestamp = now,
                Reservations = new List<Reservation>(),
                Price = new Price()
            };

            var resources = await dataProvider.GetResources();
            foreach (var reservation in request.Reservations!)
            {
                if (!resources.TryGetValue(reservation.ResourceId, out var resource))
                    return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };

                reservation.Resource = resource;
                if (!await IsReservationDurationValid(reservation))
                    return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };

                reservation.Status = ReservationStatus.Reserved;
                reservation.UpdatedTimestamp = now;

                reservation.Days = Enumerable.Range(0, reservation.DurationInDays)
                    .Select(i => new ReservedDay
                    {
                        Date = reservation.Date.PlusDays(i),
                        ResourceId = reservation.ResourceId
                    })
                    .ToList();

                var policy = reservationPolicyProvider.GetPolicy(resource.Type);
                reservation.Price = await policy.GetPrice(reservation);

                order.Reservations.Add(reservation);
            }

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is SqliteException sqliteException && sqliteException.SqliteErrorCode == 19)
                    return new PlaceOrderResponse { Result = PlaceOrderResult.ReservationConflict };
                return new PlaceOrderResponse { Result = PlaceOrderResult.GeneralError };
            }

            return new PlaceOrderResponse();

            async Task<bool> IsReservationDurationValid(Reservation reservation)
            {
                var policy = reservationPolicyProvider.GetPolicy(reservation.Resource!.Type);
                var (minimumDays, maximumDays) = await policy.GetReservationAllowedNumberOfDays(reservation.ResourceId, reservation.Date);
                return minimumDays <= reservation.DurationInDays && reservation.DurationInDays <= maximumDays;
            }
        }
    }
}