using System;
using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;
using Order = Frederikskaj2.Reservations.Server.Data.Order;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;

namespace Frederikskaj2.Reservations.Tests
{
    internal sealed class OrderBuilder
    {
        private readonly Order order = new Order();

        private OrderBuilder()
        {
        }

        public static OrderBuilder CreaterOrder() => new OrderBuilder();

        public OrderBuilder WithUserId(int userId)
        {
            order.UserId = userId;
            return this;
        }

        public OrderBuilder WithFlags(OrderFlags flags)
        {
            order.Flags = flags;
            return this;
        }

        public OrderBuilder WithApartmentId(int apartmentId)
        {
            order.ApartmentId = apartmentId;
            return this;
        }

        public OrderBuilder WithReservation(Action<ReservationBuilder> buildAction)
        {
            var reservationBuilder = ReservationBuilder.CreateReservation();
            buildAction(reservationBuilder);
            var reservation = reservationBuilder.Build();
            (order.Reservations ??= new List<Reservation>()).Add(reservation);
            return this;
        }

        public Order Build() => order;
    }
}
