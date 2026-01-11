using Frederikskaj2.Reservations.Core;
using LanguageExt;
using System.Net;

namespace Frederikskaj2.Reservations.Bank;

static class CancelPayOut
{
    public static Either<Failure<Unit>, CancelPayOutOutput> CancelPayOutCore(CancelPayOutInput input) =>
        input.PayOut.Status is PayOutStatus.InProgress
            ? new CancelPayOutOutput(Cancel(input.PayOut, input.Command))
            : Failure.New(HttpStatusCode.UnprocessableEntity, "Pay-out is not in progress.");

    static PayOut Cancel(PayOut payOut, CancelPayOutCommand command) =>
        payOut with
        {
            Status = PayOutStatus.Cancelled,
            Resolution = (PayOutResolution) new Cancelled(command.Timestamp),
            Audits = payOut.Audits.Add(CreateAudit(command)),
        };

    static PayOutAudit CreateAudit(CancelPayOutCommand command) =>
        new(command.Timestamp, command.UserId, PayOutAuditType.Cancel);
}
