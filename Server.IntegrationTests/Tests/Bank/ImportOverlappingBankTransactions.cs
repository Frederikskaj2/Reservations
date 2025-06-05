using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to import the same bank transactions multiple times
    So that I don't have to worry about which transactions I imported last time
    """)]
public class ImportOverlappingBankTransactions(SessionFixture session) : BankTransactionsFixture(session)
{
    [Scenario]
    public Task ImportOverlapping() =>
        Runner.RunScenarioAsync(
            _ => WhenACsvFileFromBankIsImported(
                """
                "Bogført dato";"Rentedato";"Tekst";"Antal";"Beløb i DKK";"Bogført saldo i DKK";"Status";"Bankens arkivreference"
                "31.03.2024";"31.03.2024";"B-AAAA";"";"1200,00";"129693,59";"Udført";"9876      0000000000"
                "02.04.2024";"02.04.2024";"B-BBBB";"";"1400,00";"131093,59";"Udført";"8765      0000000000"
                """),
            _ => WhenACsvFileFromBankIsImported(
                """
                "Bogført dato";"Rentedato";"Tekst";"Antal";"Beløb i DKK";"Bogført saldo i DKK";"Status";"Bankens arkivreference"
                "02.04.2024";"02.04.2024";"B-BBBB";"";"1400,00";"131093,59";"Udført";"8765      0000000000"
                "02.04.2024";"02.04.2024";"Ørsted Salg & Service A/";"";"-1339,49";"129754,10";"Udf�rt";"1150      0000000000"
                """),
            _ => ThenANumberOfNewTransactionsAreCreated(1));
}