using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
using Price = Frederikskaj2.Reservations.Server.Data.Price;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;
using ReservedDay = Frederikskaj2.Reservations.Server.Data.ReservedDay;
using Resource = Frederikskaj2.Reservations.Shared.Resource;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    public class OrderService
    {
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly IDataProvider dataProvider;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly IReservationPolicyProvider reservationPolicyProvider;
        private readonly ReservationsOptions reservationsOptions;
        private readonly UserManager<User> userManager;

        public OrderService(
            IBackgroundWorkQueue<EmailService> backgroundWorkQueue, IDataProvider dataProvider,
            DateTimeZone dateTimeZone, ReservationsContext db, IReservationPolicyProvider reservationPolicyProvider,
            ReservationsOptions reservationsOptions, UserManager<User> userManager)
        {
            this.backgroundWorkQueue =
                backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.reservationPolicyProvider
                = reservationPolicyProvider ?? throw new ArgumentNullException(nameof(reservationPolicyProvider));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Order?> GetOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .FirstOrDefaultAsync(order => order.Id == orderId && order.ApartmentId != null && order.AccountNumber != null);

        public async Task<IEnumerable<Order>> GetOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .Where(order => order.ApartmentId != null && order.AccountNumber != null)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetOrders(int userId)
            => await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .Where(order => order.UserId == userId && order.ApartmentId != null && order.AccountNumber != null)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<Order?> GetOwnerOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .FirstOrDefaultAsync(order => order.Id == orderId && order.ApartmentId == null && order.AccountNumber == null);

        public async Task<IEnumerable<Order>> GetOwnerOrders()
        => await db.Orders
            .Include(order => order.User)
            .Include(order => order.Reservations)
            .ThenInclude(reservation => reservation.Days)
            .Where(order => order.ApartmentId == null)
            .OrderBy(order => order.CreatedTimestamp)
            .ToListAsync();

        public async Task<(PlaceOrderResult Result, Order? Order)> PlaceOrder(
            Instant timestamp, int userId, int apartmentId, string accountNumber,
            IEnumerable<ReservationRequest> reservations)
        {
            var resources = await dataProvider.GetResources();
            var order = new Order
            {
                UserId = userId,
                ApartmentId = apartmentId,
                AccountNumber = accountNumber,
                CreatedTimestamp = timestamp,
                Reservations = new List<Reservation>()
            };
            var totalPrice = new Price();
            foreach (var reservationRequest in reservations)
            {
                var reservation = await CreateReservation(timestamp, order, reservationRequest, ReservationStatus.Reserved, resources);
                if (reservation == null)
                    return (PlaceOrderResult.GeneralError, null);
                order.Reservations.Add(reservation);
                totalPrice.Accumulate(reservation.Price!);
            }

            order.Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Timestamp = timestamp,
                    Type = TransactionType.Order,
                    CreatedByUserId = userId,
                    UserId = userId,
                    Order = order,
                    Amount = -(totalPrice.Rent + totalPrice.CleaningFee)
                },
                new Transaction
                {
                    Timestamp = timestamp,
                    Type = TransactionType.Deposit,
                    CreatedByUserId = userId,
                    UserId = userId,
                    Order = order,
                    Amount = -totalPrice.Deposit
                }
            };

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is SqliteException sqliteException
                    && sqliteException.SqliteErrorCode == 19)
                    return (PlaceOrderResult.ReservationConflict, null);
                return (PlaceOrderResult.GeneralError, null);
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendOrderReceivedEmail(user, order.Id, totalPrice.GetTotal()));

            return (PlaceOrderResult.Success, order);
        }

        public async Task<(PlaceOrderResult Result, Order? Order)> PlaceOwnerOrder(
            Instant timestamp, int userId, IEnumerable<ReservationRequest> reservations)
        {
            var resources = await dataProvider.GetResources();
            var order = new Order
            {
                UserId = userId,
                CreatedTimestamp = timestamp,
                Reservations = new List<Reservation>()
            };
            foreach (var reservationRequest in reservations)
            {
                var reservation = await CreateReservation(timestamp, order, reservationRequest, ReservationStatus.Confirmed, resources);
                if (reservation == null)
                    return (PlaceOrderResult.GeneralError, null);
                order.Reservations.Add(reservation);
            }

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is SqliteException sqliteException
                    && sqliteException.SqliteErrorCode == 19)
                    return (PlaceOrderResult.ReservationConflict, null);
                return (PlaceOrderResult.GeneralError, null);
            }

            return (PlaceOrderResult.Success, order);
        }

        public async Task<Order?> UpdateOrder(
            Instant timestamp, int orderId, string accountNumber, IEnumerable<int> cancelledReservations,
            int createdByUserId, int? userId = null)
        {
            var order = await GetOrder(orderId);
            if (order == null || userId.HasValue && order.UserId != userId.Value)
                return default;

            var today = timestamp.InZone(dateTimeZone).Date;
            order.AccountNumber = accountNumber;
            var reservationsCancelledWithFee = new List<Reservation>();
            foreach (var reservationId in cancelledReservations)
            {
                var reservation = order.Reservations!.FirstOrDefault(r => r.Id == reservationId);
                if (reservation == null || userId.HasValue && !reservation.CanBeCancelled(today, reservationsOptions))
                    continue;

                var previousStatus = reservation.Status;
                reservation.Status = ReservationStatus.Cancelled;
                reservation.UpdatedTimestamp = timestamp;
                reservation.Days!.Clear();

                order.Transactions!.Add(
                    new Transaction
                    {
                        Timestamp = timestamp,
                        Type = TransactionType.DepositCancellation,
                        CreatedByUserId = createdByUserId,
                        UserId = order.UserId,
                        OrderId = order.Id,
                        ResourceId = reservation.ResourceId,
                        ReservationDate = reservation.Date,
                        Amount = reservation.Price!.Deposit
                    });
                order.Transactions!.Add(
                    new Transaction
                    {
                        Timestamp = timestamp,
                        Type = TransactionType.OrderCancellation,
                        CreatedByUserId = createdByUserId,
                        UserId = order.UserId,
                        OrderId = order.Id,
                        ResourceId = reservation.ResourceId,
                        ReservationDate = reservation.Date,
                        Amount = reservation.Price.Rent + reservation.Price.CleaningFee
                    });
                if (previousStatus == ReservationStatus.Confirmed)
                {
                    reservationsCancelledWithFee.Add(reservation);
                    order.Transactions.Add(
                        new Transaction
                        {
                            Timestamp = timestamp,
                            Type = TransactionType.CancellationFee,
                            CreatedByUserId = createdByUserId,
                            UserId = order.UserId,
                            OrderId = order.Id,
                            ResourceId = reservation.ResourceId,
                            ReservationDate = reservation.Date,
                            Amount = -reservationsOptions.CancellationFee
                        });
                }
            }

            // TODO: If all reservations are cancelled then convert order to history order.
            // TODO: Handle this conversion gracefully in the client.

            var totals = GetTotals(order);
            var orderIsConfirmed = false;
            if (totals.GetBalance() >= 0)
            {
                var reservedReservations =
                    order.Reservations.Where(reservation => reservation.Status == ReservationStatus.Reserved);
                foreach (var reservation in reservedReservations)
                {
                    reservation.Status = ReservationStatus.Confirmed;
                    orderIsConfirmed = true;
                }
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return default;
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            var resources = await dataProvider.GetResources();
            foreach (var reservation in reservationsCancelledWithFee)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendReservationCancelledEmail(
                        user, order.Id, resources[reservation.ResourceId].Name!, reservation.Date,
                        reservation.Price!.Deposit, reservationsOptions.CancellationFee));
            if (orderIsConfirmed)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(user, order.Id, 0, totals.GetBalance()));

            return order;
        }

        public async Task<Order?> UpdateOwnerOrder(int orderId, IEnumerable<int> cancelledReservations)
        {
            var order = await GetOwnerOrder(orderId);
            if (order == null)
                return default;

            var reservationsCancelledWithFee = new List<Reservation>();
            foreach (var reservationId in cancelledReservations)
            {
                var reservation = order.Reservations!.FirstOrDefault(r => r.Id == reservationId);
                if (reservation == null)
                    continue;
                order.Reservations!.Remove(reservation);
            }

            // TODO: If all reservations are cancelled then delete order.
            // TODO: Handle this conversion gracefully in the client.

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return default;
            }

            return order;
        }

        public async Task<Order?> PayIn(Instant timestamp, int orderId, int userId, int amount)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return null;

            var totals = GetTotals(order);
            var amountToPay = -totals.GetBalance();

            if (amount >= amountToPay)
            {
                var reservedReservations =
                    order.Reservations.Where(reservation => reservation.Status == ReservationStatus.Reserved);
                foreach (var reservation in reservedReservations)
                    reservation.Status = ReservationStatus.Confirmed;
            }

            var transaction = new Transaction
            {
                Timestamp = timestamp,
                Type = TransactionType.PayIn,
                CreatedByUserId = userId,
                UserId = order.UserId,
                OrderId = orderId,
                Amount = amount
            };
            await db.Transactions.AddAsync(transaction);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (amount >= amountToPay)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(user, order.Id, amount, amount - amountToPay));
            else
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendMissingPaymentEmail(user, order.Id, amount, amountToPay - amount));

            return order;
        }

        public async Task<Order?> Settle(
            Instant timestamp, int orderId, int userId, int reservationId, int damages, string? description)
        {
            var order = await GetOrder(orderId);
            var reservation = order?.Reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation == null || reservation.Status != ReservationStatus.Confirmed
                || !(0 <= damages && damages <= reservation.Price!.Deposit)
                || damages > 0 && string.IsNullOrWhiteSpace(description))
                return null;

            reservation.Status = ReservationStatus.Settled;

            await db.Transactions.AddAsync(
                new Transaction
                {
                    Timestamp = timestamp,
                    Type = TransactionType.SettlementDeposit,
                    CreatedByUserId = userId,
                    UserId = order!.UserId,
                    OrderId = orderId,
                    ResourceId = reservation.ResourceId,
                    ReservationDate = reservation.Date,
                    Amount = reservation.Price!.Deposit
                });
            if (damages > 0)
                await db.Transactions.AddAsync(
                    new Transaction
                    {
                        Timestamp = timestamp,
                        Type = TransactionType.SettlementDamages,
                        CreatedByUserId = userId,
                        UserId = order.UserId,
                        OrderId = orderId,
                        ResourceId = reservation.Id,
                        ReservationDate = reservation.Date,
                        Comment = description,
                        Amount = -damages
                    });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            var resources = await dataProvider.GetResources();
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendReservationSettledEmail(
                    user, order.Id, resources[reservation.ResourceId].Name!, reservation.Date,
                    reservation.Price.Deposit, damages, description));

            return order;
        }

        public async Task<Order?> PayOut(Instant timestamp, int orderId, int userId, int amount)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return null;

            var transaction = new Transaction
            {
                Timestamp = timestamp,
                Type = TransactionType.PayOut,
                CreatedByUserId = userId,
                UserId = order.UserId,
                OrderId = orderId,
                Amount = -amount
            };
            await db.Transactions.AddAsync(transaction);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null;
            }

            var user = await userManager.FindByIdAsync(userId.ToString());
            backgroundWorkQueue.Enqueue((service, _) => service.SendPayOutEmail(user, order.Id, amount));

            return order;
        }


        public async Task<IEnumerable<PayOut>> GetPayOuts()
        {
            var orders = await db.Orders
                .Include(order => order.User)
                .Include(order => order.Transactions)
                .Where(order => order.ApartmentId != null && order.AccountNumber != null)
                .ToListAsync();

            var payOuts = orders.Select(
                    order =>
                    {
                        Debug.Assert(order.ApartmentId != null, "order.ApartmentId != null");
                        return new PayOut
                        {
                            OrderId = order.Id,
                            Email = order.User!.Email,
                            FullName = order.User.FullName,
                            Phone = order.User.PhoneNumber,
                            ApartmentId = order.ApartmentId.Value,
                            AccountNumber = order.AccountNumber!,
                            Amount = GetTotals(order).GetBalance()
                        };
                    })
                .Where(payOut => payOut.Amount > 0);
            return payOuts;
        }

        public OrderTotals GetTotals(Order order)
        {
            var totals = new OrderTotals();
            foreach (var transaction in order.Transactions!)
                switch (transaction.Type)
                {
                    case TransactionType.Order:
                    case TransactionType.Deposit:
                    case TransactionType.OrderCancellation:
                    case TransactionType.DepositCancellation:
                        totals.Price += -transaction.Amount;
                        break;
                    case TransactionType.CancellationFee:
                        totals.CancellationFee += -transaction.Amount;
                        break;
                    case TransactionType.SettlementDeposit:
                        totals.SettledDeposits += transaction.Amount;
                        break;
                    case TransactionType.SettlementDamages:
                        totals.Damages += -transaction.Amount;
                        break;
                    case TransactionType.PayIn:
                        totals.PayIn += transaction.Amount;
                        break;
                    case TransactionType.PayOut:
                        totals.PayOut += -transaction.Amount;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return totals;
        }

        private async Task<Reservation?> CreateReservation(Instant timestamp, Order order, ReservationRequest reservation, ReservationStatus status, IReadOnlyDictionary<int, Resource> resources)
        {
            if (!resources.TryGetValue(reservation.ResourceId, out var resource))
                return null;
            var policy = reservationPolicyProvider.GetPolicy(resource.Type);
            if (!await IsReservationDurationValid(reservation, policy))
                return null;

            var days = Enumerable
                .Range(0, reservation.DurationInDays)
                .Select(
                    i => new ReservedDay
                    {
                        Date = reservation.Date.PlusDays(i),
                        ResourceId = reservation.ResourceId
                    })
                .ToList();
            var price = (await policy.GetPrice(reservation.Date, reservation.DurationInDays)).Adapt<Price>();
            return new Reservation
            {
                Order = order,
                ResourceId = resource.Id,
                Status = status,
                UpdatedTimestamp = timestamp,
                Date = reservation.Date,
                DurationInDays = reservation.DurationInDays,
                Days = days,
                Price = price
            };

            static async Task<bool> IsReservationDurationValid(ReservationRequest reservation, IReservationPolicy policy)
            {
                var (minimumDays, maximumDays) =
                    await policy.GetReservationAllowedNumberOfDays(reservation.ResourceId, reservation.Date);
                return minimumDays <= reservation.DurationInDays && reservation.DurationInDays <= maximumDays;
            }
        }
    }
}