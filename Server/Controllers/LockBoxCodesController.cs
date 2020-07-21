using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("lock-box-codes")]
    [Authorize(Roles = Roles.LockBoxCodes)]
    [ApiController]
    public class LockBoxCodesController : Controller
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly LockBoxCodeService lockBoxCodeService;
        private readonly UserManager<User> userManager;

        public LockBoxCodesController(IClock clock, DateTimeZone dateTimeZone, LockBoxCodeService lockBoxCodeService, UserManager<User> userManager)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.lockBoxCodeService = lockBoxCodeService ?? throw new ArgumentNullException(nameof(lockBoxCodeService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public Task<IEnumerable<WeeklyLockBoxCodes>> Get()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return lockBoxCodeService.GetWeeklyLockBoxCodes(today);
        }

        [HttpPost("send")]
        public async Task<ActionResult> Send()
        {
            var userId = User.Id();
            var user = await userManager.FindByIdAsync(userId!.Value.ToString(CultureInfo.InvariantCulture));
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            await lockBoxCodeService.SendWeeklyLockBoxCodes(user, today);
            return NoContent();
        }
    }
}