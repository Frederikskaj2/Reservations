using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class AccountBalance
    {
        public int UserId { get; set; }
        public virtual User? User { get; set; }
        public Account Account { get; set; }

        // Debit is positive.
        public int Amount { get; set; }

        [Timestamp]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This property is only used by the framework.")]
        public byte[]? Timestamp { get; set; }
        
        public override string ToString() => Amount > 0 ? $"{Account}: {Amount} D" : $"{Account}: {-Amount} C";
    }
}
