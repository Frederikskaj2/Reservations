using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Importer.Input;
using Frederikskaj2.Reservations.Infrastructure.Persistence;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;
using static Frederikskaj2.Reservations.Application.UserBalanceFunctions;
using static LanguageExt.Prelude;
using Account = Frederikskaj2.Reservations.Importer.Input.Account;
using Apartment = Frederikskaj2.Reservations.Importer.Input.Apartment;
using CancellationFee = Frederikskaj2.Reservations.Application.CancellationFee;
using Damages = Frederikskaj2.Reservations.Application.Damages;
using LineItem = Frederikskaj2.Reservations.Application.LineItem;
using LockBoxCode = Frederikskaj2.Reservations.Application.LockBoxCode;
using Order = Frederikskaj2.Reservations.Application.Order;
using OrderAudit = Frederikskaj2.Reservations.Application.OrderAudit;
using OrderFlags = Frederikskaj2.Reservations.Importer.Input.OrderFlags;
using OwnerOrder = Frederikskaj2.Reservations.Application.OwnerOrder;
using Price = Frederikskaj2.Reservations.Shared.Core.Price;
using Reservation = Frederikskaj2.Reservations.Application.Reservation;
using ReservationEmails = Frederikskaj2.Reservations.Application.ReservationEmails;
using ReservationStatus = Frederikskaj2.Reservations.Shared.Core.ReservationStatus;
using Roles = Frederikskaj2.Reservations.Shared.Core.Roles;
using Transaction = Frederikskaj2.Reservations.Application.Transaction;
using User = Frederikskaj2.Reservations.Application.User;
using UserAudit = Frederikskaj2.Reservations.Application.UserAudit;
using UserOrder = Frederikskaj2.Reservations.Application.UserOrder;

namespace Frederikskaj2.Reservations.Importer;

class Importer : IHostedService
{
    const string deletedFullName = "Slettet";
    const string deletedPhone = "Slettet";
    const int lockBoxCodeCount = 10;
    const string partitionKey = "";
    const string singletonId = "";

    static readonly System.Collections.Generic.HashSet<int> ordersConfirmedWhenPlaced = new(new[] { 96, 244, 276 });

    static readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

    static readonly LocalDate lastPostingsV1Month = new(2022, 8, 1);

    readonly CosmosOptions cosmosOptions;
    readonly IHostApplicationLifetime hostApplicationLifetime;
    readonly ILogger logger;
    readonly IPersistenceContextFactory persistenceContextFactory;
    readonly IServiceProvider serviceProvider;
    readonly System.Collections.Generic.HashSet<UserId> userIds = new();

    Dictionary<int, ApartmentId> apartmentIdMap = null!;
    TransactionId nextTransactionId = new(1);
    OrderingOptions orderingOptions = null!;
    Dictionary<int, ResourceId> resourceIdMap = null!;

    public Importer(
        IOptions<CosmosOptions> cosmosOptions, ILogger<Importer> logger, IHostApplicationLifetime hostApplicationLifetime,
        IPersistenceContextFactory persistenceContextFactory, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this.persistenceContextFactory = persistenceContextFactory;
        this.serviceProvider = serviceProvider;

        this.cosmosOptions = cosmosOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Begin import");

        using var scope = serviceProvider.CreateScope();
        orderingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<OrderingOptions>>().Value;
        var dbContext = scope.ServiceProvider.GetRequiredService<ReservationsContext>();

        await ClearCosmos(cancellationToken);

        await InitializeApartmentIdMap(dbContext, cancellationToken);
        await InitializeResourceIdMap(dbContext, cancellationToken);

        var timestamp = SystemClock.Instance.GetCurrentInstant();
        await ImportNextIds(dbContext, cancellationToken);
        await ImportLockBoxCodes(dbContext, cancellationToken);
        await ImportUsers(dbContext, timestamp, cancellationToken);
        await ImportOrders(dbContext, timestamp, cancellationToken);
        await CreateNextTransactionId();
        await EqualizeAccounts();
        await ImportPostingsV1(dbContext);
        await ScheduleCleaning(orderingOptions);
        await UpdateUsers();

        hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("End import");
        return Task.CompletedTask;
    }

    async Task ClearCosmos(CancellationToken cancellationToken)
    {
        using var client = new CosmosClient(cosmosOptions.ConnectionString);
        var container = client.GetContainer(cosmosOptions.DatabaseId, cosmosOptions.ContainerId);
        logger.LogInformation("Deleting Cosmos container {Container} in database {Database}", container.Id, container.Database.Id);
        await container.DeleteContainerStreamAsync(cancellationToken: cancellationToken);
    }

