using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to reconcile bank transactions with pay-ins
    So that a payment from a resident is posted when it appears in the bank
    """)]
public partial class ReconcilePayIns
{
    [Scenario]
    public Task ReconcilePayIn() =>
        Runner.RunScenarioAsync(
            GivenAnUnpaidOrder,
            GivenOrderHasBeenPaidAndBankTransactionsImported,
            WhenThePayInIsReconciled,
            ThenTheBankTransactionIsReconciled,
            ThenTheResidentsBalanceIs0,
            ThenTheResidentHasNoOutstandingPayment);
}
