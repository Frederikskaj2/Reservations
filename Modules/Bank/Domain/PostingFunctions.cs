using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;
using System.Net;
using System.Threading;
using OrderId = Frederikskaj2.Reservations.Users.OrderId;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class PostingFunctions
{
    static readonly LocalDate lastPostingsV1Month = new(2022, 8, 1);

    public static EitherAsync<HttpStatusCode, Seq<Posting>> GetPostingsV1OrV2(IEntityReader reader, LocalDate month, CancellationToken cancellationToken) =>
        month <= lastPostingsV1Month
            ? GetPostingsV1(reader, month, cancellationToken)
            : GetPostingsV2(reader, month, cancellationToken);

    static EitherAsync<HttpStatusCode, Seq<Posting>> GetPostingsV1(IEntityReader reader, LocalDate month, CancellationToken cancellationToken) =>
        reader.Query(GetPostingsV1Query(month, month.PlusMonths(1)), cancellationToken);

    static IProjectedQuery<Posting> GetPostingsV1Query(LocalDate fromDate, LocalDate toDate) =>
        QueryFactory.Query<Posting>()
            .Where(posting => fromDate <= posting.Date && posting.Date < toDate)
            .OrderBy(posting => posting.Date)
            .OrderBy(posting => posting.TransactionId)
            .Project();

    static EitherAsync<HttpStatusCode, Seq<Posting>> GetPostingsV2(IEntityReader reader, LocalDate month, CancellationToken cancellationToken) =>
        from transactions in reader.Query(GetPostingsV2Query(month, month.PlusMonths(1)), cancellationToken)
        select CreatePostings(transactions);

    static IProjectedQuery<Transaction> GetPostingsV2Query(LocalDate fromDate, LocalDate toDate) =>
        QueryFactory.Query<Transaction>()
            .Where(transaction => fromDate <= transaction.Date && transaction.Date < toDate)
            .OrderBy(transaction => transaction.Date)
            .OrderBy(transaction => transaction.TransactionId)
            .Project();

    static Seq<Posting> CreatePostings(Seq<Transaction> transactions) =>
        transactions.Map(CreatePosting);

    static Posting CreatePosting(Transaction transaction) =>
        new(
            transaction.TransactionId,
            transaction.Date,
            transaction.Activity,
            transaction.ResidentId,
            GetOrderId(transaction.Description),
            SafeGetAmounts(transaction));

    static Option<OrderId> GetOrderId(Option<TransactionDescription> transactionDescription) =>
        transactionDescription.Case switch
        {
            TransactionDescription description => description.Match(
                placeOrder => Some(placeOrder.OrderId),
                cancellation => Some(cancellation.OrderId),
                settlement => Some(settlement.OrderId),
                reservationsUpdate => Some(reservationsUpdate.OrderId),
                _ => None,
                _ => None),
            _ => None,
        };

    static HashMap<PostingAccount, Amount> SafeGetAmounts(Transaction transaction) =>
        toHashMap(ValidateAmounts(transaction, GetAmounts(transaction)));

    static Seq<(PostingAccount Account, Amount Amount)> ValidateAmounts(Transaction transaction, Seq<(PostingAccount Account, Amount Amount)> amounts) =>
        !amounts.IsEmpty && amounts.Fold(Amount.Zero, (sum, amount) => sum + amount.Amount) == Amount.Zero
            ? amounts
            : throw new InternalValidationException($"Invalid {transaction}.");

    static Seq<(PostingAccount Account, Amount Amount)> GetAmounts(Transaction transaction) =>
        Seq(
                GetIncome(transaction),
                GetBank(transaction),
                GetAccountsReceivable(transaction),
                GetDeposits(transaction),
                GetAccountsPayable(transaction))
            .Somes();

    static Option<(PostingAccount Account, Amount Amount)> GetIncome(Transaction transaction) =>
        GetAccountAmount(
            PostingAccount.Income,
            transaction.Amounts[Account.Rent] +
            transaction.Amounts[Account.Cleaning] +
            transaction.Amounts[Account.CancellationFees] +
            transaction.Amounts[Account.Damages]);

    static Option<(PostingAccount Account, Amount Amount)> GetBank(Transaction transaction) =>
        GetAccountAmount(PostingAccount.Bank, transaction.Amounts[Account.Bank]);

    static Option<(PostingAccount Account, Amount Amount)> GetAccountsReceivable(Transaction transaction) =>
        GetAccountAmount(PostingAccount.AccountsReceivable, transaction.Amounts[Account.AccountsReceivable]);

    static Option<(PostingAccount Account, Amount Amount)> GetDeposits(Transaction transaction) =>
        GetAccountAmount(PostingAccount.Deposits, transaction.Amounts[Account.Deposits]);

    static Option<(PostingAccount Account, Amount Amount)> GetAccountsPayable(Transaction transaction) =>
        GetAccountAmount(PostingAccount.AccountsPayable, transaction.Amounts[Account.AccountsPayable]);

    static Option<(PostingAccount Account, Amount Amount)> GetAccountAmount(PostingAccount account, Amount amount) =>
        amount != Amount.Zero ? (account, amount) : None;
}
