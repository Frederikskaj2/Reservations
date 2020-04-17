using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NodaTime.Text;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("postings")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    public class PostingsController : Controller
    {
        private static readonly LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
        private readonly PostingsService postingsService;

        public PostingsController(PostingsService postingsService)
            => this.postingsService = postingsService ?? throw new ArgumentNullException(nameof(postingsService));

        [HttpGet]
        public Task<IEnumerable<Posting>> Get(string month)
        {
            var result = pattern.Parse(month);
            return postingsService.GetPostings(result.Value);
        }

        [HttpGet("range")]
        public Task<PostingsRange> Get() => postingsService.GetPostingsRange();
    }
}