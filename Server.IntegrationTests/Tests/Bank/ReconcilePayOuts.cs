using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to reconcile bank transactions with pay-outs
    So that a refund to a resident is posted when it appears in the bank
    """)]
public partial class ReconcilePayOuts
{
    [Scenario]
    public Task ReconcilePayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            GivenAPayOutIsCreated,
            GivenBankTransactionsAreImported,
            WhenTheTransactionIsReconciledWithThePayOut,
            ThenTheBankTransactionIsReconciled,
            ThenThePayOutIsDeleted,
            ThenThePayOutAppearsOnTheResidentsAccountStatementThatHasABalanceOf0,
            ThenTheReconciliationIsAudited);

    [Scenario]
    public Task ReconcileCreditTransaction() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            GivenBankTransactionsAreImported,
            WhenTheTransactionIsReconciledToTheResident,
            ThenTheBankTransactionIsReconciled,
            ThenThePayOutAppearsOnTheResidentsAccountStatementThatHasABalanceOf0,
            ThenTheReconciliationIsAudited);

    [Scenario]
    public Task ReconcileCreditTransactionWhenThereIsAMatchingPayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            GivenAPayOutIsCreated,
            GivenBankTransactionsAreImported,
            WhenTheTransactionIsReconciledToTheResident,
            ThenTheBankTransactionIsReconciled,
            ThenThePayOutIsDeleted,
            ThenThePayOutAppearsOnTheResidentsAccountStatementThatHasABalanceOf0,
            ThenTheReconciliationIsAudited);

    [Scenario]
    public Task ReconcileCreditTransactionWhenThereIsANonMatchingPayOut() =>
        Runner.RunScenarioAsync(
            GivenASettledOrder,
            GivenANonMatchingPayOutIsCreated,
            GivenBankTransactionsAreImported,
            WhenTheTransactionIsReconciledToTheResident,
            ThenTheBankTransactionIsReconciled,
            ThenThePayOutIsNotDeleted,
            ThenThePayOutAppearsOnTheResidentsAccountStatementThatHasABalanceOf0,
            ThenTheReconciliationIsAudited);
}
