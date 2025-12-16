namespace Frederikskaj2.Reservations.Core;

public static class UrlPath
{
    public const string BankReconciliation = "bank";
    public const string Calendar = "kalender";
    public const string Checkout1 = "bestil/1";
    public const string Checkout2 = "bestil/2";
    public const string Checkout3 = "bestil/3";
    public const string CheckoutConflict = "bestil/overlap";
    public const string CleaningSchedule = "rengoeringsplan";
    public const string ConfirmEmail = "bekraeft-mail";
    public const string Consent = "samtykke";
    public const string InventoryBanquetFacilities = "inventarliste.pdf";
    public const string LockBoxCodes = "noeglebokskoder";
    public const string MyAccount = "bruger/konto";
    public const string MyOrders = "bruger/bestillinger";
    public const string MyTransactions = "bruger/kontoudtog";
    public const string NewPassword = "ny-adgangskode";
    public const string Orders = "bestillinger";
    public const string OwnerCalendar = "ejer/kalender";
    public const string OwnerCheckout1 = "ejer/bestil/1";
    public const string OwnerCheckout2 = "ejer/bestil/2";
    public const string OwnerOrders = "ejer/bestillinger";
    public const string PayOuts = "til-udbetaling";
    public const string Postings = "posteringer";
    public const string Reports = "rapporter";
    public const string RequestNewPassword = "anmod-om-ny-adgangskode";
    public const string RoomsBanquetFacilities = "vaerelser/festlokale";
    public const string RoomsBedrooms = "vaerelser/sovevaerelser";
    public const string RulesBanquetFacilities = "husorden/festlokale";
    public const string RulesBedrooms = "husorden/sovevaerelser";
    public const string SignIn = "log-ind";
    public const string SignUp = "opret-bruger";
    public const string Terms = "lejebetingelser";
    public const string Users = "brugere";
    public const string UsersTransactions = "brugere/{0}/kontoudtog";
    public const string UsersTransactionsSpecific = "brugere/{0}/kontoudtog#t-{1}";
}
