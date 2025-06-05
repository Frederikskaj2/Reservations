using Frederikskaj2.Reservations.Core;
using Microsoft.AspNetCore.Authorization;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

[Authorize(Policy = Policy.ViewOrders)]
partial class BankReconciliationPage;
