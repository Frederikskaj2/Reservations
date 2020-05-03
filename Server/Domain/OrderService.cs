﻿using System;
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
                .FirstOrDefaultAsync(order => order.Id == orderId && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

        public async Task<IEnumerable<Order>> GetOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetAllOrders(int userId)
            => await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .Where(order => order.UserId == userId && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetOrders(int userId)
            => await db.Orders
                .Include(order => order.Transactions)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Where(order => order.UserId == userId && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<Order?> GetOwnerOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .FirstOrDefaultAsync(order => order.Id == orderId && order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

        public async Task<IEnumerable<Order>> GetOwnerOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Where(order => order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetHistoryOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .Where(order => order.Flags.HasFlag(OrderFlags.IsHistoryOrder))
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
                Flags = OrderFlags.IsCleaningRequired,
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

            var today = timestamp.InZone(dateTimeZone).Date;
            order.Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Type = TransactionType.Order,
                    Date = today,
                    CreatedByUserId = userId,
                    Timestamp = timestamp,
                    UserId = userId,
                    Order = order,
                    Amount = -(totalPrice.Rent + totalPrice.CleaningFee)
                },
                new Transaction
                {
                    Type = TransactionType.Deposit,
                    Date = today,
                    CreatedByUserId = userId,
                    Timestamp = timestamp,
                    UserId = userId,
                    Order = order,
                    Amount = -totalPrice.Deposit
                }
            };

            var balance = await TryApplyBalance(timestamp, today, userId, order, totalPrice.GetTotal());

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
                (service, _) => service.SendOrderReceivedEmail(user, order.Id, balance, totalPrice.GetTotal()));

            var users = await db.Users
                .Where(u => u.EmailSubscriptions.HasFlag(EmailSubscriptions.NewOrder))
                .ToListAsync();
            foreach (var u in users)
                backgroundWorkQueue.Enqueue((service, _) => service.SendNewOrderEmail(user, order.Id));

            return (PlaceOrderResult.Success, order);
        }

        public async Task<(PlaceOrderResult Result, Order? Order)> PlaceOwnerOrder(
            Instant timestamp, int userId, IEnumerable<ReservationRequest> reservations, bool isCleaningRequired)
        {
            if (reservations is null)
                throw new ArgumentNullException(nameof(reservations));

            var resources = await dataProvider.GetResources();
            var order = new Order
            {
                UserId = userId,
                Flags = OrderFlags.IsOwnerOrder | (isCleaningRequired ? OrderFlags.IsCleaningRequired : OrderFlags.None),
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

            var wasOrderConfirmed = order.Reservations.Any(reservation => reservation.Status == ReservationStatus.Confirmed);

            var today = timestamp.InZone(dateTimeZone).Date;
            order.AccountNumber = accountNumber;
            var cancelledReservedReservations = new List<Reservation>();
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
                        Type = TransactionType.DepositCancellation,
                        Date = today,
                        CreatedByUserId = createdByUserId,
                        Timestamp = timestamp,
                        UserId = order.UserId,
                        OrderId = order.Id,
                        ResourceId = reservation.ResourceId,
                        ReservationDate = reservation.Date,
                        Amount = reservation.Price!.Deposit
                    });
                order.Transactions!.Add(
                    new Transaction
                    {
                        Type = TransactionType.OrderCancellation,
                        Date = today,
                        CreatedByUserId = createdByUserId,
                        Timestamp = timestamp,
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
                                Type = TransactionType.CancellationFee,
                                Date = today,
                                CreatedByUserId = createdByUserId,
                                Timestamp = timestamp,
                                UserId = order.UserId,
                                OrderId = order.Id,
                                ResourceId = reservation.ResourceId,
                                ReservationDate = reservation.Date,
                                Amount = -reservationsOptions.CancellationFee
                            });
                }
                else
                {
                    cancelledReservedReservations.Add(reservation);
                }
            }

            var totals = GetTotals(order);
            var isOrderConfirmed = await GetIsOrderConfirmed();

            async Task<bool> GetIsOrderConfirmed()
            {
                if (wasOrderConfirmed)
                    return false;
                var amountToPay = -totals.GetBalance();
                var balance = await TryApplyBalance(timestamp, today, order.UserId!.Value, order, amountToPay);
                return balance >= amountToPay;
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
                            reservation.Price!.GetTotal(), cancellationFee);
                    });
            foreach (var reservation in cancelledReservedReservations)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendReservationCancelledEmail(
                        order.User!, order.Id, resources[reservation.ResourceId].Name!, reservation.Date, 0, 0));
            if (isOrderConfirmed)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(order.User!, order.Id, 0, totals.GetBalance()));

            var isUserDeleted = await TryDeleteUser(order.User!);

            return (isUserDeleted, order);
        }

        public async Task<(bool IsOrderDeleted, Order? Order)> UpdateOwnerOrder(
            int orderId, IEnumerable<int> cancelledReservations, bool isCleaningRequired)
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

            if (isCleaningRequired)
                order.Flags |= OrderFlags.IsCleaningRequired;
            else
                order.Flags &= ~OrderFlags.IsCleaningRequired;

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

        public async Task<Order?> PayIn(Instant timestamp, int orderId, int userId, LocalDate date, int amount)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return null;

            var today = timestamp.InZone(dateTimeZone).Date;
            var totals = GetTotals(order);
            var amountToPay = -totals.GetBalance();
            var balance = await TryApplyBalance(timestamp, today, order.UserId!.Value, order, amountToPay, amount);
            var orderIsConfirmed = balance >= amountToPay;

            var transaction = new Transaction
            {
                Type = TransactionType.PayIn,
                Date = date,
                CreatedByUserId = userId,
                Timestamp = timestamp,
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

            if (orderIsConfirmed)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(
                        order.User!, order.Id, amount, balance - amountToPay));
            else
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendMissingPaymentEmail(
                        order.User!, order.Id, amount, amountToPay - balance));

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

            var today = timestamp.InZone(dateTimeZone).Date;
            await db.Transactions.AddAsync(
                new Transaction
                {
                    Type = TransactionType.SettlementDeposit,
                    Date = today,
                    CreatedByUserId = userId,
                    Timestamp = timestamp,
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
                        Type = TransactionType.SettlementDamages,
                        Date = today,
                        CreatedByUserId = userId,
                        Timestamp = timestamp,
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

            var resources = await dataProvider.GetResources();
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendReservationSettledEmail(
                    order.User!, order.Id, resources[reservation.ResourceId].Name!, reservation.Date,
                    reservation.Price.Deposit, damages, description));

            await TryDeleteUser(order.User!);

            return order;
        }

        public async Task<Order?> PayOut(Instant timestamp, int orderId, int userId, LocalDate date, int amount)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return null;

            var transaction = new Transaction
            {
                Type = TransactionType.PayOut,
                Date = date,
                CreatedByUserId = userId,
                Timestamp = timestamp,
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

            backgroundWorkQueue.Enqueue((service, _) => service.SendPayOutEmail(order.User!, order.Id, amount));

            await TryDeleteUser(order.User!);

            return order;
        }

        public async Task<IEnumerable<PayOut>> GetPayOuts()
        {
            var orders = await db.Orders
                .Include(order => order.User)
                .Include(order => order.Transactions)
                .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
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
                    case TransactionType.BalanceIn:
                        totals.BalanceIn += transaction.Amount;
                        break;
                    case TransactionType.BalanceOut:
                        totals.BalanceOut += -transaction.Amount;
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

            var hasOrders = await db.Orders.AnyAsync(order => order.UserId == user.Id && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder));
            if (hasOrders)
            {
                user.IsPendingDelete = true;
                await userManager.UpdateAsync(user);
                return false;
            }

            await ReallyDeleteUser(user);
            return true;
        }

        public async Task SendOverduePaymentEmails(LocalDate date)
        {
            var deadline = date.PlusDays(1 - reservationsOptions.PaymentDeadlineInDays).AtStartOfDayInZone(dateTimeZone)
                .ToInstant();
            var orders = await db.Orders
                .Include(order => order.Reservations)
                .Where(
                    order => order.CreatedTimestamp <= deadline
                        && order.Reservations.Any(
                            reservation => reservation.Status == ReservationStatus.Reserved
                                && !reservation.SentEmails.HasFlag(ReservationEmails.OverduePayment)))
                .ToListAsync();
            if (orders.Count == 0)
                return;

            var users = await db.Users
                .Where(user => user.EmailSubscriptions.HasFlag(EmailSubscriptions.OverduePayment))
                .ToListAsync();
            if (users.Count == 0)
                return;

            foreach (var order in orders)
            {
                foreach (var user in users)
                    backgroundWorkQueue.Enqueue(
                        (service, _) => service.SendOverduePaymentEmail(user, order.Id));
                var reservationsToUpdate = order.Reservations
                    .Where(reservation => reservation.Status == ReservationStatus.Reserved);
                foreach (var reservation in reservationsToUpdate)
                    reservation.SentEmails |= ReservationEmails.OverduePayment;
            }

            await db.SaveChangesAsync();
        }


        public async Task SendSettlementNeededEmails(LocalDate date)
        {
            var orders = await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Resource)
                .Where(
                    order => order.Reservations.Any(
                        reservation => reservation.Status == ReservationStatus.Confirmed && reservation.Date < date
                            && !reservation.SentEmails.HasFlag(ReservationEmails.NeedsSettlement)))
                .ToListAsync();
            // EF Core is not able to convert the NodaTime date arithmetic to SQL so the filtering is done client side instead.
            var reservations = orders.SelectMany(
                    order => order.Reservations.Where(
                        reservation => reservation.Date.PlusDays(reservation.DurationInDays) <= date))
                .ToList();
            if (reservations.Count == 0)
                return;

            var users = await db.Users
                .Where(user => user.EmailSubscriptions.HasFlag(EmailSubscriptions.SettlementRequired))
                .ToListAsync();
            if (users.Count == 0)
                return;

            foreach (var reservation in reservations)
            {
                foreach (var user in users)
                    backgroundWorkQueue.Enqueue(
                        (service, _) => service.SendSettlementNeededEmail(
                            user, reservation.OrderId, reservation.Resource!.Name,
                            reservation.Date.PlusDays(reservation.DurationInDays)));
                reservation.SentEmails |= ReservationEmails.NeedsSettlement;
            }

            await db.SaveChangesAsync();
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
            var price = policy.GetPrice(request.Date, request.DurationInDays).Adapt<Price>();
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

            static async Task<bool> IsReservationDurationValid(
                ReservationRequest reservation, IReservationPolicy policy)
            {
                var (minimumDays, maximumDays) =
                    await policy.GetReservationAllowedNumberOfDays(reservation.ResourceId, reservation.Date);
                return minimumDays <= reservation.DurationInDays && reservation.DurationInDays <= maximumDays;
            }
        }

        private async Task<int> TryApplyBalance(Instant timestamp, LocalDate today, int userId, Order order, int amountToPay, int payIn = 0)
        {
            var orders = await GetOrders(userId);
            if (order.Id != default)
                orders = orders.Where(order => order.Id != order.Id);
            var ordersWithPositiveBalance = orders
                .Select(order => new { Order = order, Balance = GetTotals(order).GetBalance() })
                .Where(tuple => tuple.Balance > 0);
            var balance = ordersWithPositiveBalance.Sum(tuple => tuple.Balance);

            if (balance + payIn >= amountToPay)
                foreach (var reservation in order.Reservations!.Where(reservation => reservation.Status == ReservationStatus.Reserved))
                    reservation.Status = ReservationStatus.Confirmed;

            amountToPay -= Math.Min(payIn, amountToPay);
            if (amountToPay > 0)
                foreach (var tuple in ordersWithPositiveBalance)
                {
                    var balanceToUse = Math.Min(amountToPay, tuple.Balance);
                    tuple.Order.Transactions!.Add(new Transaction
                    {
                        Type = TransactionType.BalanceOut,
                        Date = today,
                        CreatedByUserId = userId,
                        Timestamp = timestamp,
                        UserId = userId,
                        Order = tuple.Order,
                        Amount = -balanceToUse
                    });
                    order.Transactions!.Add(new Transaction
                    {
                        Type = TransactionType.BalanceIn,
                        Date = today,
                        CreatedByUserId = userId,
                        Timestamp = timestamp,
                        UserId = userId,
                        Order = tuple.Order,
                        Amount = balanceToUse
                    });
                    TryMakeHistoryOrder(tuple.Order, GetTotals(tuple.Order));
                    amountToPay -= balanceToUse;
                    if (amountToPay == 0)
                        break;
                }

            return balance + payIn;
        }

        private static void TryMakeHistoryOrder(Order order, OrderTotals totals)
        {
            var isHistoryOrder = totals.GetBalance() == 0 && order.Reservations.All(
                reservation => reservation.Status == ReservationStatus.Cancelled
                    || reservation.Status == ReservationStatus.Settled);
            if (!isHistoryOrder)
                return;

            order.Flags |= OrderFlags.IsHistoryOrder;
            order.AccountNumber = null;
            foreach (var reservation in order.Reservations!)
                reservation.Days!.Clear();
        }

        private async Task<bool> TryDeleteUser(User user)
        {
            if (!user.IsPendingDelete)
                return false;

            var hasOrders = await db.Orders.AnyAsync(order => order.UserId == user.Id && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder));
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