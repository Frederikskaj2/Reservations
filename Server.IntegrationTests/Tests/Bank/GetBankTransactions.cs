using Frederikskaj2.Reservations.Bank;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using NodaTime;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Bank;

[FeatureDescription(
    """
    As an administrator
    I want to import bank transactions
    So that I can reconcile payments
    """)]
public partial class GetBankTransactions
{
    const string csvContent =
        """
        "Bogført dato";"Rentedato";"Tekst";"Antal";"Beløb i DKK";"Bogført saldo i DKK";"Status";"Bankens arkivreference"
        "31.03.2024";"31.03.2024";"B-AAAA";"";"1300";"130993,59";"Udført";"9876      0000000000"
        "02.04.2024";"02.04.2024";"B-BBBB";"";"1100";"132093,59";"Udført";"8765      0000000000"
        "02.04.2024";"02.04.2024";"Ørsted Salg & Service A/";"";"-1339,49";"130754,1";"Udført";"1150      0000000000"
        "02.04.2024";"02.04.2024";"Ørsted Salg & Service A/";"";"-1019,28";"129734,82";"Udført";"1150      0000000000"
        "02.04.2024";"02.04.2024";"Ørsted Salg & Service A/";"";"-3238,31";"126496,51";"Udført";"1150      0000000000"
        "02.04.2024";"02.04.2024";"B0910498400000";"";"132075,26";"258571,77";"Udført";"1150      0000000000"
        "02.04.2024";"02.04.2024";"DBT.1277 + ER&ER Rengøri 6695 1011792746";"";"-2750";"255821,77";"Udført";"9444      0000000000"
        "02.04.2024";"02.04.2024";"DBT.39561 + NYRUP INSTAL 86764431";"";"-10568,75";"245253,02";"Udført";"9444      0000000000"
        "03.04.2024";"03.04.2024";"Div.indb.kort 86120127 / 71";"";"23990";"269243,02";"Udført";"          FISAM.POST"
        "03.04.2024";"03.04.2024";"DBT.15727 + SAFETY GROUP 84974498";"";"-3743,75";"265499,27";"Udført";"9444      0000000000"
        "03.04.2024";"03.04.2024";"DBT.UD B-ZZZZ 3140 3143133672";"";"-1750";"263749,27";"Udført";"9444      0000000000"
        "03.04.2024";"03.04.2024";"DBT.UD B-YYYY 4426 4426376512";"";"-600";"263149,27";"Udført";"9444      0000000000"
        "03.04.2024";"03.04.2024";"DBT.UD B-XXXX 5501 6282387857";"";"-4500";"258649,27";"Udført";"9444      0000000000"
        "03.04.2024";"03.04.2024";"DBT.UD B-WWWW 6695 1007055761";"";"-800";"257849,27";"Udført";"9444      0000000000"
        "03.04.2024";"03.04.2024";"Fra Bea Tine Olsson";"";"900";"258749,27";"Udført";"3015      0000000000"
        "04.04.2024";"04.04.2024";"MSFT * E0400REEZW\ \MSFT AZURE\";"";"-131,28";"258617,99";"Udført";"1154      0000000000"
        "05.04.2024";"05.04.2024";"DBT.20602411409 + DEAS A 82568719";"";"-8973,43";"249644,56";"Udført";"9444      0000000000"
        "05.04.2024";"05.04.2024";"01877154   FAKTURA";"";"-232,88";"249411,68";"Udført";"1150      0000000000"
        "05.04.2024";"05.04.2024";"Sjeldani Boligadministra";"";"-14915,73";"234495,95";"Udført";"1150      0000000000"
        "05.04.2024";"05.04.2024";"Div.indb.kort 86120127 / 71";"";"11995";"246490,95";"Udført";"          FISAM.POST"
        "05.04.2024";"05.04.2024";"B-CCCC 666";"";"2000";"248490,95";"Udført";"6695      SC00000000"
        "05.04.2024";"05.04.2024";"DBT.Deas feb 82568719";"";"-8973,43";"239517,52";"Udført";"9444      0000000000"
        "08.04.2024";"08.04.2024";"Bauhaus Valby )))) 83324";"";"-2051,8";"237465,72";"Udført";"2325      0000000000"
        "08.04.2024";"08.04.2024";"B-EEEE";"";"1650";"239115,72";"Udført";"3015      0000000000"
        "08.04.2024";"08.04.2024";"B-FFFF";"";"1300";"240415,72";"Udført";"6695      0000000000"
        "09.04.2024";"09.04.2024";"DBT.20602412806 + DEAS A 82568719";"";"-11250";"229165,72";"Udført";"9444      0000000000"
        "11.04.2024";"11.04.2024";"B-GGGG";"";"1850";"231015,72";"Udført";"3015      0000000000"
        "12.04.2024";"12.04.2024";"DBT.udl.mlr.Sjel. 3124 9407391";"";"-5215";"225800,72";"Udført";"9444      0000000000"
        "13.04.2024";"13.04.2024";"B-HHHH";"";"1350";"227150,72";"Udført";"8117      0000000000"
        "15.04.2024";"15.04.2024";"DBT.1307 + ER&ER Rengøri 6695 1011792746";"";"-4750";"222400,72";"Udført";"9444      0000000000"
        "15.04.2024";"15.04.2024";"DBT.23001 + Team One Sec 4400 10504864";"";"-12282,5";"210118,22";"Udført";"9444      0000000000"
        "15.04.2024";"15.04.2024";"B-IIII";"";"2050";"212168,22";"Udført";"3015      0000000000"
        "19.04.2024";"19.04.2024";"B-JJJJ Ole 2k 7tv";"";"900";"213068,22";"Udført";"0890      0000000000"
        """;

