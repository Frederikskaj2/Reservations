using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Posting
    {
        public LocalDate Date { get; set; }
        public int OrderId { get; set; }
        public IEnumerable<AccountAmount>? AccountAmounts { get; set; }
    }
}
