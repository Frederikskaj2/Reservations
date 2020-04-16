using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("pay-outs")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    public class PayOutsController : Controller
    {
        private readonly OrderService orderService;

        public PayOutsController(OrderService orderService) => this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));

        [HttpGet]
        public Task<IEnumerable<PayOut>> Get() => orderService.GetPayOuts();
    }
}