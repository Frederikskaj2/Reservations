using Frederikskaj2.Reservations.Persistence;

namespace Frederikskaj2.Reservations.Bank;

record PayOutPair(ETaggedEntity<PayOut> PayOutEntity, ETaggedEntity<InProgressPayOut> InProgressPayOutEntity);
