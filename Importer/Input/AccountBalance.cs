using System.ComponentModel.DataAnnotations;

namespace Frederikskaj2.Reservations.Importer.Input;

public class AccountBalance
{
    public int UserId { get; set; }
    public virtual User? User { get; set; }
    public Account Account { get; set; }

    // Debit is positive.
    public int Amount { get; set; }

    public override string ToString() => Amount > 0 ? $"{Account}: {Amount} D" : $"{Account}: {-Amount} C";
}
