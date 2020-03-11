using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Passwords;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SignInResult = Frederikskaj2.Reservations.Shared.SignInResult;

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
        public UserInfo GetUser() => GetUser(User.Identity);

        [Authorize]
        [HttpGet("apartment")]
        public async Task<ApartmentResponse> GetApartment()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = nameIdentifierClaim != null && int.TryParse(nameIdentifierClaim.Value, out var id) ? (int?) id : null;
            if (!userId.HasValue)
                return new ApartmentResponse();
            var user = await db.Users.FindAsync(userId.Value);
            return new ApartmentResponse { ApartmentId = user?.ApartmentId };
        }

        [HttpPost("sign-up")]
        public async Task<SignUpResponse> SignUp([FromBody] SignUpRequest request)
        {
            var user = new User
            {
                Email = request.Email.Trim().ToLowerInvariant(),
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                HashedPassword = passwordHasher.HashPassword(request.Password),
                ApartmentId = request.ApartmentId
            };
            await db.Users.AddAsync(user);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is SqliteException sqliteException && sqliteException.SqliteErrorCode == 19)
                    return new SignUpResponse { Result = SignUpResult.DuplicateEmail };
                return new SignUpResponse { Result = SignUpResult.GeneralError };
            }

            var identity = await SignIn(user, request.IsPersistent);

            return new SignUpResponse { User = GetUser(identity, user) };
        }

        [HttpPost("sign-in")]
        public async Task<SignInResponse> SignIn([FromBody] SignInRequest request)
        {
            var email = request.Email!.ToLowerInvariant();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                passwordHasher.DelayAsIfAPasswordIsBeingChecked(request.Password!);
                return new SignInResponse { Result = SignInResult.InvalidEmailOrPassword };
            }

            var verificationResult = passwordHasher.VerifyHashedPassword(user.HashedPassword, request.Password!);

            if (verificationResult == PasswordVerificationResult.Failed)
                return new SignInResponse { Result = SignInResult.InvalidEmailOrPassword };

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.HashedPassword = passwordHasher.HashPassword(request.Password!);
                await db.SaveChangesAsync();
            }

            var identity = await SignIn(user, request.IsPersistent);

            return new SignInResponse { User = GetUser(identity, user) };
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        private UserInfo GetUser(IIdentity identity, User? user = null)
        {
            if (!identity.IsAuthenticated)
                return UnknownUser;
            return new UserInfo
            {
                Name = identity.Name, IsAuthenticated = true,
                IsAdministrator = User.HasClaim(ClaimTypes.Role, "Administrator"),
                ApartmentId = user?.ApartmentId
            };
        }

        private async Task<ClaimsIdentity> SignIn(User user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", user.FullName)
            };
            if (user.IsAdministrator)
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authenticationProperties = new AuthenticationProperties { IsPersistent = isPersistent };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);

            return claimsIdentity;
        }
    }
}