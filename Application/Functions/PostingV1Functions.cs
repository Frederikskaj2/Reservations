using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.PaymentIdEncoder;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class PostingV1Functions
{
    public static EitherAsync<Failure, IEnumerable<Posting>> GetPostingsForMonth(IPersistenceContext context, LocalDate month) =>
        from postings in ReadPostingsV1(context, month, month.PlusMonths(1))
        from userFullNames in ReadUserFullNames(context, toHashSet(postings.Map(posting => posting.UserId)))
        let userHashMap = toHashMap(userFullNames.Map(u => (u.UserId, u.FullName)))
        select postings.Map(posting => CreatePosting(userHashMap, posting));

    static Posting CreatePosting(HashMap<UserId, string> userNames, PostingV1 posting) =>
        new(
            posting.TransactionId,
            posting.Date,
            posting.Activity,
            posting.UserId,
            userNames[posting.UserId],
            FromUserId(posting.UserId).ToString(),
            posting.OrderId,
            posting.Amounts.Map(tuple1 => new AccountAmount(tuple1.Key, tuple1.Value)));
}
