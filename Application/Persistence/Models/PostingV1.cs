using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record PostingV1(
    TransactionId TransactionId,
    LocalDate Date,
    Activity Activity,
    UserId UserId,
    OrderId? OrderId,
    HashMap<PostingAccount, Amount> Amounts);