    async Task InitializeApartmentIdMap(ReservationsContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing apartment ID map");
        var tupleToIdMap = Apartments.GetAll().ToDictionary(
            apartment => (apartment.Letter, apartment.Story, apartment.Side),
            apartment => apartment.ApartmentId);
        var apartments = await dbContext.Apartments.ToListAsync(cancellationToken);
        apartmentIdMap = apartments.ToDictionary(apartment => apartment.Id, GetApartmentIdCorrectingForWrongSide);

        ApartmentId GetApartmentIdCorrectingForWrongSide(Apartment apartment) =>
            tupleToIdMap.TryGetValue((apartment.Letter, GetStory(apartment.Story), apartment.Side), out var id)
                ? id
                : tupleToIdMap[(apartment.Letter, GetStory(apartment.Story), ApartmentSide.None)];

        static int? GetStory(int inputStory) => inputStory is not -1 ? inputStory : null;
    }

    [return: NotNullIfNotNull("inputApartmentId")]
    ApartmentId? GetApartmentId(int? inputApartmentId) => inputApartmentId.HasValue ? apartmentIdMap[inputApartmentId.Value] : null;

    async Task InitializeResourceIdMap(ReservationsContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing resource ID map");
        var nameToIdMap = Resources.GetAll().ToDictionary(kvp => kvp.Value.Name, kvp => kvp.Key);
        var resources = await dbContext.Resources.ToListAsync(cancellationToken);
        resourceIdMap = resources.ToDictionary(resource => resource.Id, resource => nameToIdMap[resource.Name]);
    }

    ResourceId GetResourceId(int inputResourceId) => resourceIdMap[inputResourceId];

    async Task ImportNextIds(ReservationsContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Importing next IDs");
        var userId = await dbContext.Users.MaxAsync(user => user.Id, cancellationToken);
        var orderId = await dbContext.Orders.MaxAsync(order => order.Id, cancellationToken);

        const string device = "Device";
        var persistenceContext = persistenceContextFactory.Create(partitionKey)
            .CreateItem(device, new NextId(device, 1))
            .CreateItem(nameof(Order), new NextId(nameof(Order), orderId))
            .CreateItem(nameof(User), new NextId(nameof(User), userId));
        await WritePersistenceContext(persistenceContext);
    }

    async Task ImportLockBoxCodes(ReservationsContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Importing lock box codes");

        var input = await dbContext.LockBoxCodes.ToListAsync(cancellationToken);
        var codes = input
            .GroupBy(code => code.ResourceId, (_, values) => values.OrderBy(code => code.Date).Take(lockBoxCodeCount))
            .SelectMany(codes => codes)
            .Select(CreateLockBoxCode)
            .OrderBy(code => code.ResourceId)
            .ThenBy(code => code.Date);
        var lockBoxCodes = new LockBoxCodes(codes.ToSeq());
        await WritePersistenceContext(persistenceContextFactory.Create(partitionKey).CreateItem(singletonId, lockBoxCodes));

        logger.LogInformation("Imported {Count} lock box codes", lockBoxCodes.Codes.Count);

        LockBoxCode CreateLockBoxCode(Input.LockBoxCode code) => new(GetResourceId(code.ResourceId), code.Date, new CombinationCode(code.Code).ToString());
    }

