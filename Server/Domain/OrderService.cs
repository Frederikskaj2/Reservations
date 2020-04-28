using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
using Price = Frederikskaj2.Reservations.Server.Data.Price;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;
using ReservedDay = Frederikskaj2.Reservations.Server.Data.ReservedDay;
using Resource = Frederikskaj2.Reservations.Shared.Resource;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class OrderService
    {
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly IDataProvider dataProvider;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly ILogger logger;
        private readonly IReservationPolicyProvider reservationPolicyProvider;
        private readonly ReservationsOptions reservationsOptions;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public OrderService(
            ILogger<OrderService> logger, IBackgroundWorkQueue<EmailService> backgroundWorkQueue,
            IDataProvider dataProvider, DateTimeZone dateTimeZone, ReservationsContext db,
            IReservationPolicyProvider reservationPolicyProvider, ReservationsOptions reservationsOptions,
            SignInManager<User> signInManager, UserManager<User> userManager)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.backgroundWorkQueue =
                backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.reservationPolicyProvider
                = reservationPolicyProvider ?? throw new ArgumentNullException(nameof(reservationPolicyProvider));
            this.reservationsOptions =
                reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Order?> GetOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .FirstOrDefaultAsync(
                    order => order.Id == orderId && order.ApartmentId != null);

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
                .Where(order => order.UserId == userId && order.ApartmentId != null)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<Order?> GetOwnerOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .FirstOrDefaultAsync(
                    order => order.Id == orderId && order.ApartmentId == null && order.AccountNumber == null);

        public async Task<IEnumerable<Order>> GetOwnerOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Where(order => order.ApartmentId == null)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetHistoryOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .Where(order => order.ApartmentId != null && order.AccountNumber == null)
                .ToListAsync();

        public async Task<(PlaceOrderResult Result, Order? Order)> PlaceOrder(
            Instant timestamp, int userId, int apartmentId, string accountNumber,
            IEnumerable<ReservationRequest> reservations)
        {
            if (reservations is null)
                throw new ArgumentNullException(nameof(reservations));

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
                var (result, reservation) = await CreateReservation(
                    timestamp, order, reservationRequest, ReservationStatus.Reserved, resources);
                if (reservation == null)
                    return (result, null);
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
                logger.LogWarning(exception, "Unable to place order");
                if (exception.InnerException is SqliteException sqliteException
                    && sqliteException.SqliteErrorCode == 19)
                    return (PlaceOrderResult.ReservationConflict, null);
                return (PlaceOrderResult.GeneralError, null);
            }

            var user = await userManager.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendOrderReceivedEmail(user, order.Id, totalPrice.GetTotal()));

            return (PlaceOrderResult.Success, order);
        }

        public async Task<(PlaceOrderResult Result, Order? Order)> PlaceOwnerOrder(
            Instant timestamp, int userId, IEnumerable<ReservationRequest> reservations)
        {
            if (reservations is null)
                throw new ArgumentNullException(nameof(reservations));

            var resources = await dataProvider.GetResources();
            var order = new Order
            {
                UserId = userId,
                CreatedTimestamp = timestamp,
                Reservations = new List<Reservation>()
            };
            foreach (var reservationRequest in reservations)
            {
                var (result, reservation) = await CreateReservation(
                    timestamp, order, reservationRequest, ReservationStatus.Confirmed, resources);
                if (reservation == null)
                    return (result, null);
                order.Reservations.Add(reservation);
            }

            db.Orders.Add(order);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to place owner order");
                if (exception.InnerException is SqliteException sqliteException
                    && sqliteException.SqliteErrorCode == 19)
                    return (PlaceOrderResult.ReservationConflict, null);
                return (PlaceOrderResult.GeneralError, null);
            }

            return (PlaceOrderResult.Success, order);
        }

        public async Task<(bool IsUserDeleted, Order? Order)> UpdateOrder(
            Instant timestamp, int orderId, string accountNumber, IEnumerable<int> cancelledReservations,
            int createdByUserId, bool waiveFee, int? userId = null)
        {
            if (cancelledReservations is null)
                throw new ArgumentNullException(nameof(cancelledReservations));

            var order = await GetOrder(orderId);
            if (order == null || userId.HasValue && order.UserId != userId.Value)
                return default;

            var today = timestamp.InZone(dateTimeZone).Date;
            order.AccountNumber = accountNumber;
            var cancelledConfirmedReservations = new List<Reservation>();
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
                    cancelledConfirmedReservations.Add(reservation);
                    if (!waiveFee)
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

            TryMakeHistoryOrder(order, totals);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to update order");
                return default;
            }

            var resources = await dataProvider.GetResources();
            foreach (var reservation in cancelledConfirmedReservations)
                backgroundWorkQueue.Enqueue(
                    (service, _) =>
                    {
                        var cancellationFee = !waiveFee ? reservationsOptions.CancellationFee : 0;
                        return service.SendReservationCancelledEmail(
                            order.User!, order.Id, resources[reservation.ResourceId].Name!, reservation.Date,
                            reservation.Price!.Deposit, cancellationFee);
                    });
            if (orderIsConfirmed)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(order.User!, order.Id, 0, totals.GetBalance()));

            var isUserDeleted = await TryDeleteUser(order.User!);

            return (isUserDeleted, order);
        }

        public async Task<(bool IsOrderDeleted, Order? Order)> UpdateOwnerOrder(int orderId, IEnumerable<int> cancelledReservations)
        {
            if (cancelledReservations is null)
                throw new ArgumentNullException(nameof(cancelledReservations));

            var order = await GetOwnerOrder(orderId);
            if (order == null)
                return default;

            foreach (var reservationId in cancelledReservations)
            {
                var reservation = order.Reservations!.FirstOrDefault(r => r.Id == reservationId);
                if (reservation == null)
                    continue;
                order.Reservations!.Remove(reservation);
            }

            var isOrderDeleted = order.Reservations!.Count == 0;
            if (isOrderDeleted)
                db.Orders.Remove(order);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to update owner order");
                return default;
            }

            return !isOrderDeleted ? (false, order) : (true, default);
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
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to pay in");
                return null;
            }

            Debug.Assert(order.UserId != null, "order.UserId != null");
            var user = await userManager.FindByIdAsync(order.UserId.Value.ToString(CultureInfo.InvariantCulture));
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
                return default;

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
                        ResourceId = reservation.ResourceId,
                        ReservationDate = reservation.Date,
                        Comment = description,
                        Amount = -damages
                    });

            TryMakeHistoryOrder(order, GetTotals(order));

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to settle order");
                return default;
            }

            Debug.Assert(order.UserId != null, "order.UserId != null");
            var user = await userManager.FindByIdAsync(order.UserId.Value.ToString(CultureInfo.InvariantCulture));
            var resources = await dataProvider.GetResources();
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendReservationSettledEmail(
                    user, order.Id, resources[reservation.ResourceId].Name!, reservation.Date,
                    reservation.Price.Deposit, damages, description));

            await TryDeleteUser(order.User!);

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

            TryMakeHistoryOrder(order, GetTotals(order));

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to update pay out");
                return null;
            }

            Debug.Assert(order.UserId != null, "order.UserId != null");
            var user = await userManager.FindByIdAsync(order.UserId.Value.ToString(CultureInfo.InvariantCulture));
            backgroundWorkQueue.Enqueue((service, _) => service.SendPayOutEmail(user, order.Id, amount));

            await TryDeleteUser(order.User!);

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

        [SuppressMessage(
            "Performance", "CA1822:Mark members as static",
            Justification = "This member is accessed through an injected instance.")]
        public OrderTotals GetTotals(Order order)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

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
                        throw new ArgumentOutOfRangeException(nameof(order), "Unknown transaction type.");
                }

            return totals;
        }

        public async Task<bool> DeleteUser(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var hasOrders = await db.Orders.AnyAsync(order => order.UserId == user.Id && order.AccountNumber != null);
            if (hasOrders)
            {
                user.IsPendingDelete = true;
                await userManager.UpdateAsync(user);
                return false;
            }

            await ReallyDeleteUser(user);
            return true;
        }

        private async Task<(PlaceOrderResult Result, Reservation? Reservation)> CreateReservation(
            Instant timestamp, Order order, ReservationRequest request, ReservationStatus status,
            IReadOnlyDictionary<int, Resource> resources)
        {
            if (!resources.TryGetValue(request.ResourceId, out var resource))
                return (PlaceOrderResult.GeneralError, null);
            var policy = reservationPolicyProvider.GetPolicy(resource.Type);
            if (!await IsReservationDurationValid(request, policy))
                return (PlaceOrderResult.ReservationConflict, null);

            var days = Enumerable
                .Range(0, request.DurationInDays)
                .Select(
                    i => new ReservedDay
                    {
                        Date = request.Date.PlusDays(i),
                        ResourceId = request.ResourceId
                    })
                .ToList();
            var price = (await policy.GetPrice(request.Date, request.DurationInDays)).Adapt<Price>();
            var reservation = new Reservation
            {
                Order = order,
                ResourceId = resource.Id,
                Status = status,
                UpdatedTimestamp = timestamp,
                Date = request.Date,
                DurationInDays = request.DurationInDays,
                Days = days,
                Price = price
            };
            return (PlaceOrderResult.Success, reservation);
            ;

            static async Task<bool> IsReservationDurationValid(
                ReservationRequest reservation, IReservationPolicy policy)
            {
                var (minimumDays, maximumDays) =
                    await policy.GetReservationAllowedNumberOfDays(reservation.ResourceId, reservation.Date);
                return minimumDays <= reservation.DurationInDays && reservation.DurationInDays <= maximumDays;
            }
        }

        private static void TryMakeHistoryOrder(Order order, OrderTotals totals)
        {
            var isHistoryOrder = totals.GetBalance() == 0 && order.Reservations.All(
                reservation => reservation.Status == ReservationStatus.Cancelled
                    || reservation.Status == ReservationStatus.Settled);
            if (!isHistoryOrder)
                return;

            order.AccountNumber = null;
            foreach (var reservation in order.Reservations!)
                reservation.Days!.Clear();
        }

        private async Task<bool> TryDeleteUser(User user)
        {
            if (!user.IsPendingDelete)
                return false;

            var hasOrders = await db.Orders.AnyAsync(order => order.UserId == user.Id && order.AccountNumber != null);
            if (hasOrders)
                return false;

            await ReallyDeleteUser(user);

            return true;
        }

        private async Task ReallyDeleteUser(User user)
        {
            await userManager.DeleteAsync(user);
            await signInManager.SignOutAsync();
            backgroundWorkQueue.Enqueue((service, _) => service.SendUserDeletedEmail(user));
        }
    }
}