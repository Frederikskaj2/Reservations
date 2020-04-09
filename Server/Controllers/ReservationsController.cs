using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("reservations")]
    [Authorize(Roles = Roles.Administrator)]
    [ApiController]
    public class ReservationsController : Controller
    {
        private readonly OrderService orderService;

        public ReservationsController(OrderService orderService) => this.orderService = orderService;

        public async Task<IEnumerable<HistoryReservation>> Get()
        {
            var orders = await orderService.GetHistoryOrders();
            return orders
                .SelectMany(
                    order => order.Reservations.Where(reservation => reservation.Status == ReservationStatus.Settled),
                    (order, reservation) =>
                    {
                        Debug.Assert(order.ApartmentId != null, "order.ApartmentId != null");
                        return new HistoryReservation
                        {
                            Id = reservation.Id,
                            OrderId = order.Id,
                            ResourceId = reservation.ResourceId,
                            Date = reservation.Date,
                            DurationInDays = reservation.DurationInDays,
                            ApartmentId = order.ApartmentId.Value,
                            UserEmail = order.User!.Email,
                            UserName = order.User.FullName
                        };
                    })
                .OrderBy(reservation => reservation.Date)
                .ThenBy(reservation => reservation.ResourceId);
        }
    }
}