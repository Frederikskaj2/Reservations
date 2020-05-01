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
    [Route("cleaning-tasks")]
    [Authorize(Roles = Roles.KeyCodes)]
    [ApiController]
    public class CleaningTasksController : Controller
    {
        private readonly CleaningTaskService cleaningTaskService;
        private readonly IClock clock;
        private readonly IDataProvider dataProvider;
        private readonly DateTimeZone dateTimeZone;
        private readonly UserManager<User> userManager;

        public CleaningTasksController(
            CleaningTaskService cleaningTaskService, IClock clock, IDataProvider dataProvider,
            DateTimeZone dateTimeZone, UserManager<User> userManager)
        {
            this.cleaningTaskService =
                cleaningTaskService ?? throw new ArgumentNullException(nameof(cleaningTaskService));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<IEnumerable<CleaningTask>> Get()
        {
            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            var resources = await dataProvider.GetResources();
            var cleaningTasks = await cleaningTaskService.GetTasks(today);
            return cleaningTasks
                .OrderBy(task => task.Date)
                .ThenBy(task => resources[task.ResourceId].Sequence)
                .AsQueryable()
                .ProjectToType<CleaningTask>();
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
            await cleaningTaskService.SendCleaningTasksEmail(user, today);
            return new OperationResponse { Result = OperationResult.Success };
        }
    }
}