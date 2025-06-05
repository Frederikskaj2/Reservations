using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

public record PayOutDetails(PayOut PayOut, ETag ETag, UserExcerpt User);
