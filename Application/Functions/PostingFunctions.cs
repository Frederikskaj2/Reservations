using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NodaTime;
using System.Collections.Generic;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.PostingFactory;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

static class PostingFunctions
{
    static readonly LocalDate lastPostingsV1Month = new(2022, 8, 1);

    public static EitherAsync<Failure, PostingsRange> GetPostingsRangeOrThisMonth(IPersistenceContext context, IDateProvider dateProvider) =>
        from earliestTransaction in ReadEarliestTransaction(context)
        from postingsRange in GetPostingsRangeOrThisMonth(context, dateProvider, earliestTransaction)
        select postingsRange;

    static EitherAsync<Failure, PostingsRange> GetPostingsRangeOrThisMonth(
        IPersistenceContext context, IDateProvider dateProvider, Option<Transaction> earliestTransaction) =>
        earliestTransaction.Case switch
        {
            Transaction transaction => GetPostingsRange(context, GetMonthStart(transaction.Date)),
            _ => CreatePostingsRangeForThisMonth(dateProvider)
        };

    static EitherAsync<Failure, PostingsRange> GetPostingsRange(IPersistenceContext context, LocalDate earliestMonth) =>
        from latestTransaction in ReadLatestTransaction(context)
        select new PostingsRange(earliestMonth, GetMonthStart(latestTransaction.ValueUnsafe().Date));

    static PostingsRange CreatePostingsRangeForThisMonth(IDateProvider dateProvider) =>
        CreatePostingsRangeForMonth(GetMonthStart(dateProvider.Today));

    static PostingsRange CreatePostingsRangeForMonth(LocalDate monthStart) =>
        new(monthStart, monthStart);

    static LocalDate GetMonthStart(LocalDate date) =>
        date.PlusDays(-(date.Day - 1));

    public static EitherAsync<Failure, IEnumerable<Posting>> GetPostingsForMonth(IPersistenceContext context, LocalDate month) =>
        month <= lastPostingsV1Month ? PostingV1Functions.GetPostingsForMonth(context, month) : GetPostingsV2ForMonth(context, month);

    static EitherAsync<Failure, IEnumerable<Posting>> GetPostingsV2ForMonth(IPersistenceContext context, LocalDate month) =>
        from transactions in ReadTransactions(context, month, month.PlusMonths(1))
        let userIds = toHashSet(transactions.Map(transaction => transaction.UserId))
        from userFullNames in ReadUserFullNames(context, userIds)
        let userNames = toHashMap(userFullNames.Map(userFullName => (userFullName.UserId, userFullName.FullName)))
        select GetPostings(userNames, transactions);

    static IEnumerable<Posting> GetPostings(HashMap<UserId, string> userNames, IEnumerable<Transaction> transactions) =>
        transactions.Map(transaction => CreatePosting(userNames, transaction));

    public static EitherAsync<Failure, EmailAddress> SendPostingsForMonth(
        IPersistenceContext context, IEmailService emailService, SendPostingsCommand command) =>
        from user in ReadUser(context, command.UserId)
        from postings in GetPostingsForMonth(context, command.Month)
        from _ in SendPostingsEmail(emailService, command.Month, user, postings)
        select user.Email();

    static EitherAsync<Failure, Unit> SendPostingsEmail(
        IEmailService emailService, LocalDate month, User user, IEnumerable<Posting> postings) =>
        emailService.Send(new PostingsForMonthEmailModel(user.Email(), user.FullName, month, postings)).ToRightAsync<Failure, Unit>();
}
