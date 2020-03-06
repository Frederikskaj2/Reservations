using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly ReservationsContext db;

        public OrdersController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<Order>> Get() => await db.Orders.ToListAsync();
    }
}