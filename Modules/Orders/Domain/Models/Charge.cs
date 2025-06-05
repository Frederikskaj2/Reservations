namespace Frederikskaj2.Reservations.Orders;

// "Opkrævning" - not used yet.
public record Charge(IncomeAccount AccountToCredit, string Description);
