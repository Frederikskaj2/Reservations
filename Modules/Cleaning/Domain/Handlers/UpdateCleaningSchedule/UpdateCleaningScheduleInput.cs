using Frederikskaj2.Reservations.Orders;
using LanguageExt;

namespace Frederikskaj2.Reservations.Cleaning;

record UpdateCleaningScheduleInput(UpdateCleaningScheduleCommand Command, Seq<Order> Orders);
