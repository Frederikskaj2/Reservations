using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("key-codes")]
    [Authorize(Roles = Roles.KeyCodes)]
    [ApiController]
    public class KeyCodesController : Controller
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly KeyCodeService keyCodeService;
        private readonly UserManager<User> userManager;

        public KeyCodesController(IClock clock, DateTimeZone dateTimeZone, KeyCodeService keyCodeService, UserManager<User> userManager)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.keyCodeService = keyCodeService ?? throw new ArgumentNullException(nameof(keyCodeService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<IEnumerable<KeyCode>> Get()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            var keyCodes = await keyCodeService.GetKeyCodes(today);
            return keyCodes.AsQueryable().ProjectToType<KeyCode>();
        }

        [HttpPost("send")]
        public async Task<OperationResponse> Send()
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OperationResponse { Result = OperationResult.GeneralError };
            var user = await userManager.FindByIdAsync(userId.Value.ToString(CultureInfo.InvariantCulture));
            if (user == null)
                return new OperationResponse { Result = OperationResult.GeneralError };

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            await keyCodeService.SendKeyCodes(user, today);
            return new OperationResponse { Result = OperationResult.Success };
        }
    }
}