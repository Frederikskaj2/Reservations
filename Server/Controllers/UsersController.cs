﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("users")]
    [ApiController]
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "The framework ensures that the action arguments are non-null.")]
    public class UsersController : Controller
    {
        private readonly IClock clock;
        private readonly ReservationsContext db;
        private readonly OrderService orderService;
        private readonly UserManager<User> userManager;

        public UsersController(IClock clock, ReservationsContext db, OrderService orderService, UserManager<User> userManager)
        {
            this.clock=clock ?? throw new ArgumentNullException(nameof(clock));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.orderService=orderService ?? throw new ArgumentNullException(nameof(orderService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        [Authorize(Roles = Roles.UserAdministration)]
        public async Task<IEnumerable<Shared.User>> Get()
            => await db.Users
                .Include(user => user.Orders)
                .Include(user => user.UserRoles)
                .ThenInclude(role => role.Role)
                .Select(
                    user => new Shared.User
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Phone = user.PhoneNumber,
                        IsEmailConfirmed = user.EmailConfirmed,
                        Roles = user.UserRoles.Select(role => role.Role!.Name),
                        IsPendingDelete = user.IsPendingDelete,
                        OrderCount = user.Orders!.Count(order => !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                    })
#pragma warning disable CA1304 // Specify CultureInfo
                .OrderBy(user => user.Email!.ToUpper())
#pragma warning restore CA1304 // Specify CultureInfo
                .ToListAsync();

        [HttpPatch("{userId:int}")]
        [Authorize(Roles = Roles.UserAdministration)]
        public async Task<UpdateUserResponse> Patch(int userId, UpdateUserRequest request)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return new UpdateUserResponse { Result = UpdateUserResult.GeneralError };

            var currentRoles = (await userManager.GetRolesAsync(user)).ToHashSet();
            var rolesToAdd = request.Roles.Where(role => !currentRoles.Contains(role));
            foreach (var role in rolesToAdd)
                await userManager.AddToRoleAsync(user, role);
            var rolesToRemove = currentRoles.Where(role => !request.Roles.Contains(role));
            foreach (var role in rolesToRemove)
                await userManager.RemoveFromRoleAsync(user, role);
            CleanupEmailSubscriptions(user, request.Roles);

            user.FullName = request.FullName;
            user.PhoneNumber = request.Phone;
            user.IsPendingDelete = request.IsPendingDelete;

            var response = new UpdateUserResponse();
            var deleteUser = user.IsPendingDelete && !await db.Orders.AnyAsync(order => order.UserId == userId);
            if (deleteUser)
            {
                await userManager.DeleteAsync(user);
                response.Result = UpdateUserResult.UserWasDeleted;
            }
            else
            {
                response.User = new Shared.User
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.PhoneNumber,
                    IsEmailConfirmed = user.EmailConfirmed,
                    Roles = request.Roles,
                    IsPendingDelete = user.IsPendingDelete,
                    OrderCount = await db.Orders.Where(order => order.UserId == user.Id).CountAsync()
                };
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new UpdateUserResponse { Result = UpdateUserResult.GeneralError };
            }

            return response;

            static void CleanupEmailSubscriptions(User user, HashSet<string> roles)
            {
                if (user.EmailSubscriptions.HasFlag(EmailSubscriptions.NewOrder) && !(roles.Contains(Roles.OrderHandling) || roles.Contains(Roles.Payment)))
                    user.EmailSubscriptions &= ~EmailSubscriptions.NewOrder;
                if (user.EmailSubscriptions.HasFlag(EmailSubscriptions.OverduePayment) && !(roles.Contains(Roles.OrderHandling) || roles.Contains(Roles.Payment)))
                    user.EmailSubscriptions &= ~EmailSubscriptions.OverduePayment;
                if (user.EmailSubscriptions.HasFlag(EmailSubscriptions.SettlementRequired) && !roles.Contains(Roles.Settlement))
                    user.EmailSubscriptions &= ~EmailSubscriptions.SettlementRequired;
                if (user.EmailSubscriptions.HasFlag(EmailSubscriptions.CleaningScheduleUpdated) && !roles.Contains(Roles.Cleaning))
                    user.EmailSubscriptions &= ~EmailSubscriptions.CleaningScheduleUpdated;
            }
        }

        [HttpPost("{userId:int}/pay-out")]
        [Authorize(Roles = Roles.Payment)]
        public async Task<PayOut> PayOut(int userId, PayOutRequest request)
        {
            var createdByUserId = User.Id();
            var now = clock.GetCurrentInstant();
            return await orderService.PayOut(now, createdByUserId!.Value, userId, request.Date, request.Amount);
        }
    }
}