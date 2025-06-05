using Frederikskaj2.Reservations.Users;
using LanguageExt;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;

namespace Frederikskaj2.Reservations.Bank;

static class PostingsFactory
{
    public static Seq<PostingDto> CreatePostings(PostingsForMonth postingsForMonth) =>
        postingsForMonth.Postings.Map(posting => CreatePosting(posting, postingsForMonth.Users));

    static PostingDto CreatePosting(Posting posting, HashMap<UserId, UserExcerpt> users) =>
        new(
            posting.TransactionId,
            posting.Date,
            posting.Activity,
            posting.ResidentId,
            users[posting.ResidentId].FullName,
            FromUserId(posting.ResidentId),
            posting.OrderId.ToNullable(),
            posting.Amounts.ToSeq().Map(tuple => new AccountAmountDto(tuple.Key, tuple.Value)));
}