    static readonly BankTransactionDto[] transactions =
    [
        new(1, new(2024, 3, 31), "B-AAAA", 1300, 130993.59M, BankTransactionStatus.Unknown),
        new(2, new(2024, 4, 2), "B-BBBB", 1100, 132093.59M, BankTransactionStatus.Unknown),
        new(3, new(2024, 4, 2), "Ørsted Salg & Service A/", -1339.49M, 130754.10M, BankTransactionStatus.Ignored),
        new(4, new(2024, 4, 2), "Ørsted Salg & Service A/", -1019.28M, 129734.82M, BankTransactionStatus.Ignored),
        new(5, new(2024, 4, 2), "Ørsted Salg & Service A/", -3238.31M, 126496.51M, BankTransactionStatus.Ignored),
        new(6, new(2024, 4, 2), "B0910498400000", 132075.26M, 258571.77M, BankTransactionStatus.Ignored),
        new(7, new(2024, 4, 2), "DBT.1277 + ER&ER Rengøri 6695 1011792746", -2750, 255821.77M, BankTransactionStatus.Ignored),
        new(8, new(2024, 4, 2), "DBT.39561 + NYRUP INSTAL 86764431", -10568.75M, 245253.02M, BankTransactionStatus.Ignored),
        new(9, new(2024, 4, 3), "Div.indb.kort 86120127 / 71", 23990, 269243.02M, BankTransactionStatus.Ignored),
        new(10, new(2024, 4, 3), "DBT.15727 + SAFETY GROUP 84974498", -3743.75M, 265499.27M, BankTransactionStatus.Ignored),
        new(11, new(2024, 4, 3), "DBT.UD B-ZZZZ 3140 3143133672", -1750, 263749.27M, BankTransactionStatus.Unknown),
        new(12, new(2024, 4, 3), "DBT.UD B-YYYY 4426 4426376512", -600, 263149.27M, BankTransactionStatus.Unknown),
        new(13, new(2024, 4, 3), "DBT.UD B-XXXX 5501 6282387857", -4500, 258649.27M, BankTransactionStatus.Unknown),
        new(14, new(2024, 4, 3), "DBT.UD B-WWWW 6695 1007055761", -800, 257849.27M, BankTransactionStatus.Unknown),
        new(15, new(2024, 4, 3), "Fra Bea Tine Olsson", 900, 258749.27M, BankTransactionStatus.Unknown),
        new(16, new(2024, 4, 4), @"MSFT * E0400REEZW\ \MSFT AZURE\", -131.28M, 258617.99M, BankTransactionStatus.Ignored),
        new(17, new(2024, 4, 5), "DBT.20602411409 + DEAS A 82568719", -8973.43M, 249644.56M, BankTransactionStatus.Ignored),
        new(18, new(2024, 4, 5), "01877154   FAKTURA", -232.88M, 249411.68M, BankTransactionStatus.Ignored),
        new(19, new(2024, 4, 5), "Sjeldani Boligadministra", -14915.73M, 234495.95M, BankTransactionStatus.Ignored),
        new(20, new(2024, 4, 5), "Div.indb.kort 86120127 / 71", 11995, 246490.95M, BankTransactionStatus.Ignored),
        new(21, new(2024, 4, 5), "B-CCCC 666", 2000, 248490.95M, BankTransactionStatus.Unknown),
        new(22, new(2024, 4, 5), "DBT.Deas feb 82568719", -8973.43M, 239517.52M, BankTransactionStatus.Ignored),
        new(23, new(2024, 4, 8), "Bauhaus Valby )))) 83324", -2051.80M, 237465.72M, BankTransactionStatus.Ignored),
        new(24, new(2024, 4, 8), "B-EEEE", 1650, 239115.72M, BankTransactionStatus.Unknown),
        new(25, new(2024, 4, 8), "B-FFFF", 1300, 240415.72M, BankTransactionStatus.Unknown),
        new(26, new(2024, 4, 9), "DBT.20602412806 + DEAS A 82568719", -11250, 229165.72M, BankTransactionStatus.Ignored),
        new(27, new(2024, 4, 11), "B-GGGG", 1850, 231015.72M, BankTransactionStatus.Unknown),
        new(28, new(2024, 4, 12), "DBT.udl.mlr.Sjel. 3124 9407391", -5215, 225800.72M, BankTransactionStatus.Ignored),
        new(29, new(2024, 4, 13), "B-HHHH", 1350, 227150.72M, BankTransactionStatus.Unknown),
        new(30, new(2024, 4, 15), "DBT.1307 + ER&ER Rengøri 6695 1011792746", -4750, 222400.72M, BankTransactionStatus.Ignored),
        new(31, new(2024, 4, 15), "DBT.23001 + Team One Sec 4400 10504864", -12282.50M, 210118.22M, BankTransactionStatus.Ignored),
        new(32, new(2024, 4, 15), "B-IIII", 2050, 212168.22M, BankTransactionStatus.Unknown),
        new(33, new(2024, 4, 19), "B-JJJJ Ole 2k 7tv", 900, 213068.22M, BankTransactionStatus.Unknown),
    ];

    [Scenario]
    public Task Import() =>
        Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(null, null, true, true, true),
            _ => ThenBankTransactionsAreRetrieved(transactions));

