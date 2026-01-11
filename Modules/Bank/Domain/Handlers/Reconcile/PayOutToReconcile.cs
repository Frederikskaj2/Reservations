using Frederikskaj2.Reservations.Persistence;

namespace Frederikskaj2.Reservations.Bank;

record PayOutToReconcile(ETaggedEntity<PayOut> OriginalPayOutEntity, ETaggedEntity<InProgressPayOut> InProgressPayOutEntity, PayOut UpdatedPayOut);
