using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record OrderConfirmedEmailModel(EmailAddress Email, string FullName, OrderId OrderId);
