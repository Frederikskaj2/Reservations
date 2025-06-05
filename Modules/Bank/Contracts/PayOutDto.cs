using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutDto(PayOutId PayOutId, Instant Timestamp, UserIdentityDto UserIdentity, PaymentId PaymentId, Amount Amount, ETag ETag);
