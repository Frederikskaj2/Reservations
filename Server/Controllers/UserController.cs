using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Passwords;
using Frederikskaj2.Reservations.Server.State;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private static readonly UserInfo UnknownUser = new UserInfo { IsAuthenticated = false };
        private readonly ReservationsContext db;
        private readonly IPasswordHasher passwordHasher;

        public UserController(ReservationsContext db, IPasswordHasher passwordHasher)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

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


        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInRequest request)
        {
            var email = request.Email!.ToLowerInvariant();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                passwordHasher.DelayAsIfAPasswordIsBeingChecked(request.Password!);
                return BadRequest();
            }

            var verificationResult = passwordHasher.VerifyHashedPassword(user.HashedPassword, request.Password!);

            if (verificationResult == PasswordVerificationResult.Failed)
                return BadRequest();

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.HashedPassword = passwordHasher.HashPassword(request.Password!);
                await db.SaveChangesAsync();
            }

            return Ok();
        }

    }
}