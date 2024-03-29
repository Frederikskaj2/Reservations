namespace Frederikskaj2.Reservations.Importer.Input;

public class TransactionAmount
{
    public int TransactionId { get; set; }
    public virtual Transaction? Transaction { get; set; }
    public Account Account { get; set; }

    // Debit is positive.
    public int Amount { get; set; }

    public override string ToString() => Amount > 0 ? $"{Account}: {Amount} D" : $"{Account}: {-Amount} C";
}