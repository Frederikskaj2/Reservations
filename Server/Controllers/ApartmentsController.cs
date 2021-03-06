﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apartment = Frederikskaj2.Reservations.Shared.Apartment;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("apartments")]
    [ApiController]
    public class ApartmentsController : Controller
    {
        private readonly ReservationsContext db;

        public ApartmentsController(ReservationsContext db)
            => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet]
        public async Task<IEnumerable<Apartment>> Get()
            => await db.Apartments
                .OrderBy(a => a.Letter)
                .ThenBy(a => a.Story)
                .ThenBy(a => a.Side)
                .ProjectToType<Apartment>()
                .ToListAsync();
    }
}