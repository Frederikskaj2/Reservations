using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly TransactionService transactionService;
        private readonly UserManager<User> userManager;

        public OrderService(
            ILogger<OrderService> logger, IBackgroundWorkQueue<EmailService> backgroundWorkQueue,
            IDataProvider dataProvider, DateTimeZone dateTimeZone, ReservationsContext db,
            IReservationPolicyProvider reservationPolicyProvider, ReservationsOptions reservationsOptions,
            SignInManager<User> signInManager, TransactionService transactionService,
            UserManager<User> userManager)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.backgroundWorkQueue = backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.reservationPolicyProvider = reservationPolicyProvider ?? throw new ArgumentNullException(nameof(reservationPolicyProvider));
            this.reservationsOptions = reservationsOptions ?? throw new ArgumentNullException(nameof(reservationsOptions));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }


        public async Task<Order?> GetOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .ThenInclude(user => user!.AccountBalances)
                .Include(order => order.User)
                .ThenInclude(user => user!.Postings)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .ThenInclude(transaction => transaction.Amounts)
                .FirstOrDefaultAsync(order => order.Id == orderId && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));

        public async Task<IEnumerable<Order>> GetOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .ThenInclude(transaction => transaction.Amounts)
                .Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetOrders(int userId)
            => await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .ThenInclude(transaction => transaction.Amounts)
                .Where(order => order.UserId == userId && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetAllOrders(int userId)
            => await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .ThenInclude(transaction => transaction.Amounts)
                .Where(order => order.UserId == userId && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
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
            if (string.IsNullOrEmpty(accountNumber))
                throw new ArgumentException("Value cannot be null or empty.", nameof(accountNumber));
            if (reservations is null)
                throw new ArgumentNullException(nameof(reservations));

            var user = await GetUser(userId);
            if (user == null)
                return (PlaceOrderResult.GeneralError, null);

            user.AccountNumber = accountNumber;

            var resources = await dataProvider.GetResources();
            var order = new Order
            {
                User = user,
                Flags = OrderFlags.IsCleaningRequired,
                ApartmentId = apartmentId,
                CreatedTimestamp = timestamp,
                Reservations = new List<Reservation>(),
                Transactions = new List<Transaction>()
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
            transactionService.CreateOrderTransaction(timestamp, today, order, totalPrice);
            db.Orders.Add(order);

            await UpdateOrders(timestamp, today, user.Id, user, order);

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

            var prepaidAmount = order.Balance(Account.FromPayments);
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendOrderReceivedEmail(user, order.Id, prepaidAmount, totalPrice.GetTotal()));

            var users = await db.Users
                .Where(u => u.EmailSubscriptions.HasFlag(EmailSubscriptions.NewOrder))
                .ToListAsync();
            foreach (var u in users)
                backgroundWorkQueue.Enqueue((service, _) => service.SendNewOrderEmail(u, order.Id));

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
            if (string.IsNullOrEmpty(accountNumber))
                throw new ArgumentException("Value cannot be null or empty.", nameof(accountNumber));
            if (cancelledReservations is null)
                throw new ArgumentNullException(nameof(cancelledReservations));

            var order = await GetOrder(orderId);
            if (order == null)
                return default;

            var user = await GetUser(order.UserId!.Value);
            user.AccountNumber = accountNumber;

            var cancelledReservationsEmailData = new List<(Reservation Reservation, int Total, int CancellationFee)>();
            var confirmedOrderIds = Enumerable.Empty<int>();
            if (cancelledReservations.Any())
            {
                var today = timestamp.InZone(dateTimeZone).Date;
                foreach (var reservationId in cancelledReservations)
                {
                    var reservation = order.Reservations!.FirstOrDefault(r => r.Id == reservationId);
                    if (reservation == null
                        || !userId.HasValue && !reservation.CanBeCancelledByAdministrator()
                        || userId.HasValue && !reservation.CanBeCancelledUser(today, reservationsOptions))
                        continue;

                    var fee = reservation.Status == ReservationStatus.Confirmed && !waiveFee ? reservationsOptions.CancellationFee : 0;
                    transactionService.CreateReservationCancelledTransaction(timestamp, today, createdByUserId, order, reservation, fee);

                    if (reservation.Status == ReservationStatus.Reserved)
                        cancelledReservationsEmailData.Add((reservation, 0, 0));
                    else
                        cancelledReservationsEmailData.Add((reservation, reservation.Price!.GetTotal(), fee));

                    reservation.Status = ReservationStatus.Cancelled;
                    reservation.UpdatedTimestamp = timestamp;
                    reservation.Days!.Clear();
                }

                confirmedOrderIds = await UpdateOrders(timestamp, today, createdByUserId, user);
                await TryClearAccountNumber(user);
            }

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
            foreach (var (reservation, total, fee) in cancelledReservationsEmailData)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendReservationCancelledEmail(
                        order.User!, order.Id, resources[reservation.ResourceId].Name!, reservation.Date, total, fee));
            foreach (var confirmedOrderId in confirmedOrderIds)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(
                        order.User!, confirmedOrderId));

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

        public async Task<Order?> PayIn(Instant timestamp, int orderId, int userId, LocalDate date, string accountNumber, int amount)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return null;

            transactionService.CreatePayInTransaction(timestamp, date, userId, order, amount);
            var confirmedOrderIds = await UpdateOrders(timestamp, date, userId, order.User!);

            // This is required to avoid the situation where the account number is deleted just before a pay-in is registered.
            order.User!.AccountNumber = accountNumber;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to pay in");
                return null;
            }

            var accountsReceivable = order.Balance(Account.AccountsReceivable);
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendPayInEmail(order.User!, order.Id, amount, accountsReceivable));

            foreach (var confirmedOrderId in confirmedOrderIds)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(order.User!, confirmedOrderId));

            return order;
        }

        public async Task<Order?> Settle(
            Instant timestamp, int orderId, int userId, int reservationId, int damages, string? description)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return default;
            var reservation = order.Reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation == null || reservation.Status != ReservationStatus.Confirmed
                || !(0 <= damages && damages <= reservation.Price!.Deposit)
                || damages > 0 && string.IsNullOrWhiteSpace(description))
                return default;

            reservation.Status = ReservationStatus.Settled;

            var today = timestamp.InZone(dateTimeZone).Date;

            transactionService.CreateSettlementTransaction(timestamp, today, userId, order, reservation, damages, description);
            var confirmedOrderIds = await UpdateOrders(timestamp, today, userId, order.User!);

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

            foreach (var confirmedOrderId in confirmedOrderIds)
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendOrderConfirmedEmail(order.User!, confirmedOrderId));

            await TryDeleteUser(order.User!);

            return order;
        }

        public async Task<PayOut?> PayOut(Instant timestamp, int createdByUserId, int userId, LocalDate date, int amount)
        {
            var user = await GetUserWithTransactions(userId);
            if (user == null)
                return null;

            await ApplyPayOutToPayments(timestamp, date, createdByUserId, user, amount);
            await ConvertToHistoryOrders(user);
            await TryClearAccountNumber(user);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(exception, "Unable to update pay out");
                return null;
            }

            backgroundWorkQueue.Enqueue((service, _) => service.SendPayOutEmail(user, amount));

            await TryDeleteUser(user);

            var remainingAmount = -user.Balance(Account.Payments);
            return new PayOut
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.PhoneNumber,
                ApartmentId = user.ApartmentId!.Value,
                AccountNumber = user.AccountNumber ?? string.Empty,
                Amount = remainingAmount
            };
        }

        public async Task<IEnumerable<PayOut>> GetPayOuts()
        {
            var payments = await db.AccountBalances
                .Include(accountBalance => accountBalance.User)
                .Where(accountBalance => accountBalance.Account == Account.Payments && accountBalance.Amount < 0)
                .ToListAsync();

            return payments.Select(CreatePayout);

            static PayOut CreatePayout(AccountBalance payment) => new PayOut
            {
                UserId = payment.User!.Id,
                Email = payment.User!.Email,
                FullName = payment.User.FullName,
                Phone = payment.User.PhoneNumber,
                ApartmentId = payment.User.ApartmentId!.Value,
                AccountNumber = payment.User.AccountNumber!,
                Amount = -payment.Amount
            };
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
                foreach (var amount in transaction.Amounts!)
                    switch (amount.Account)
                    {
                        case Account.Rent:
                        case Account.Cleaning:
                        case Account.Deposits when amount.Amount < 0:
                            totals.Price += -amount.Amount;
                            break;
                        case Account.CancellationFees:
                            totals.CancellationFee += -amount.Amount;
                            break;
                        case Account.Damages:
                            totals.Damages += -amount.Amount;
                            totals.DamagesDescription = transaction.Description;
                            break;
                        case Account.Bank when amount.Amount > 0:
                            totals.PayIn += amount.Amount;
                            break;
                        case Account.Bank when amount.Amount < 0:
                            totals.PayOut += -amount.Amount;
                            break;
                        case Account.AccountsReceivable:
                            break;
                        case Account.FromPayments:
                            totals.FromOtherOrders += amount.Amount;
                            break;
                        case Account.Deposits when amount.Amount > 0:
                            totals.RefundedDeposits += amount.Amount;
                            break;
                        case Account.Payments:
                            break;
                        case Account.ToAccountsReceivable:
                            totals.ToOtherOrders += -amount.Amount;
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

        private async Task<IEnumerable<int>> UpdateOrders(Instant timestamp, LocalDate date, int createdByUserId, User user, Order? newOrder = null)
        {
            await ApplyPaymentsToAccountsRecivables(timestamp, date, createdByUserId, user, newOrder);
            var confirmedOrderIds = await ConfirmOrders(user, newOrder);
            await ConvertToHistoryOrders(user);
            return confirmedOrderIds;
        }

        private async Task ApplyPaymentsToAccountsRecivables(Instant timestamp, LocalDate date, int createdByUserId, User user, Order? newOrder = null)
        {
            var payments = -user.Balance(Account.Payments);
            if (payments == 0)
                return;

            var orders = await GetOrders(user.Id);
            if (newOrder != null)
                orders = orders.Concat(new[] { newOrder });
            var tuples = orders
                .Select(order => new
                {
                    Order = order,
                    AccountsReceivable = order.Balance(Account.AccountsReceivable),
                    Payments = -order.Balance(Account.Payments)
                })
                .ToList();
            var ordersWithPayments = new Queue<Order>(tuples.Where(tuple => tuple.Payments > 0).Select(tuple => tuple.Order));
            foreach (var tuple in tuples.Where(tuple => tuple.AccountsReceivable > 0))
            {
                var nextOrderWithPayments = ordersWithPayments.Peek();
                transactionService.CreatePaymentsUsedTransaction(timestamp, date, createdByUserId, nextOrderWithPayments, tuple.Order);
                var paymentsRemaining = -nextOrderWithPayments.Balance(Account.Payments);
                if (paymentsRemaining == 0)
                    ordersWithPayments.Dequeue();
                if (ordersWithPayments.Count == 0)
                    break;
            }
        }

        private async Task<IEnumerable<int>> ConfirmOrders(User user, Order? newOrder = null)
        {
            var orders = await GetOrders(user.Id);
            if (newOrder != null)
                orders = orders.Concat(new[] { newOrder });
            var ordersToConfirm = orders
                .Where(order => order.Reservations.Any(reservation => reservation.Status == ReservationStatus.Reserved))
                .Select(order => new
                {
                    Order = order,
                    AccountsReceivable = order.Balance(Account.AccountsReceivable)
                })
                .Where(tuple => tuple.AccountsReceivable == 0)
                .Select(tuple => tuple.Order);
            var confirmedOrderIds = new List<int>();
            foreach (var order in ordersToConfirm)
            {
                ConfirmOrder(order);
                confirmedOrderIds.Add(order.Id);
            }
            return confirmedOrderIds;
        }

        private async Task ConvertToHistoryOrders(User user)
        {
            var orders = await GetOrders(user.Id);
            var ordersToConvert = orders
                .Where(order => order.Reservations.All(reservation => reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Settled))
                .Select(order => new
                {
                    Order = order,
                    Payments = order.Balance(Account.Payments)
                })
                .Where(tuple => tuple.Payments == 0)
                .Select(tuple => tuple.Order);
            foreach (var order in ordersToConvert)
            {
                order.Flags |= OrderFlags.IsHistoryOrder;
                foreach (var reservation in order.Reservations!)
                    reservation.Days!.Clear();
            }
        }

        private async Task ApplyPayOutToPayments(Instant timestamp, LocalDate date, int createdByUserId, User user, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var payments = -user.Balance(Account.Payments);
            if (amount > payments)
                throw new ReservationsException("Amount paid to user exceeds amount we owe them.");

            var orders = await GetOrders(user.Id);
            var tuples = orders
                .Select(order => (Order: order, Payments: -order.Balance(Account.Payments)))
                .Where(tuple => tuple.Payments > 0);
            var ordersWithPayments = new Queue<(Order Order, int Payments)>(tuples);
            while (amount > 0 && ordersWithPayments.Count > 0)
            {
                var tuple = ordersWithPayments.Peek();
                var amountToApplyToThisOrder = Math.Min(amount, tuple.Payments);
                transactionService.CreatePayOutTransaction(timestamp, date, createdByUserId, tuple.Order, amountToApplyToThisOrder);
                amount -= amountToApplyToThisOrder;
                if (amountToApplyToThisOrder == tuple.Payments)
                    ordersWithPayments.Dequeue();
            }
            if (amount > 0)
            {
                // Pathological case where a all orders are history orders. This
                // can happen if a pay-in is registered on an order that was
                // cancelled while the pay-in was registered.
                var latestOrder = await db.Orders
                    .Where(order => order.UserId == user.Id && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                    .OrderByDescending(order => order.Reservations.Max(reservation => reservation.UpdatedTimestamp))
                    .FirstAsync();
                transactionService.CreatePayOutTransaction(timestamp, date, createdByUserId, latestOrder, amount);
            }

            transactionService.CreatePosting(date, user, PostingType.PayOut);
        }

        private async Task TryClearAccountNumber(User user)
        {
            var activeOrders = await db.Orders
                .Where(order => order.UserId == user.Id && !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                .ToListAsync();
            var areThereAnyActiveOrdersWhenIncludingTrackedEntities = activeOrders
                .Any(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder) && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder));
            if (!areThereAnyActiveOrdersWhenIncludingTrackedEntities)
                user.AccountNumber = null;
        }

        private static void ConfirmOrder(Order order)
        {
            foreach (var reservation in order.Reservations.Where(reservation => reservation.Status == ReservationStatus.Reserved))
                reservation.Status = ReservationStatus.Confirmed;
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

        private Task<User> GetUser(int userId)
            => db.Users
                .Include(user => user.AccountBalances)
                .Where(user => user.Id == userId)
                .FirstOrDefaultAsync();

        private Task<User> GetUserWithTransactions(int userId)
            => db.Users
                .Include(user => user.AccountBalances)
                .Include(user => user.Transactions)
                .ThenInclude(transaction => transaction.Amounts)
                .Include(user => user.Postings)
                .Where(user => user.Id == userId)
                .FirstOrDefaultAsync();
    }
}