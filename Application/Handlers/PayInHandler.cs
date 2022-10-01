using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static Frederikskaj2.Reservations.Application.DebtorFactory;
using static Frederikskaj2.Reservations.Application.EmailFunctions;
using static Frederikskaj2.Reservations.Application.PayInFunctions;
using static Frederikskaj2.Reservations.Application.PaymentFunctions;
using static Frederikskaj2.Reservations.Application.TransactionFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class PayInHandler
{
    public static EitherAsync<Failure, Debtor> Handle(
        IPersistenceContextFactory contextFactory, IDateProvider dateProvider, IEmailService emailService, OrderingOptions options, PayInCommand command) =>
        from context1 in ReadUserAndAllOrdersContext(CreateContext(contextFactory), PaymentIdEncoder.ToUserId(command.PaymentId))
        let today = dateProvider.GetDate(command.Timestamp)
        from transactionId in CreateTransactionId(contextFactory)
        let context2 = PayIn(options, command, context1, transactionId)
        let user = context2.Item<User>()
        from _1 in DatabaseFunctions.WriteContext(context2)
        let payment = GetPaymentInformation(options, user)
        from _2 in SendPayInReceivedEmail(emailService, user, command.Amount, payment)
        from _3 in SendOrdersConfirmedEmail(emailService, context1, context2)
        select CreateDebtor(user);
}
