using Frederikskaj2.Reservations.Bank;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to update a bank transaction
    So that I can change the status of the transaction
    """)]
public partial class UpdateBankTransactions
{
    const string csvContent =
        """
        "Bogført dato";"Rentedato";"Tekst";"Antal";"Beløb i DKK";"Bogført saldo i DKK";"Status";"Bankens arkivreference"
        "31.03.2024";"31.03.2024";"B-AAAA";"";"1300,00";"130993,59";"Udført";"9876      0000000000"
        "02.04.2024";"02.04.2024";"B-BBBB";"";"1100,00";"132093,59";"Udført";"8765      0000000000"
        "02.04.2024";"02.04.2024";"Ørsted Salg & Service A/";"";"-1339,49";"130754,10";"Udført";"1150      0000000000"
        """;

    [Scenario]
    public Task ChangeStatusToIgnored()
    {
        var transaction = new BankTransactionDto(1, new(2024, 3, 31), "B-AAAA", 1300M, 130993.59M, BankTransactionStatus.Ignored);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionIsUpdated(1, BankTransactionStatus.Ignored),
            _ => WhenBankTransactionsAreRetrieved(),
            _ => ThenUpdatedBankTransactionIs(transaction),
            _ => ThenRetrievedBankTransactionContains(transaction));
    }

    [Scenario]
    public Task ChangeStatusToUnknown()
    {
        var transaction = new BankTransactionDto(3, new(2024, 4, 2), "Ørsted Salg & Service A/", -1339.49M, 130754.10M, BankTransactionStatus.Unknown);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionIsUpdated(3, BankTransactionStatus.Unknown),
            _ => WhenBankTransactionsAreRetrieved(),
            _ => ThenUpdatedBankTransactionIs(transaction),
            _ => ThenRetrievedBankTransactionContains(transaction));
    }

    [Scenario]
    public Task DoNotChangeStatus()
    {
        var transaction = new BankTransactionDto(2, new(2024, 4, 2), "B-BBBB", 1100M, 132093.59M, BankTransactionStatus.Unknown);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionIsUpdated(2, BankTransactionStatus.Unknown),
            _ => WhenBankTransactionsAreRetrieved(),
            _ => ThenUpdatedBankTransactionIs(transaction),
            _ => ThenRetrievedBankTransactionContains(transaction));
    }
}
