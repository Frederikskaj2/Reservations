using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Frederikskaj2.Reservations.Shared.SignInResult;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public UserController(
            UserManager<User> userManager, SignInManager<User> signInManager,
            IBackgroundWorkQueue<EmailService> backgroundWorkQueue)
        {
            this.backgroundWorkQueue =
                backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        [HttpGet("")]
        public async Task<UserResponse> GetUser()
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new UserResponse();
            var user = await userManager.FindByIdAsync(userId.Value.ToString());
            var apiUser = user.Adapt<Shared.User>();
            apiUser.IsEmailConfirmed = user.EmailConfirmed;
            apiUser.IsAdministrator = await userManager.IsInRoleAsync(user, Roles.Administrator);
            return new UserResponse { User = apiUser };
        }

        [HttpGet("authenticated")]
        public async Task<AuthenticatedUser> GetAuthenticatedUser()
        {
            if (!User.Identity.IsAuthenticated)
                return AuthenticatedUser.UnknownUser;
            var userId = User.Id();
            if (!userId.HasValue)
                return AuthenticatedUser.UnknownUser;
            var user = await userManager.FindByIdAsync(userId.Value.ToString());
            return await GetAuthenticatedUser(user);
        }

        [Authorize]
        [HttpGet("apartment")]
        public async Task<ApartmentResponse> GetApartment()
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new ApartmentResponse();
            var user = await userManager.FindByIdAsync(userId.Value.ToString());
            return new ApartmentResponse { ApartmentId = user?.ApartmentId };
        }

        [HttpPost("sign-up")]
        public async Task<SignUpResponse> SignUp(SignUpRequest request)
        {
            var trimmedEmail = request.Email.Trim();
            var user = new User
            {
                UserName = trimmedEmail,
                Email = trimmedEmail,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.Phone.Trim(),
                ApartmentId = request.ApartmentId
            };
            var result = await userManager.CreateAsync(user, request.Password);
            // TODO: Handle duplicate email error.
            if (!result.Succeeded)
                return new SignUpResponse { Result = SignUpResult.GeneralError };

            await SendConfirmEmailEmail(user);

            var authenticationProperties = new AuthenticationProperties { IsPersistent = request.IsPersistent };
            await signInManager.SignInAsync(user, authenticationProperties);

            var u1 = User;
            var u2 = HttpContext.User;
            return new SignUpResponse { User = await GetAuthenticatedUser(user) };
        }

        [HttpPost("sign-in")]
        public async Task<SignInResponse> SignIn(SignInRequest request)
        {
            var user = await FindUser(request.Email!);
            if (user == null)
                // TODO: Consider adding security delay here.
                return new SignInResponse { Result = SignInResult.InvalidEmailOrPassword };

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return new SignInResponse { Result = SignInResult.InvalidEmailOrPassword };

            var authenticationProperties = new AuthenticationProperties { IsPersistent = request.IsPersistent };
            await signInManager.SignInAsync(user, authenticationProperties);

            return new SignInResponse { User = await GetAuthenticatedUser(user) };
        }

        [HttpPost("sign-out")]
        public Task SignOut() => signInManager.SignOutAsync();

        [HttpPost("sign-out-everywhere-else")]
        [Authorize]
        public async Task<OperationResponse> SignOutEverywhereElse()
        {
            var user = await FindUser();
            if (user == null)
                return new OperationResponse { Result = OperationResult.GeneralError };

            var result = await userManager.UpdateSecurityStampAsync(user);
            if (!result.Succeeded)
                return new OperationResponse { Result = OperationResult.GeneralError };

            await signInManager.RefreshSignInAsync(user);

            return new OperationResponse { Result = OperationResult.Success };
        }

        [HttpPost("update-password")]
        [Authorize]
        public async Task<OperationResponse> UpdatePassword(UpdatePasswordRequest request)
        {
            var user = await FindUser();
            if (user == null)
                return new OperationResponse { Result = OperationResult.GeneralError };

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return new OperationResponse { Result = OperationResult.GeneralError };

            await signInManager.RefreshSignInAsync(user);

            return new OperationResponse { Result = OperationResult.Success };
        }

        [HttpPost("confirm-email")]
        public async Task<TokenValidationResponse> ConfirmEmail(ConfirmEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
                return new TokenValidationResponse { Result = TokenValidationResult.Failure };

            var user = await FindUser(request.Email);
            if (user == null)
                return new TokenValidationResponse { Result = TokenValidationResult.Failure };

            if (user.EmailConfirmed)
                return new TokenValidationResponse { Result = TokenValidationResult.Success };

            var result = await userManager.ConfirmEmailAsync(user, request.Token);
            // TODO: Handle token expiration or simply error reporting.
            if (!result.Succeeded)
                return new TokenValidationResponse { Result = TokenValidationResult.Failure };

            return new TokenValidationResponse { Result = TokenValidationResult.Success };
        }

        [HttpPost("resend-confirm-email-email")]
        [Authorize]
        public async Task<OperationResponse> ResendConfirmEmailEmail()
        {
            var user = await FindUser();
            if (user == null)
                return new OperationResponse { Result = OperationResult.GeneralError };

            await SendConfirmEmailEmail(user);

            return new OperationResponse { Result = OperationResult.Success };
        }

        private async Task SendConfirmEmailEmail(User user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            backgroundWorkQueue.Enqueue((service, _) => service.SendConfirmEmail(user, token));
        }

        private async Task<AuthenticatedUser> GetAuthenticatedUser(User user)
        {
            var isAdministrator = await userManager.IsInRoleAsync(user, Roles.Administrator);
            return new AuthenticatedUser
            {
                Id = user.Id,
                Name = user.Email,
                IsAuthenticated = true,
                IsAdministrator = isAdministrator,
                ApartmentId = user.ApartmentId
            };
        }

        private Task<User> FindUser(string email) => userManager.FindByNameAsync(email);

        private async Task<User?> FindUser()
        {
            var userId = User.Id();
            return userId.HasValue ? await userManager.FindByIdAsync(userId.Value.ToString()) : null;
        }
    }
}