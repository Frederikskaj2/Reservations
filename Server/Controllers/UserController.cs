using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly ReservationsContext db;
        private readonly ILogger logger;
        private readonly OrderService orderService;
        private readonly Random random = new Random();
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public UserController(
            ILogger<UserController> logger, IBackgroundWorkQueue<EmailService> backgroundWorkQueue,
            OrderService orderService, ReservationsContext db, SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.backgroundWorkQueue =
                backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<MyUser> GetMyUser()
        {
            var user = await FindUser();
            if (user == null)
                return new MyUser();
            var myUser = user.Adapt<MyUser>();
            myUser.Phone = user.PhoneNumber;
            myUser.IsEmailConfirmed = user.EmailConfirmed;
            myUser.IsAdministrator = await userManager.IsInRoleAsync(user, Roles.UserAdministration);
            return myUser;
        }

        [HttpGet("authenticated")]
        public async Task<AuthenticatedUser> GetAuthenticatedUser()
        {
            if (!User.Identity.IsAuthenticated)
                return AuthenticatedUser.UnknownUser;
            var user = await FindUser();
            if (user == null)
                return AuthenticatedUser.UnknownUser;
            return await GetAuthenticatedUser(user);
        }

        [HttpPost("sign-up")]
        public async Task<SignUpResponse> SignUp(SignUpRequest request)
        {
            var trimmedEmail = request.Email.Trim();
            logger.LogInformation("New user sign-up with email {Email}", trimmedEmail.MaskEmail());
            var user = new User
            {
                UserName = trimmedEmail,
                Email = trimmedEmail,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.Phone.Trim(),
                ApartmentId = request.ApartmentId
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
            var user = await FindUser(request.Email);
            if (user == null)
            {
                await WaitRandomDelay();
                return new SignInResponse { Result = SignInResult.InvalidEmailOrPassword };
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (!result.Succeeded)
                return new SignInResponse
                    { Result = result.IsLockedOut ? SignInResult.LockedOut : SignInResult.InvalidEmailOrPassword };

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

        [HttpPatch("")]
        [Authorize]
        public async Task<OperationResponse> Update(UpdateMyUserRequest request)
        {
            var user = await FindUser();
            if (user == null)
                return new OperationResponse { Result = OperationResult.GeneralError };

            user.FullName = request.FullName;
            user.PhoneNumber = request.Phone;

            var result = await userManager.UpdateAsync(user);
            return new OperationResponse
                { Result = result.Succeeded ? OperationResult.Success : OperationResult.GeneralError };
        }

        [HttpPost("update-password")]
        [Authorize]
        public async Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request)
        {
            var user = await FindUser();
            if (user == null)
                return new UpdatePasswordResponse { Errors = UpdatePasswordErrorCodes.GeneralError };

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
                return new UpdatePasswordResponse { Errors = result.ToUpdatePasswordErrors() };

            await signInManager.RefreshSignInAsync(user);

            return new UpdatePasswordResponse();
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

        [HttpPost("delete")]
        [Authorize]
        public async Task<DeleteUserResponse> Delete()
        {
            var user = await FindUser();
            if (user == null)
                return new DeleteUserResponse { Result = DeleteUserResult.GeneralError };

            logger.LogInformation("Request deletion of user with email {Email}", user.Email.MaskEmail());

            var isDeleted = await orderService.DeleteUser(user);
            return new DeleteUserResponse
                { Result = isDeleted ? DeleteUserResult.Success : DeleteUserResult.IsPendingDelete };
        }

        [HttpPost("confirm-email")]
        public async Task<OperationResponse> ConfirmEmail(ConfirmEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
                return new OperationResponse { Result = OperationResult.GeneralError };

            logger.LogInformation("Request confirmation of email {Email}", request.Email.MaskEmail());

            var user = await FindUser(request.Email);
            if (user == null)
                return new OperationResponse { Result = OperationResult.GeneralError };

            if (user.EmailConfirmed)
                return new OperationResponse { Result = OperationResult.GeneralError };

            var result = await userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
                return new OperationResponse { Result = OperationResult.GeneralError };

            return new OperationResponse { Result = OperationResult.Success };
        }

        [HttpPost("new-password")]
        public async Task<NewPasswordResponse> NewPassword(NewPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token))
                return new NewPasswordResponse { Errors = NewPasswordErrorCodes.GeneralError };

            var user = await FindUser(request.Email);
            if (user == null)
                return new NewPasswordResponse { Errors = NewPasswordErrorCodes.GeneralError };

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
                return new NewPasswordResponse { Errors = result.ToNewPasswordErrors() };

            return new NewPasswordResponse();
        }

        [HttpPost("send-reset-password-email")]
        public async Task<OperationResponse> SendResetPasswordEmail(SendResetPasswordEmailRequest request)
        {
            var user = await FindUser(request.Email);
            if (user != null)
                await SendPasswordResetEmail(user);

            return new OperationResponse { Result = OperationResult.Success };
        }

        [HttpGet("transactions")]
        [Authorize]
        public async Task<IEnumerable<MyTransaction>> GetTransactions()
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return Enumerable.Empty<MyTransaction>();

            return await db.Transactions
                .Where(transaction => transaction.UserId == userId.Value)
                .OrderBy(transaction => transaction.Id)
                .ProjectToType<MyTransaction>()
                .ToListAsync();
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
                Roles = roles,
                ApartmentId = user.ApartmentId
            };
        }

        private Task<User> FindUser(string email) => userManager.FindByNameAsync(email);

        private async Task<User?> FindUser()
        {
            var userId = User.Id();
            return userId.HasValue
                ? await userManager.FindByIdAsync(userId.Value.ToString(CultureInfo.InvariantCulture))
                : null;
        }

        private Task WaitRandomDelay()
        {
            var delayInMs = random.Next(200, 400);
            return Task.Delay(delayInMs);
        }
    }
}