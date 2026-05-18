using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.RoomAccess;

record SendRoomEntryCodesInput(SendRoomEntryCodesCommand Command, Seq<Order> Orders, Seq<UserExcerpt> Users);
