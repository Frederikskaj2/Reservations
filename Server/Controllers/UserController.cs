using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime;
using SignInResult = Frederikskaj2.Reservations.Shared.SignInResult;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("user")]
    [ApiController]
    [SuppressMessage(
        "Design", "CA1062:Validate arguments of public methods",
        Justification = "The framework ensures that the action arguments are non-null.")]
    public class UserController : Controller
    {
        private readonly IBackgroundWorkQueue<IEmailService> backgroundWorkQueue;
        private readonly IClock clock;
        private readonly ILogger logger;
        private readonly MyTransactionService myTransactionService;
        private readonly OrderService orderService;
        private readonly Random random = new Random();
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public UserController(
            ILogger<UserController> logger, IBackgroundWorkQueue<IEmailService> backgroundWorkQueue, IClock clock,
            MyTransactionService myTransactionService, OrderService orderService, SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.backgroundWorkQueue = backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.myTransactionService = myTransactionService ?? throw new ArgumentNullException(nameof(myTransactionService));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyUser()
        {
            if (!User.Id().HasValue)
                return Ok();
            var user = await FindUser();
            var myUser = user.Adapt<MyUser>();
            myUser.Phone = user.PhoneNumber;
            myUser.IsEmailConfirmed = user.EmailConfirmed;
            myUser.Roles = await userManager.GetRolesAsync(user);
            return Ok(myUser);
        }

        [HttpGet("authenticated")]
        public async Task<AuthenticatedUser> GetAuthenticatedUser()
        {
            if (!User.Identity.IsAuthenticated)
                return AuthenticatedUser.UnknownUser;
            var user = await FindUser();
            return await GetAuthenticatedUser(user);
        }

        [HttpPost("sign-up")]
        public async Task<SignUpResponse> SignUp(SignUpRequest request)
        {
            var trimmedEmail = request.Email.Trim();
            logger.LogInformation("New user sign-up with email {Email}", trimmedEmail.MaskEmail());
            var now = clock.GetCurrentInstant();
            var user = new User
            {
                UserName = trimmedEmail,
                Email = trimmedEmail,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.Phone.Trim(),
                ApartmentId = request.ApartmentId,
                Created = now,
                LatestSignIn = now
            };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return new SignUpResponse { Errors = result.ToSignUpErrors() };

            await SendConfirmEmailEmail(user);

            var authenticationProperties = new AuthenticationProperties { IsPersistent = request.IsPersistent };
            await signInManager.SignInAsync(user, authenticationProperties);

            return new SignUpResponse { User = await GetAuthenticatedUser(user) };
        }

        [HttpPost("sign-in")]
        public async Task<SignInResponse> SignIn(SignInRequest request)
        {
            var user = await TryFindUser(request.Email);
            if (user == null)
            {
                await WaitRandomDelay();
                return new SignInResponse { Result = SignInResult.InvalidEmailOrPassword };
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (!result.Succeeded)
                return new SignInResponse
                    { Result = result.IsLockedOut ? SignInResult.LockedOut : SignInResult.InvalidEmailOrPassword };

            user.LatestSignIn = clock.GetCurrentInstant();
            await userManager.UpdateAsync(user);

            var authenticationProperties = new AuthenticationProperties { IsPersistent = request.IsPersistent };
            await signInManager.SignInAsync(user, authenticationProperties);

            return new SignInResponse { User = await GetAuthenticatedUser(user) };
        }

        [HttpPost("sign-out")]
        public Task SignOut() => signInManager.SignOutAsync();

        [HttpPost("sign-out-everywhere-else")]
        [Authorize]
        public async Task<IActionResult> SignOutEverywhereElse()
        {
            var user = await FindUser();
            var result = await userManager.UpdateSecurityStampAsync(user);
            if (!result.Succeeded)
                return new StatusCodeResult(500);

            await signInManager.RefreshSignInAsync(user);
            return NoContent();
        }

        [HttpPatch("")]
        [Authorize]
        public async Task<IActionResult> Update(UpdateMyUserRequest request)
        {
            var user = await FindUser();
            user.FullName = request.FullName;
            user.PhoneNumber = request.Phone;
            user.EmailSubscriptions = request.EmailSubscriptions;

            await userManager.UpdateAsync(user);
            return NoContent();
        }

        [HttpPost("update-password")]
        [Authorize]
        public async Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request)
        {
            var user = await FindUser();
            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return new UpdatePasswordResponse { Errors = result.ToUpdatePasswordErrors() };

            await signInManager.RefreshSignInAsync(user);

            return new UpdatePasswordResponse();
        }

        [HttpPost("resend-confirm-email-email")]
        [Authorize]
        public async Task<IActionResult> ResendConfirmEmailEmail()
        {
            var user = await FindUser();
            await SendConfirmEmailEmail(user);
            return NoContent();
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<DeleteUserResponse> Delete()
        {
            var user = await FindUser();
            logger.LogInformation("Request deletion of user with email {Email}", user.Email.MaskEmail());
            var isDeleted = await orderService.DeleteUser(user);
            return new DeleteUserResponse
                { Result = isDeleted ? DeleteUserResult.Success : DeleteUserResult.IsPendingDelete };
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
                return BadRequest();

            logger.LogInformation("Request confirmation of email {Email}", request.Email.MaskEmail());

            var user = await TryFindUser(request.Email);
            if (user == null)
                return BadRequest();

            if (user.EmailConfirmed)
                return NoContent();

            var result = await userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
                return new StatusCodeResult(500);

            return NoContent();
        }

        [HttpPost("new-password")]
        public async Task<NewPasswordResponse> NewPassword(NewPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
                return new NewPasswordResponse { Errors = NewPasswordErrorCodes.GeneralError };

            var user = await TryFindUser(request.Email);
            if (user == null)
                return new NewPasswordResponse { Errors = NewPasswordErrorCodes.GeneralError };

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
                return new NewPasswordResponse { Errors = result.ToNewPasswordErrors() };

            return new NewPasswordResponse();
        }

        [HttpPost("send-reset-password-email")]
        public async Task<IActionResult> SendResetPasswordEmail(SendResetPasswordEmailRequest request)
        {
            var user = await TryFindUser(request.Email);
            if (user != null)
                await SendPasswordResetEmail(user);

            return NoContent();
        }

        [HttpGet("transactions")]
        [Authorize]
        public async Task<IEnumerable<MyTransaction>> GetTransactions()
        {
            var userId = User.Id();
            return await myTransactionService.GetMyTransactions(userId!.Value);
        }

        private async Task SendConfirmEmailEmail(User user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            backgroundWorkQueue.Enqueue((service, _) => service.SendConfirmEmail(user, token));
        }

        private async Task SendPasswordResetEmail(User user)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            backgroundWorkQueue.Enqueue((service, _) => service.SendResetPasswordEmail(user, token));
        }

        private async Task<AuthenticatedUser> GetAuthenticatedUser(User user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new AuthenticatedUser
            {
                Id = user.Id,
                Name = user.Email,
                IsAuthenticated = true,
                Roles = roles
            };
        }

        private async Task<User?> TryFindUser(string email) => await userManager.FindByNameAsync(email);

        private Task<User> FindUser()
        {
            var userId = User.Id();
            return userManager.FindByIdAsync(userId!.Value.ToString(CultureInfo.InvariantCulture));
        }

        private Task WaitRandomDelay()
        {
            var delayInMs = random.Next(200, 400);
            return Task.Delay(delayInMs);
        }
    }
}