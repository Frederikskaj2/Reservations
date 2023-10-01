using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

record ReimbursementDescription(IncomeAccount AccountToDebit, string Description);
