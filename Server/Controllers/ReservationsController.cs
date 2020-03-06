using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("reservations")]
    [ApiController]
    public class ReservationsController : Controller
    {
        private static readonly LocalDatePattern
            DatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

        private readonly ReservationsContext db;

        public ReservationsController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<Reservation>> Get(int? resourceId, string? fromDate, string? toDate)
        {
            IQueryable<Reservation> query = db.Reservations;

            if (resourceId != null)
                query = query.Where(rr => rr.ResourceId == resourceId.Value);

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