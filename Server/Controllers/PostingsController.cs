using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime.Text;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("postings")]
    [Authorize(Roles = Roles.Bookkeeping)]
    [ApiController]
    public class PostingsController : Controller
    {
        private static readonly LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
        private readonly PostingsService postingsService;
        private readonly UserManager<User> userManager;

        public PostingsController(PostingsService postingsService, UserManager<User> userManager)
        {
            this.postingsService = postingsService ?? throw new ArgumentNullException(nameof(postingsService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public Task<IEnumerable<Posting>> Get(string month)
        {
            var result = pattern.Parse(month);
            return postingsService.GetPostings(result.Value);
        }

        [HttpGet("range")]
        public Task<PostingsRange> Get() => postingsService.GetPostingsRange();

        [HttpPost("send")]
        public async Task<IActionResult> Send(string month)
        {
            var result = pattern.Parse(month);
            var userId = User.Id();
            var user = await userManager.FindByIdAsync(userId!.Value.ToString(CultureInfo.InvariantCulture));
            await postingsService.SendPostingsEmail(user, result.Value);
            return NoContent();
        }
    }
}