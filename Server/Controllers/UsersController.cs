using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("users")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly ReservationsContext db;
        private readonly UserManager<User> userManager;

        public UsersController(ReservationsContext db, UserManager<User> userManager)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet("")]
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
                        IsAdministrator = user.UserRoles.Any(role => role.Role!.Name == Roles.Administrator),
                        IsPendingDelete = user.IsPendingDelete,
                        OrderCount = user.Orders!.Count
                    })
                .OrderBy(user => user.Email)
                .ToListAsync();

        [HttpPatch("{userId:int}")]
        public async Task<UpdateUserResponse> Patch(int userId, UpdateUserRequest request)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return new UpdateUserResponse { Result = UpdateUserResult.GeneralError };

            if (request.IsAdministrator)
                await userManager.AddToRoleAsync(user, Roles.Administrator);
            else
                await userManager.RemoveFromRoleAsync(user, Roles.Administrator);

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
                    IsAdministrator = request.IsAdministrator,
                    IsPendingDelete = user.IsPendingDelete,
                    OrderCount = await db.Orders.Where(order => order.UserId == user.Id).CountAsync()
                };
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return new UpdateUserResponse { Result = UpdateUserResult.GeneralError };
            }

            return response;
        }
    }
}