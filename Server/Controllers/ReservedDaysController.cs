using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using ReservedDay = Frederikskaj2.Reservations.Shared.ReservedDay;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("reserved-days")]
    [ApiController]
    public class ReservedDaysController : Controller
    {
        private static readonly LocalDatePattern
            DatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

        private readonly ReservationsContext db;

        public ReservedDaysController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<ReservedDay>> Get(string? fromDate, string? toDate)
        {
            var userId = User.Id();
            var query = db.ReservedDays
                .Include(reservedDay => reservedDay.Reservation!)
                .ThenInclude(reservation => reservation.Order)
                .Select(reservedDay => new ReservedDay
                {
                    Date = reservedDay.Date,
                    ResourceId = reservedDay.Reservation!.ResourceId,
                    IsMyReservation = reservedDay.Reservation.Order!.UserId == userId
                });

            if (!string.IsNullOrEmpty(fromDate))
            {
                var parseResult = DatePattern.Parse(fromDate);
                if (parseResult.Success)
                {
                    var date = parseResult.Value;
                    query = query.Where(rr => date <= rr.Date);
                }
            }

            if (!string.IsNullOrEmpty(toDate))
            {
                var parseResult = DatePattern.Parse(toDate);
                if (parseResult.Success)
                {
                    var date = parseResult.Value;
                    query = query.Where(rr => rr.Date <= date);
                }
            }

            return await query.OrderBy(rr => rr.Date).ToListAsync();
        }
    }
}