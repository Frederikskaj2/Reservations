using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.State;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("reservations")]
    [ApiController]
    public class ReservationsController : Controller
    {
        private readonly ReservationsContext db;

        public ReservationsController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<Reservation>> Get() => await db.Reservations.ToListAsync();
    }
}