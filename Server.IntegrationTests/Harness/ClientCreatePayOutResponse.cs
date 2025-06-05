using Frederikskaj2.Reservations.Bank;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

record ClientCreatePayOutResponse(PayOutDto PayOut, string? ETag) : CreatePayOutResponse(PayOut);
