using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Order = Frederikskaj2.Reservations.Shared.Order;
using Reservation = Frederikskaj2.Reservations.Shared.Reservation;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [Authorize]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly ReservationsOptions reservationsOptions;

        public OrdersController(ReservationsContext db, ReservationsOptions reservationsOptions, IClock clock, DateTimeZone dateTimeZone)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.reservationsOptions = reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            var orders = await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Resource)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return orders.Select(order => CreateOrder(order, today)).ToList();
        }

        private Order CreateOrder(Data.Order order, LocalDate today)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            return new Order
            {
                Id = order.Id,
                CreatedTimestamp = order.CreatedTimestamp,
                Mail = order.User!.Email,
                FullName = order.User.FullName,
                Phone = order.User.PhoneNumber,
                AccountNumber = order.AccountNumber!,
                Reservations = reservations,
            };

            Reservation CreateReservation(Data.Reservation reservation) => new Reservation
            {
                Id = reservation.Id,
                ResourceId = reservation.ResourceId,
                Status = reservation.Status,
                UpdatedTimestamp = reservation.UpdatedTimestamp,
                Price = reservation.Price!.Adapt<Shared.Price>(),
                Date = reservation.Days!.First().Date,
                DurationInDays = reservation.Days!.Count,
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