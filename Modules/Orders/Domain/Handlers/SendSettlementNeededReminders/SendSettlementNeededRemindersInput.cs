using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record SendSettlementNeededRemindersInput(SendSettlementNeededRemindersCommand Command, Seq<Order> Orders);
