using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("holidays")]
    [ApiController]
    public class HolidaysController : Controller
    {
        private readonly ReservationsContext db;

        public HolidaysController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<LocalDate>> Get()
            => await db.Holidays.Select(h => h.Date).OrderBy(date => date).ToListAsync();
    }
}