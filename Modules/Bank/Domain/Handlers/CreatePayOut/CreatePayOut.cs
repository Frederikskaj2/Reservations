using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

static class CreatePayOut
{
    public static CreatePayOutOutput CreatePayOutCore(CreatePayOutInput input) =>
        new(
            new(
                input.PayOutId,
                input.Command.Timestamp,
                input.Command.ResidentId,
                input.Command.AccountNumber,
                input.Command.Amount,
                PayOutStatus.InProgress,
                None,
                Empty, new PayOutAudit(input.Command.Timestamp, input.Command.AdministratorId, PayOutAuditType.Create).Cons()),
            new(input.Command.ResidentId, input.PayOutId));
}
