using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
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
        private readonly IReservationPolicyProvider reservationPolicyProvider;

        public OrdersController(ReservationsContext db, IDataProvider dataProvider, IReservationPolicyProvider reservationPolicyProvider, IClock clock)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.reservationPolicyProvider = reservationPolicyProvider ?? throw new ArgumentNullException(nameof(reservationPolicyProvider));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get() => await db.Orders.ToListAsync();

        [HttpPost]
        public async Task<PlaceOrderResponse> Post(PlaceOrderRequest request)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = nameIdentifierClaim != null && int.TryParse(nameIdentifierClaim.Value, out var id) ? (int?) id : null;
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
                UpdatedTimestamp = now,
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

                reservation.Days = Enumerable.Range(0, reservation.DurationInDays)
                    .Select(i => new ReservedDay
                    {
                        Date = reservation.Date.PlusDays(i),
                        ResourceId = reservation.ResourceId
                    })
                    .ToList();
                order.Reservations.Add(reservation);

                var policy = reservationPolicyProvider.GetPolicy(resource.Type);
                var price = await policy.GetPrice(reservation);
                order.Price.Rent += price.Rent;
                order.Price.CleaningFee += price.CleaningFee;
                order.Price.Deposit += price.Deposit;
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