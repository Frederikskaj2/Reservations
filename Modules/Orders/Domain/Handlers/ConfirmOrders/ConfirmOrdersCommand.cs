using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record ConfirmOrdersCommand(Instant Timestamp);
