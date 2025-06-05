using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Bank;

public record DeletePayOutCommand(PayOutId PayOutId, ETag? Etag);
