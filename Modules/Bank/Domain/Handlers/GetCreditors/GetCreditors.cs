using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

static class GetCreditors
{
    public static GetCreditorsOutput GetCreditorsCore(GetCreditorsInput input) =>
        new(RemoveCreditorsWithInProgressPayOuts(input.Creditors, Prelude.toHashSet(input.InProgressPayOuts.Map(payOut => payOut.ResidentId))));

    static Seq<User> RemoveCreditorsWithInProgressPayOuts(Seq<User> creditors, HashSet<UserId> usersWithInProgressPayOuts) =>
        creditors.Filter(user => !usersWithInProgressPayOuts.Contains(user.UserId));
}