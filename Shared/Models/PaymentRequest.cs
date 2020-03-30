using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Shared
{
    public class PaymentRequest
    {
        [Range(1, 100000, ErrorMessage = "Angiv et beløb større end nul.")]
        public int Amount { get; set; }
    }
}