    [Scenario]
    public Task GetWithStartDate()
    {
        var startDate = new LocalDate(2024, 4, 14);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(startDate, null, true, true, true),
            _ => ThenBankTransactionsAreRetrieved(transactions.Where(transaction => transaction.Date >= startDate).ToArray()));
    }

    [Scenario]
    public Task GetWithEndDate()
    {
        var endDate = new LocalDate(2024, 4, 3);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(null, endDate, true, true, true),
            _ => ThenBankTransactionsAreRetrieved(transactions.Where(transaction => transaction.Date < endDate).ToArray()));
    }

    [Scenario]
    public Task GetWithStartAndEndDate()
    {
        var startDate = new LocalDate(2024, 4, 10);
        var endDate = new LocalDate(2024, 4, 13);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(startDate, endDate, true, true, true),
            _ => ThenBankTransactionsAreRetrieved(transactions.Where(transaction => startDate <= transaction.Date && transaction.Date < endDate).ToArray()));
    }

    [Scenario]
    public Task GetOnlyUnknown() =>
        Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(null, null, true, false, false),
            _ => ThenBankTransactionsAreRetrieved(transactions.Where(transaction => transaction.Status == BankTransactionStatus.Unknown).ToArray()));

    [Scenario]
    public Task GetOnlyIgnored() =>
        Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(null, null, false, true, false),
            _ => ThenBankTransactionsAreRetrieved(transactions.Where(transaction => transaction.Status == BankTransactionStatus.Ignored).ToArray()));

    [Scenario]
    public Task GetOnlyReconciled() =>
        Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(null, null, false, false, true),
            _ => ThenBankTransactionsAreRetrieved());

    [Scenario]
    public Task GetOnlyUnknownAndIgnored() =>
        Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(null, null, true, true, false),
            _ => ThenBankTransactionsAreRetrieved(transactions));

    [Scenario]
    public Task GetWithMultipleConditions()
    {
        var startDate = new LocalDate(2024, 4, 2);
        var endDate = new LocalDate(2024, 4, 5);
        return Runner.RunScenarioAsync(
            _ => WhenBankTransactionsAreRetrieved(startDate, endDate, false, true, false),
            _ => ThenBankTransactionsAreRetrieved(
                transactions
                    .Where(transaction => startDate <= transaction.Date && transaction.Date < endDate && transaction.Status == BankTransactionStatus.Ignored)
                    .ToArray()));
    }
}
