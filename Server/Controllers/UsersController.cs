using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User = Frederikskaj2.Reservations.Shared.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("users")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly ReservationsContext db;

        public UsersController(ReservationsContext db) => this.db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpGet("")]
        public async Task<IEnumerable<User>> Get()
            => await db.Users.OrderBy(user => user.Email).ProjectToType<User>().ToListAsync();

        [HttpPatch("{userId:int}")]
        public async Task<UpdateUserResponse> Patch(int userId, UpdateUserRequest request)
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return new UpdateUserResponse { Result = UpdateUserResult.GeneralError };

            user.FullName = request.FullName;
            user.PhoneNumber = request.Phone;
            // TODO: Update administrator role.
            user.IsPendingDelete = request.IsPendingDelete;

            var response = new UpdateUserResponse();
            var deleteUser = user.IsPendingDelete && !await db.Orders.AnyAsync(order => order.UserId == userId);
            if (deleteUser)
            {
                db.Users.Remove(user);
                response.Result = UpdateUserResult.UserWasDeleted;
            }
            else
            {
                response.User = user.Adapt<User>();
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