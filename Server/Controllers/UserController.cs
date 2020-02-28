using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Passwords;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private static readonly UserInfo UnknownUser = new UserInfo { IsAuthenticated = false };
        private readonly IPasswordHasher passwordHasher;

        public UserController(IPasswordHasher passwordHasher)
            => this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));

        [HttpGet("")]
        public UserInfo GetUser() => User.Identity.IsAuthenticated
            ? new UserInfo { Name = User.Identity.Name, IsAuthenticated = true }
            : UnknownUser;


        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            var user = new User
            {
                Email = request.Email.ToLowerInvariant(),
                FullName = request.FullName,
                HashedPassword = passwordHasher.HashPassword(request.Password)
            };
            return Ok();
        }
    }
}