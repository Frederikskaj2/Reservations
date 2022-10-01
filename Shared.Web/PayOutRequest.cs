using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Web;

public class PayOutRequest
{
    public LocalDate Date { get; init; }
    public Amount Amount { get; init; }
}
