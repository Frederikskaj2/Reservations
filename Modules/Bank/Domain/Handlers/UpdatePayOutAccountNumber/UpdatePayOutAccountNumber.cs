using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;

namespace Frederikskaj2.Reservations.Bank;

static class UpdatePayOutAccountNumber
{
    public static Either<Failure<Unit>, UpdatePayOutAccountNumberOutput> UpdatePayOutAccountNumberCore(UpdatePayOutAccountNumberInput input) =>
        input.PayOut.Status is PayOutStatus.InProgress
            ? TryUpdatePayOut(input.Command, input.PayOut)
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Pay-out is not in progress (including delayed).");

    static UpdatePayOutAccountNumberOutput TryUpdatePayOut(UpdatePayOutAccountNumberCommand command, PayOut payOut) =>
        command.AccountNumber != payOut.AccountNumber
            ? new(UpdatePayOut(command, payOut), IsModified: true)
            : new UpdatePayOutAccountNumberOutput(payOut, IsModified: false);

    static PayOut UpdatePayOut(UpdatePayOutAccountNumberCommand command, PayOut payOut) =>
        payOut with
        {
            AccountNumber = command.AccountNumber,
            Audits = payOut.Audits.Add(CreateAudit(command)),
        };

    static PayOutAudit CreateAudit(UpdatePayOutAccountNumberCommand command) =>
        new(command.Timestamp, command.UserId, PayOutAuditType.UpdateAccountNumber);
}