    async Task ImportUsers(ReservationsContext dbContext, Instant timestamp, CancellationToken cancellationToken)
    {
        logger.LogInformation("Importing users");
        var count = 0;

        var inputs = dbContext.Users
            .Include(user => user.UserRoles)!.ThenInclude(role => role.Role)
            .AsSingleQuery()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        await foreach (var input in inputs)
        {
            var userId = UserId.FromInt32(input.Id);
            var email = EmailAddress.FromString(input.Email);
            var normalizedEmail = EmailAddress.NormalizeEmail(input.Email);

            var user = new User(
                userId,
                Seq1(new EmailStatus(email, normalizedEmail, input.EmailConfirmed)),
                input.FullName,
                SanitizePhoneNumber(input.PhoneNumber),
                GetApartmentId(input.ApartmentId),
                new UserSecurity { HashedPassword = Convert.FromBase64String(input.PasswordHash).ToSeq() },
                GetRoles(input))
            {
                LatestSignIn = input.LatestSignIn,
                AccountNumber = input.AccountNumber,
                EmailSubscriptions = input.EmailSubscriptions,
                Audits = GetAudits(timestamp, input).ToSeq()
            };

            if (input.Id is 69) // Helle Nikolajsen
                user = user with { ApartmentId = null };

            var userEmail = new UserEmail(normalizedEmail, userId);

            var persistenceContext = persistenceContextFactory.Create(partitionKey).CreateItem(User.GetId(userId), user).CreateItem(normalizedEmail, userEmail);
            await WritePersistenceContext(persistenceContext);

            userIds.Add(userId);

            count += 1;
        }

        logger.LogInformation("Imported {Count} users", count);

        static string SanitizePhoneNumber(string phoneNumber)
        {
            try
            {
                var parsed = phoneNumberUtil.Parse(phoneNumber, "DK");
                var format = parsed.CountryCode is 45 ? PhoneNumberFormat.NATIONAL : PhoneNumberFormat.INTERNATIONAL;
                return phoneNumberUtil.Format(parsed, format);
            }
            catch (NumberParseException)
            {
                return phoneNumber;
            }
        }

        static Roles GetRoles(Input.User user)
        {
            var roles =
                user.UserRoles!.Any(role => role.Role!.Name is
                    Input.Roles.Bookkeeping or Input.Roles.Cleaning or Input.Roles.OrderHandling or Input.Roles.Payment or Input.Roles.UserAdministration)
                    ? Roles.None
                    : Roles.Resident;
            if (user.UserRoles!.Any(role => role.Role!.Name is Input.Roles.Bookkeeping))
                roles |= Roles.Bookkeeping;
            if (user.UserRoles!.Any(role => role.Role!.Name is Input.Roles.Cleaning))
                roles |= Roles.Cleaning;
            if (user.UserRoles!.Any(role => role.Role!.Name is Input.Roles.LockBoxCodes))
                roles |= Roles.LockBoxCodes;
            if (user.UserRoles!.Any(role => role.Role!.Name is Input.Roles.OrderHandling or Input.Roles.Payment))
                roles |= Roles.OrderHandling;
            if (user.UserRoles!.Any(role => role.Role!.Name is Input.Roles.UserAdministration))
                roles |= Roles.UserAdministration;

            // Helle Nikolajsen
            if (user.Id is 69)
                roles = Roles.OrderHandling | Roles.Bookkeeping;
            // Martin Liversage
            // Stefan Vrang
            else if (user.Id is 1 or 25)
                roles |= Roles.Resident;
            // Mark Goldmann Pedersen
            // Rima Taheri Abkenar
            // Maria Krogh
            // Helle Hellesen
            // Thomas Witling
            // Casper Rubæk Nielsen
            // Jeppe Hvedstrup mann
            // Michael Villads Vedel
            else if (user.Id is 24 or 89 or 6 or 111 or 20 or 13 or 29 or 56)
                roles &= ~Roles.Resident;
            return roles;
        }

        static IEnumerable<UserAudit> GetAudits(Instant timestamp, Input.User user)
        {
            var userId = UserId.FromInt32(user.Id);
            yield return new UserAudit(user.Created, userId, UserAuditType.SignUp);
            yield return new UserAudit(timestamp, null, UserAuditType.Import);
        }
    }

