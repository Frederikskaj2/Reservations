using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record SendSettlementNeededRemindersOutput(Seq<Order> UpdatedOrders, Seq<ReservationWithOrder> ReservationsToSettle);
