using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Web;

public class ReimburseRequest
{
    public LocalDate Date { get; set; }
    public IncomeAccount AccountToDebit { get; set; }
    public string? Description { get; set; }
    public Amount Amount { get; set; }
}
