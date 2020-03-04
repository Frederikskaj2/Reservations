using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.State;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("resource-reservations")]
    [ApiController]
    public class ResourceReservationsController : Controller
    {
        private static readonly LocalDatePattern DatePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

        private readonly ReservationsContext db;

        public ResourceReservationsController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<ResourceReservation>> Get(string fromDate)
        {
            IQueryable<ResourceReservation> query = db.ResourceReservations;

            var result = DatePattern.Parse(fromDate);
            if (result.Success)
            {
                var date = result.Value;
                query = query.Where(rr => rr.Date >= date);
            }

            return await query.ToListAsync();
        }
    }
}