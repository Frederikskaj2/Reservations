﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using Mapster;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
using Price = Frederikskaj2.Reservations.Server.Data.Price;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;
using ReservedDay = Frederikskaj2.Reservations.Server.Data.ReservedDay;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    public class OrderService
    {
        private readonly IDataProvider dataProvider;
        private readonly DateTimeZone dateTimeZone;
        private readonly ReservationsContext db;
        private readonly IReservationPolicyProvider reservationPolicyProvider;
        private readonly ReservationsOptions reservationsOptions;

        public OrderService(
            IDataProvider dataProvider, DateTimeZone dateTimeZone, ReservationsContext db,
            IReservationPolicyProvider reservationPolicyProvider, ReservationsOptions reservationsOptions)
        {
            this.dataProvider = dataProvider;
            this.dateTimeZone = dateTimeZone;
            this.db = db;
            this.reservationPolicyProvider = reservationPolicyProvider;
            this.reservationsOptions = reservationsOptions;
        }

        public async Task<Order?> GetOrder(int orderId)
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .FirstOrDefaultAsync(order => order.Id == orderId);

        public async Task<IEnumerable<Order>> GetOrders()
            => await db.Orders
                .Include(order => order.User)
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<IEnumerable<Order>> GetOrders(int userId)
            => await db.Orders
                .Include(order => order.Reservations)
                .ThenInclude(reservation => reservation.Days)
                .Include(order => order.Transactions)
                .Where(order => order.UserId == userId)
                .OrderBy(order => order.CreatedTimestamp)
                .ToListAsync();

        public async Task<PlaceOrderResult> PlaceOrder(
            Instant now, int userId, int apartmentId, string accountNumber,
            IEnumerable<ReservationRequest> reservations)
        {
            var resources = await dataProvider.GetResources();
            var order = new Order
            {
                UserId = userId,
                ApartmentId = apartmentId,
                AccountNumber = accountNumber,
                CreatedTimestamp = now,
                Reservations = new List<Reservation>()
            };
            var totalPrice = new Price();
            foreach (var reservationRequest in reservations)
            {
                var reservation = await CreateReservation(reservationRequest);
                if (reservation == null)
                    return PlaceOrderResult.GeneralError;
                order.Reservations.Add(reservation);
                totalPrice.Accumulate(reservation.Price!);
            }

            order.Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Timestamp = now,
                    Type = TransactionType.Order,
                    CreatedByUserId = userId,
                    UserId = userId,
                    Order = order,
                    Amount = -(totalPrice.Rent + totalPrice.CleaningFee)
                },
                new Transaction
                {
                    Timestamp = now,
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
                    return PlaceOrderResult.ReservationConflict;
                return PlaceOrderResult.GeneralError;
            }

            return PlaceOrderResult.Success;

            async Task<Reservation?> CreateReservation(ReservationRequest reservation)
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
                    Status = ReservationStatus.Reserved,
                    UpdatedTimestamp = now,
                    Date = reservation.Date,
                    DurationInDays = reservation.DurationInDays,
                    Days = days,
                    Price = price
                };
            }

            static async Task<bool> IsReservationDurationValid(
                ReservationRequest reservation, IReservationPolicy policy)
            {
                var (minimumDays, maximumDays) =
                    await policy.GetReservationAllowedNumberOfDays(reservation.ResourceId, reservation.Date);
                return minimumDays <= reservation.DurationInDays && reservation.DurationInDays <= maximumDays;
            }
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

            // TODO: If all reservations are cancelled then convert order to history order.
            // TODO: Handle this conversion gracefully in the client.

            var totals = GetTotals(order);
            if (totals.GetBalance() >= 0)
            {
                var reservedReservations =
                    order.Reservations.Where(reservation => reservation.Status == ReservationStatus.Reserved);
                foreach (var reservation in reservedReservations)
                    reservation.Status = ReservationStatus.Confirmed;
            }

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
                // TODO: Include reservation status change in mail.
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

            // TODO: Send mail about the pay-in.

            return order;
        }

        public async Task<Order?> Settle(Instant timestamp, int orderId, int userId, int reservationId, int damages, string? description)
        {
            var order = await GetOrder(orderId);
            if (order == null)
                return null;

            var reservation = order.Reservations.FirstOrDefault(r => r.Id == reservationId);
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
                    UserId = order.UserId,
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

            // TODO: Send mail about the settlement.

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

            // TODO: Send mail about the payout.

            return order;
        }


        public async Task<IEnumerable<PayOut>> GetPayOuts()
        {
            var orders = await db.Orders
                .Include(order => order.User)
                .Include(order => order.Transactions)
                .ToListAsync();

            var payOuts = orders.Select(
                    order => new PayOut
                    {
                        OrderId = order.Id,
                        Mail = order.User!.Email,
                        FullName = order.User.FullName,
                        Phone = order.User.PhoneNumber,
                        ApartmentId = order.ApartmentId,
                        AccountNumber = order.AccountNumber!,
                        Amount = GetTotals(order).GetBalance()
                    })
                .Where(payOut => payOut.Amount > 0);
            return payOuts;
        }

        public OrderTotals GetTotals(Order order)
        {
            var totals = new OrderTotals();
            foreach (var transaction in order.Transactions!)
            {
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
            }
            return totals;
        }
    }
}