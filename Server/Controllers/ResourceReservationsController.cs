using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.State;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("resource-reservations")]
    [ApiController]
    public class ResourceReservationsController : Controller
    {
        private readonly ReservationsContext db;

        public ResourceReservationsController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<ResourceReservation>> Get(DateTime fromDate)
            => await db.ResourceReservations.Where(rr => rr.Date >= fromDate).ToListAsync();
    }
}