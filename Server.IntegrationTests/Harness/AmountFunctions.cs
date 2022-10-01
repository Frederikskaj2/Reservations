using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class AmountFunctions
{
    public static Amount Debit(Amount amount) => amount;

    public static Amount Credit(Amount amount) => -amount;
}
