using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record PlaceResidentOrderOutput(
    User Administrator,
    User User,
    Order Order,
    Transaction Transaction,
    Option<PaymentInformation> Payment,
    HashMap<Reservation, Seq<DatedLockBoxCode>> LockBoxCodesForOrder);
