using Frederikskaj2.Reservations.Orders;
using LanguageExt;

namespace Frederikskaj2.Reservations.RoomAccess;

record SendRoomEntryCodesOutput(Seq<Order> UpdatedOrders, Seq<RoomEntryCodeEmail> Emails);
