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
using NodaTime;
using Order = Frederikskaj2.Reservations.Shared.Order;
using Reservation = Frederikskaj2.Reservations.Shared.Reservation;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("orders")]
    [Authorize]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IClock clock;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly ReservationsOptions reservationsOptions;

        public OrdersController(ReservationsContext db, ReservationsOptions reservationsOptions, IClock clock, DateTimeZone dateTimeZone)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.reservationsOptions = reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            var orders = await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return orders.Select(order => CreateOrder(order, today)).ToList();
        }

        [HttpGet("{orderId:int}")]
        public async Task<Order> Get(int orderId)
        {
            var order = await db.Orders
                .Include(o => o.User)
                .Include(o => o.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(o => o.Transactions)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return CreateOrder(order, today);
        }

        [HttpPost("{orderId:int}/pay-in")]
        public async Task<OrderResponse> PayIn(int orderId, PaymentRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse();

            var order = await db.Orders
                .Include(o => o.User)
                .Include(o => o.Reservations)
                .ThenInclude(r => r.Days)
                .Include(o => o.Transactions)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return new OrderResponse();

            var existingPayInsForOrder = order.Transactions
                .Where(t => t.Type == TransactionType.PayIn)
                .Sum(t => t.Amount);

            var reservedReservations = order.Reservations
                .Where(reservation => reservation.Status == ReservationStatus.Reserved);
            var totalPrice = reservedReservations.Sum(reservation => reservation.Price!.GetTotal());
            var amountToPay = totalPrice - existingPayInsForOrder;

            if (request.Amount >= amountToPay)
            {
                foreach (var reservation in reservedReservations)
                    reservation.Status = ReservationStatus.Confirmed;
                // TODO: Send email about reservation status change.
            }

            if (request.Amount >= amountToPay)
            {
                // TODO: Register payout of excess pay-in.
            }
            else if (request.Amount < amountToPay)
            {
                // TODO: Send mail about missing payment.
            }

            var now = clock.GetCurrentInstant();
            var transaction = new Transaction
            {
                Timestamp = now,
                Type = TransactionType.PayIn,
                CreatedByUserId = userId.Value,
                UserId = order.UserId,
                OrderId = orderId,
                Amount = request.Amount
            };
            await db.Transactions.AddAsync(transaction);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new OrderResponse();
            }

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return new OrderResponse { Order = CreateOrder(order, today) };
        }

        [HttpPost("{orderId:int}/settle")]
        public async Task<OrderResponse> Settle(int orderId, SettleReservationRequest request)
        {
            var userId = User.Id();
            if (!userId.HasValue)
                return new OrderResponse();

            var order = await db.Orders
                .Include(o => o.User)
                .Include(o => o.Reservations)
                .ThenInclude(r => r.Days)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return new OrderResponse();

            var reservation = order.Reservations.FirstOrDefault(r => r.Id == request.ReservationId);
            if (reservation == null)
                return new OrderResponse();
            if (!(0 <= request.Damages && request.Damages <= reservation.Price!.Deposit))
                return new OrderResponse();

            reservation.Status = ReservationStatus.Settled;

            var now = clock.GetCurrentInstant();
            await db.Transactions.AddAsync(
                new Transaction
                {
                    Timestamp = now,
                    Type = TransactionType.SettlementDeposit,
                    CreatedByUserId = userId.Value,
                    UserId = order.UserId,
                    OrderId = orderId,
                    Amount = reservation.Price!.Deposit
                });
            if (request.Damages > 0)
                await db.Transactions.AddAsync(
                    new Transaction
                    {
                        Timestamp = now,
                        Type = TransactionType.SettlementDamages,
                        CreatedByUserId = userId.Value,
                        UserId = order.UserId,
                        OrderId = orderId,
                        Amount = request.Damages
                    });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new OrderResponse();
            }

            var today = clock.GetCurrentInstant().InZone(dateTimeZone).Date;
            return new OrderResponse { Order = CreateOrder(order, today) };
        }

        private Order CreateOrder(Data.Order order, LocalDate today)
        {
            var reservations = order.Reservations.Select(CreateReservation).ToList();
            return new Order
            {
                Id = order.Id,
                CreatedTimestamp = order.CreatedTimestamp,
                Mail = order.User!.Email,
                FullName = order.User.FullName,
                Phone = order.User.PhoneNumber,
                AccountNumber = order.AccountNumber!,
                Reservations = reservations,
                Totals = new OrderTotals
                {
                    PayIn = order.Transactions.Where(transaction => transaction.Type == TransactionType.PayIn).Sum(transaction => transaction.Amount),
                    CancellationFee = order.Transactions.Where(transaction => transaction.Type == TransactionType.CancellationFee).Sum(transaction => transaction.Amount),
                    Damages = order.Transactions.Where(transaction => transaction.Type == TransactionType.SettlementDamages).Sum(transaction => transaction.Amount),
                    PayOut = order.Transactions.Where(transaction => transaction.Type == TransactionType.PayOut).Sum(transaction => transaction.Amount)
                }
            };

            Reservation CreateReservation(Data.Reservation reservation) => new Reservation
            {
                Id = reservation.Id,
                ResourceId = reservation.ResourceId,
                Status = reservation.Status,
                UpdatedTimestamp = reservation.UpdatedTimestamp,
                Price = reservation.Price!.Adapt<Shared.Price>(),
                Date = reservation.Days!.First().Date,
                DurationInDays = reservation.Days!.Count,
                CanBeCancelled = reservation.CanBeCancelled(today, reservationsOptions)
            };
        }
    }
}