    async Task CreateDeletedUser(Instant timestamp, UserId userId)
    {
        logger.LogInformation("Creating deleted user with ID {UserId}", userId);
        var user = new User(
            userId,
            Empty,
            deletedFullName,
            deletedPhone,
            Shared.Core.Apartment.Deleted.ApartmentId,
            new UserSecurity(),
            default)
        {
            Flags = UserFlags.IsDeleted,
            Audits = Seq1(new UserAudit(timestamp, null, UserAuditType.Import))
        };

        var persistenceContext = persistenceContextFactory.Create(partitionKey).CreateItem(User.GetId(userId), user);
        await WritePersistenceContext(persistenceContext);

        userIds.Add(userId);
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This method will be used one time in production.")]
    async Task ImportOrders(ReservationsContext dbContext, Instant timestamp, CancellationToken cancellationToken)
    {
        logger.LogInformation("Importing orders");
        var count = 0;

        var inputs = dbContext.Orders
            .Include(order => order.Reservations)
            .Include(order => order.Transactions)!
            .ThenInclude(transaction => transaction.Amounts)
            .OrderBy(order => order.CreatedTimestamp)
            .AsSplitQuery()
            .AsAsyncEnumerable().WithCancellation(cancellationToken);
        await foreach (var input in inputs)
        {
            var orderId = OrderId.FromInt32(input.Id);
            var userId = UserId.FromInt32(input.UserId!.Value);

            if (!userIds.Contains(userId))
                await CreateDeletedUser(timestamp, userId);
            var administratorUserIds = input.Transactions!.Select(transaction => transaction.CreatedByUserId).Where(id => !userIds.Contains(id));
            foreach (var administratorUserId in administratorUserIds)
                await CreateDeletedUser(timestamp, administratorUserId);

            var order = new Order(
                orderId,
                userId,
                (Application.OrderFlags) input.Flags,
                input.CreatedTimestamp,
                CreateUserOrder(input),
                CreateOwnerOrder(input),
                input.Reservations!
                    .OrderBy(reservation => reservation.Date)
                    .ThenBy(reservation => reservation.Id)
                    .Select(reservation => CreateReservation(input, reservation))
                    .ToSeq(),
                Empty);
            if (!order.Flags.HasFlag(Application.OrderFlags.IsHistoryOrder) &&
                order.Reservations.All(reservation =>
                    reservation.Status is ReservationStatus.Abandoned or ReservationStatus.Cancelled or ReservationStatus.Settled))
                order = order with { Flags = order.Flags | Application.OrderFlags.IsHistoryOrder };

            var rawTransactions = input.Transactions!
                .OrderBy(transaction => transaction.Date)
                .ThenBy(transaction => transaction.Id)
                .ToList();
            var orderIsConfirmed = rawTransactions.Any(transaction =>
                transaction.Amounts!.Any(amount => amount.Account is Account.Bank && amount.Amount > 0) && transaction.Amounts!.Count is 2 or 3);
            List<Transaction> transactions = rawTransactions
                .Select(transaction =>
                    CreateTransaction(transaction, order, TryGetReservation(transaction.ResourceId, transaction.ReservationDate, order), orderIsConfirmed))
                .Where(transaction => transaction is not null)
                .ToList()!;

            var orderWithAudits = order with { Audits = GetOrderAudits(input, transactions).ToSeq() };

            var persistenceContext = persistenceContextFactory.Create(partitionKey)
                .CreateItem(Order.GetId(orderId), orderWithAudits)
                .CreateItems(transactions, transaction => Transaction.GetId(transaction.TransactionId));
            await WritePersistenceContext(persistenceContext);

            count += 1;
        }

        logger.LogInformation("Imported {Count} orders", count);

        UserOrder? CreateUserOrder(Input.Order order) =>
            !order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
                ? new UserOrder(GetApartmentId(order.ApartmentId!.Value).Value, null, GetAdditionalLineItems(order).ToSeq())
                : null;

        static IEnumerable<LineItem> GetAdditionalLineItems(Input.Order order) => order.Transactions!
            .Select(transaction => GetAdditionalLineItem(order, transaction)).Where(lineItem => lineItem is not null)!;

        static LineItem? GetAdditionalLineItem(Input.Order order, Input.Transaction transaction)
        {
            if (transaction.Description is not null)
                return new LineItem(
                    transaction.Timestamp,
                    LineItemType.Damages,
                    null,
                    new Damages(GetReservationIndex(order, transaction.ResourceId!.Value, transaction.ReservationDate!.Value), transaction.Description),
                    transaction.Amounts!.Single(amount => amount.Account is Account.Damages).Amount);
            var fee = transaction.Amounts!.Where(amount => amount.Account is Account.CancellationFees).Sum(amount => amount.Amount);
            if (fee is not 0)
                return new LineItem(
                    transaction.Timestamp,
                    LineItemType.CancellationFee,
                    new CancellationFee(Seq1(GetReservationIndex(order, transaction.ResourceId!.Value, transaction.ReservationDate!.Value))),
                    null,
                    fee);
            return null;
        }

        static OwnerOrder? CreateOwnerOrder(Input.Order order) => order.Flags.HasFlag(OrderFlags.IsOwnerOrder) ? new OwnerOrder("(Ingen beskrivelse)") : null;

        Reservation CreateReservation(Input.Order order, Input.Reservation reservation) =>
            new(
                GetResourceId(reservation.ResourceId),
                GetReservationStatus(reservation),
                new Extent(reservation.Date, reservation.DurationInDays),
                reservation.Price is not null && !order.Flags.HasFlag(OrderFlags.IsOwnerOrder)
                    ? new Price(reservation.Price.Rent, reservation.Price.Cleaning, reservation.Price.Deposit)
                    : null,
                GetSentEmails(reservation.SentEmails),
                null);

        static ReservationStatus GetReservationStatus(Input.Reservation reservation) =>
            reservation.Status switch
            {
                Input.ReservationStatus.Reserved => ReservationStatus.Reserved,
                Input.ReservationStatus.Confirmed => ReservationStatus.Confirmed,
                Input.ReservationStatus.Cancelled => reservation.Order!.Transactions!.Any()
                    ? ReservationStatus.Cancelled
                    : ReservationStatus.Abandoned,
                Input.ReservationStatus.Settled => ReservationStatus.Settled,
                _ => throw new ArgumentException($"Invalid reservation status {reservation.Status}", nameof(reservation))
            };

        static ReservationEmails GetSentEmails(Input.ReservationEmails input)
        {
            var emails = ReservationEmails.None;
            if (input.HasFlag(Input.ReservationEmails.LockBoxCode))
                emails |= ReservationEmails.LockBoxCode;
            if (input.HasFlag(Input.ReservationEmails.NeedsSettlement))
                emails |= ReservationEmails.NeedsSettlement;
            return emails;
        }

        IEnumerable<OrderAudit> GetOrderAudits(Input.Order order, IEnumerable<Transaction> transactions)
        {
            if (order.Flags.HasFlag(OrderFlags.IsOwnerOrder))
                yield return new OrderAudit(order.CreatedTimestamp, order.UserId!.Value, OrderAuditType.PlaceOrder);
            foreach (var transaction in transactions)
                switch (transaction.Activity)
                {
                    case Activity.PlaceOrder:
                        yield return new(transaction.Timestamp, transaction.CreatedByUserId, OrderAuditType.PlaceOrder, transaction.TransactionId);
                        if (ordersConfirmedWhenPlaced.Contains(order.Id))
                            yield return new(transaction.Timestamp, transaction.CreatedByUserId, OrderAuditType.ConfirmOrder, transaction.TransactionId);
                        break;
                    case Activity.UpdateOrder:
                        yield return new(transaction.Timestamp, transaction.CreatedByUserId, OrderAuditType.CancelReservation, transaction.TransactionId);
                        break;
                    case Activity.SettleReservation:
                        yield return new(transaction.Timestamp, transaction.CreatedByUserId, OrderAuditType.SettleReservation, transaction.TransactionId);
                        break;
                    case Activity.PayIn:
                        // It's difficult to determine when an order was
                        // confirmed without reapplying the current logic to
                        // the historical data so instead some heuristics that
                        // works with the existing data is applied here.
                        if (ordersConfirmedWhenPlaced.Contains(order.Id))
                            continue;
                        // Special handling of order 214.
                        if (order.Id is 214 && transaction.Amounts[Application.Account.AccountsPayable] == Amount.Zero)
                            continue;
                        yield return new(transaction.Timestamp, transaction.CreatedByUserId, OrderAuditType.ConfirmOrder, transaction.TransactionId);
                        break;
                }

            if (order.Flags.HasFlag(OrderFlags.IsHistoryOrder))
            {
                var latestSettlement = transactions
                    .Where(transaction => transaction.Activity is Activity.SettleReservation or Activity.UpdateOrder)
                    .OrderByDescending(transaction => transaction.Timestamp)
                    .First();
                yield return new OrderAudit(latestSettlement.Timestamp, null, OrderAuditType.FinishOrder);
            }

            yield return new OrderAudit(timestamp, null, OrderAuditType.Import);
        }

        static ReservationIndex GetReservationIndex(Input.Order order, int resourceId, LocalDate reservationDate) =>
            order.Reservations!
                .Select((reservation, index) => (Reservation: reservation, Index: index))
                .Single(tuple => tuple.Reservation.ResourceId == resourceId && tuple.Reservation.Date == reservationDate)
                .Index;

        Reservation? TryGetReservation(int? resourceId, LocalDate? reservationDate, Order order) =>
            resourceId is not null && reservationDate is not null
                ? GetReservation(GetResourceId(resourceId.Value), reservationDate.Value, order)
                : null;

        static Reservation GetReservation(ResourceId resourceId, LocalDate reservationDate, Order order) =>
            order.Reservations.Single(reservation => reservation.ResourceId == resourceId && reservation.Extent.Date == reservationDate);

        Transaction? CreateTransaction(Input.Transaction transaction, Order order, Reservation? reservation, bool orderIsConfirmed)
        {
            // Place order.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.Rent && amount.Amount < 0) && transaction.Amounts!.Count is 4)
                return CreatePlaceOrderTransaction(transaction.Date, order, GetNextTransactionId(), Amount.Zero);
            // Cancel reservation.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.Rent && amount.Amount > 0) && transaction.Amounts!.Count is 4 or 5)
                return CreateCancelReservationTransaction(
                    transaction.Timestamp,
                    transaction.CreatedByUserId,
                    transaction.Date,
                    order,
                    Seq1(GetReservation(GetResourceId(transaction.ResourceId!.Value), transaction.ReservationDate!.Value, order) with
                    {
                        Status = orderIsConfirmed ? ReservationStatus.Confirmed : ReservationStatus.Reserved
                    }),
                    GetNextTransactionId(),
                    -TryGetAmount(transaction, Account.CancellationFees));
            // Pay in.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.Bank && amount.Amount > 0) && transaction.Amounts!.Count is 2 or 3)
                return CreatePayInTransaction(
                    new PayInCommand(transaction.Timestamp, transaction.CreatedByUserId, default, transaction.Date, GetAmount(transaction, Account.Bank)),
                    transaction.UserId!.Value,
                    GetNextTransactionId(),
                    -TryGetAmount(transaction, Account.Payments));
            // Settle reservation.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.Deposits && amount.Amount > 0) && transaction.Amounts!.Count is 2 or 3)
                return CreateSettleTransaction(transaction.Timestamp,
                    transaction.CreatedByUserId,
                    -TryGetAmount(transaction, Account.Damages),
                    transaction.Description is not null ? Some(transaction.Description) : None,
                    transaction.Date,
                    order,
                    reservation ?? throw CreateException(transaction),
                    GetNextTransactionId());
            // Pay out.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.Bank && amount.Amount < 0) && transaction.Amounts!.Count is 2)
                return CreatePayOutTransaction(
                    new PayOutCommand(transaction.Timestamp, transaction.CreatedByUserId, transaction.UserId!.Value, transaction.Date,
                        -GetAmount(transaction, Account.Bank)),
                    GetNextTransactionId(),
                    Amount.Zero);
            // Transfer to other orders.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.ToAccountsReceivable && amount.Amount < 0) && transaction.Amounts!.Count is 2)
                return null;
            // Transfer from other orders.
            if (transaction.Amounts!.Any(amount => amount.Account is Account.FromPayments && amount.Amount > 0) && transaction.Amounts!.Count is 2)
                return null;
            throw CreateException(transaction);

            TransactionId GetNextTransactionId()
            {
                var value = nextTransactionId;
                nextTransactionId += 1;
                return value;
            }

            static Amount GetAmount(Input.Transaction transaction, Account account) =>
                transaction.Amounts!.Single(amount => amount.Account == account).Amount;

            static Amount TryGetAmount(Input.Transaction transaction, Account account) =>
                transaction.Amounts!.SingleOrDefault(amount => amount.Account == account)?.Amount ?? Amount.Zero;

            static Exception CreateException(Input.Transaction transaction) =>
                new ImportException($"Transaction ID {transaction.Id} belonging to order {transaction.OrderId} cannot be processed.");
        }
    }

    async Task CreateNextTransactionId()
    {
        logger.LogInformation("Created {Count} transactions", nextTransactionId);

        var persistenceContext = persistenceContextFactory.Create(partitionKey)
            .CreateItem(nameof(Transaction), new NextId(nameof(Transaction), nextTransactionId.ToInt32()));
        await WritePersistenceContext(persistenceContext);
    }

    async Task EqualizeAccounts()
    {
        logger.LogInformation("Equalizing accounts");

        var persistenceContext = persistenceContextFactory.Create(partitionKey);
        var userIds = await persistenceContext.Untracked
            .ReadItems(persistenceContext.Query<Transaction>().Distinct().ProjectProperty(transaction => transaction.UserId))
            .MatchAsync(
                transactions => transactions,
                status => throw new ImportException($"Data import failed with status {status}."));
        //userIds = new[] { UserId.FromInt32(97) };
        foreach (var userId in userIds)
            await UpdateUserTransactions(persistenceContextFactory.Create(partitionKey), userId);

        logger.LogInformation("Equalized accounts for {Count} users", userIds.Count());

        static async Task UpdateUserTransactions(IPersistenceContext persistenceContext, UserId userId)
        {
            var contextWithTransactions = await persistenceContext
                .ReadItems(persistenceContext.Query<Transaction>().Where(transaction => transaction.UserId == userId))
                .MatchAsync(
                    context => context,
                    status => throw new ImportException($"Data import failed with status {status}."));
            var transactions = EqualizeAccountsReceivableAndAccountsPayable(contextWithTransactions.Items<Transaction>());
            await WritePersistenceContext(
                transactions.Aggregate(
                    contextWithTransactions,
                    (context, transaction) => context.UpdateItem(Transaction.GetId(transaction.TransactionId), transaction)));
        }

        static IEnumerable<Transaction> EqualizeAccountsReceivableAndAccountsPayable(IEnumerable<Transaction> transactions)
        {
            var ar = Amount.Zero;
            var ap = Amount.Zero;
            foreach (var transaction in transactions.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.TransactionId))
            {
                ar += transaction.Amounts[Application.Account.AccountsReceivable];
                ap += transaction.Amounts[Application.Account.AccountsPayable];
                if (ar != Amount.Zero && ap != Amount.Zero || ar < Amount.Zero || ap > Amount.Zero)
                {
                    if (ar > -ap)
                    {
                        // Move AP credit to AR debit.
                        var amount = ap;
                        yield return transaction with
                        {
                            Amounts = transaction.Amounts.Apply(
                                (Application.Account.AccountsReceivable, amount),
                                (Application.Account.AccountsPayable, -amount))
                        };
                        ar += amount;
                        ap += -amount;
                    }
                    else
                    {
                        // Move AR debit to AP credit.
                        var amount = ar;
                        yield return transaction with
                        {
                            Amounts = transaction.Amounts.Apply(
                                (Application.Account.AccountsReceivable, -amount),
                                (Application.Account.AccountsPayable, amount))
                        };
                        ar += -amount;
                        ap += amount;
                    }
                }
                else
                    yield return transaction;
            }
        }
    }

