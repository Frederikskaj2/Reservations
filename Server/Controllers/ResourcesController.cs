using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("resources")]
    [ApiController]
    public class ResourcesController : Controller
    {
        private readonly ReservationsContext db;

        public ResourcesController(ReservationsContext db) => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<Resource>> Get() => await db.Resources.ToListAsync();
    }
}