using OneOf;

namespace Frederikskaj2.Reservations.Orders;

[GenerateOneOf]
public partial class TransactionDescription : OneOfBase<PlaceOrder, Cancellation, Settlement, ReservationsUpdate, Reimbursement, Charge>;