    async Task ImportPostingsV1(ReservationsContext dbContext)
    {
        logger.LogInformation("Importing postings V1");
        var persistenceContext = persistenceContextFactory.Create(partitionKey);
        var payInTransactions = (await persistenceContext.Untracked.ReadItems(
                    persistenceContext.Query<Transaction>().Where(transaction => transaction.Activity == Activity.PayIn))
                .MatchAsync(
                    transactions => transactions,
                    status => throw new ImportException($"Data import failed with status {status}.")))
            .GroupBy(transaction => (transaction.Date, transaction.UserId))
            .ToDictionary(grouping => grouping.Key, grouping => new Queue<Transaction>(grouping));

        // User where entire deposit was withheld on a single order.
        var specialUserId = UserId.FromInt32(116);
        var payOutTransactions = (await persistenceContext.Untracked.ReadItems(
                    persistenceContext.Query<Transaction>()
                        .Where(transaction =>
                            transaction.Activity == Activity.PayOut ||
                            transaction.Activity == Activity.SettleReservation && transaction.UserId == specialUserId))
                .MatchAsync(
                    transactions => transactions,
                    status => throw new ImportException($"Data import failed with status {status}.")))
            .GroupBy(transaction => (transaction.Date, transaction.UserId))
            .ToDictionary(grouping => grouping.Key, grouping => new Queue<Transaction>(grouping));

        var endDate = lastPostingsV1Month.PlusMonths(1);
        var postings = dbContext.Postings
            .Where(posting => posting.Date < endDate)
            .OrderBy(posting => posting.Date)
            .ThenBy(posting => posting.Id)
            .Select(CreatePosting);
        foreach (var posting in postings)
            await WritePersistenceContext(persistenceContextFactory.Create(partitionKey).CreateItem(posting.TransactionId.ToString(), posting));

        PostingV1 CreatePosting(Input.Posting posting) =>
            posting.Type switch
            {
                PostingType.PayIn => new(
                    GetPayInTransaction(posting).TransactionId,
                    posting.Date,
                    Activity.PayIn,
                    posting.UserId!.Value,
                    posting.OrderId!.Value,
                    toHashMap(GetAmounts(posting))),
                PostingType.PayOut => new(
                    GetPayOutTransaction(posting).TransactionId,
                    posting.Date,
                    Activity.PayOut,
                    posting.UserId!.Value,
                    null,
                    toHashMap(GetAmounts(posting))),
                _ => throw new ImportException("Invalid posting type {posting.Type}.")
            };

        Transaction GetPayInTransaction(Input.Posting posting)
        {
            var key = (posting.Date, posting.UserId!.Value);
            if (!payInTransactions.TryGetValue(key, out var transactions))
                throw new ImportException("No matching pay in transaction.");
            var transaction = transactions.Dequeue();
            if (transactions.Count is 0)
                payInTransactions.Remove(key);
            return transaction;
        }

        Transaction GetPayOutTransaction(Input.Posting posting)
        {
            var key = (posting.Date, posting.UserId!.Value);
            if (!payOutTransactions.TryGetValue(key, out var transactions))
                throw new ImportException("No matching pay out transaction.");
            var transaction = transactions.Dequeue();
            if (transactions.Count is 0)
                payOutTransactions.Remove(key);
            return transaction;
        }

        static IEnumerable<(PostingAccount, Amount)> GetAmounts(Input.Posting posting)
        {
            if (posting.Income is not 0)
                yield return (PostingAccount.Income, Amount.FromInt32(posting.Income));
            if (posting.Bank is not 0)
                yield return (PostingAccount.Bank, Amount.FromInt32(posting.Bank));
            if (posting.Deposits is not 0)
                yield return (PostingAccount.Deposits, Amount.FromInt32(posting.Deposits));
        }
    }

