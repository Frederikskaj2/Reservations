using Frederikskaj2.Reservations.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public sealed record Import(Instant Timestamp, Seq<BankAccountImport> BankAccounts) : IHasId
{
    public const string SingletonId = "";

    string IHasId.GetId() => SingletonId;
}