    async Task ScheduleCleaning(OrderingOptions options)
    {
        logger.LogInformation("Scheduling cleaning");
        var context = persistenceContextFactory.Create(partitionKey);
        var either = context.ReadItems(context.Query<Order>().Where(order => !order.Flags.HasFlag(OrderFlags.IsHistoryOrder)));
        var contextWithOrders = await either.MatchAsync(
            c => c,
            status => throw new ImportException($"Data import failed with status {status}."));
        await WritePersistenceContext(CleaningScheduleFunctions.ScheduleCleaning(options, contextWithOrders));
    }

    async Task UpdateUsers()
    {
        logger.LogInformation("Updating users");

        var userIds = await GetUserIds();
        var orders = await GetOrders();
        var currentOrders = orders
            .Where(order => order.IsUserOrder() && !order.IsHistoryOrder())
            .GroupBy(order => order.UserId)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(order => order.OrderId).ToSeq());
        var historyOrders = orders
            .Where(order => order.IsUserOrder() && order.IsHistoryOrder())
            .GroupBy(order => order.UserId)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(order => order.OrderId).ToSeq());
        var ownerOrders = orders
            .Where(order => order.IsOwnerOrder())
            .GroupBy(order => order.UserId)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList().AsEnumerable());

        var count = 0;
        foreach (var userId in userIds)
        {
            var currentOrdersForUser = GetCurrentOrdersForUser(userId);
            var historyOrdersForUser = GetHistoryOrdersForUser(userId);
            var transactions = await GetTransactionsForUser(userId);
            if (currentOrdersForUser.IsEmpty && historyOrdersForUser.IsEmpty && !transactions.Any())
                continue;
            await UpdateUser(userId, currentOrdersForUser, historyOrdersForUser, transactions);
            count += 1;
        }

        logger.LogInformation("Updated {Count} users", count);

        Task<IEnumerable<UserId>> GetUserIds()
        {
            var context = persistenceContextFactory.Create(partitionKey);
            var either = context.Untracked.ReadItems(context.Query<User>().ProjectProperty(user => user.UserId));
            return either.MatchAsync(
                ids => ids,
                status => throw new ImportException($"Data import failed with status {status}."));
        }

        Task<IEnumerable<Order>> GetOrders()
        {
            var context = persistenceContextFactory.Create(partitionKey);
            return context.Untracked.ReadItems(context.Query<Order>()).MatchAsync(
                o => o,
                status => throw new ImportException($"Data import failed with status {status}."));
        }

        Seq<OrderId> GetCurrentOrdersForUser(UserId userId) =>
            currentOrders.TryGetValue(userId, out var o) ? o : Empty;

        Seq<OrderId> GetHistoryOrdersForUser(UserId userId) =>
            historyOrders.TryGetValue(userId, out var o) ? o : Empty;

        Task<IEnumerable<Transaction>> GetTransactionsForUser(UserId userId) =>
            ReadTransactionsForUser(persistenceContextFactory.Create(partitionKey), userId).MatchAsync(
                transactions => transactions,
                status => throw new ImportException($"Data import failed with status {status}."));

        async Task UpdateUser(UserId userId, Seq<OrderId> currentOrdersForUser, Seq<OrderId> historyOrdersForUser, IEnumerable<Transaction> transactions)
        {
            var context = persistenceContextFactory.Create(partitionKey);
            var either = context.ReadItem<User>(User.GetId(userId));
            var contextWithUser = await either.MatchAsync(
                c => c,
                status => throw new ImportException($"Data import failed with status {status}."));
            var updatedUser = UpdateUserOrders(
                UpdateUserAudits(
                    transactions.Fold(contextWithUser.Item<User>(), UpdateUserBalance),
                    transactions,
                    ownerOrders),
                currentOrdersForUser,
                historyOrdersForUser);
            await WritePersistenceContext(contextWithUser.UpdateItem(User.GetId(updatedUser.UserId), updatedUser));
        }

        static User UpdateUserOrders(User user, Seq<OrderId> currentOrders, Seq<OrderId> historyOrders) =>
            user with
            {
                Orders = currentOrders,
                HistoryOrders = historyOrders
            };

        static User UpdateUserAudits(User user, IEnumerable<Transaction> transactions, Dictionary<UserId, IEnumerable<Order>> ownerOrders) =>
            user with { Audits = AddUserAudits(user, transactions, ownerOrders) };

        static Seq<UserAudit> AddUserAudits(User user, IEnumerable<Transaction> transactions, Dictionary<UserId, IEnumerable<Order>> ownerOrders) =>
            user.Audits
                .Concat(transactions.Select(GetUserAuditForTransaction).Where(audit => audit is not null)!)
                .Concat(GetOwnerOrderUserAudits(ownerOrders, user.UserId))
                .OrderBy(audit => audit.Timestamp).ToSeq();

        static UserAudit? GetUserAuditForTransaction(Transaction transaction) =>
            transaction.Activity switch
            {
                Activity.PlaceOrder =>
                    new UserAudit(transaction.Timestamp, transaction.UserId, UserAuditType.CreateOrder, transaction.OrderId, transaction.TransactionId),
                Activity.PayIn => new UserAudit(transaction.Timestamp, transaction.UserId, UserAuditType.PayIn, transaction.OrderId, transaction.TransactionId),
                Activity.PayOut => new UserAudit(transaction.Timestamp, transaction.UserId, UserAuditType.PayOut, transaction.OrderId,
                    transaction.TransactionId),
                _ => null
            };

        static IEnumerable<UserAudit> GetOwnerOrderUserAudits(Dictionary<UserId, IEnumerable<Order>> ownerOrders, UserId userId) =>
            ownerOrders.TryGetValue(userId, out var orders)
                ? orders.Select(order => new UserAudit(order.CreatedTimestamp, userId, UserAuditType.CreateOwnerOrder, order.OrderId))
                : Enumerable.Empty<UserAudit>();
    }

    static async Task WritePersistenceContext(IPersistenceContext persistenceContext)
    {
        var either = persistenceContext.Write();
        await either.MatchAsync(_ => { }, status => throw new ImportException($"Data import failed with status {status}."));
    }
}
