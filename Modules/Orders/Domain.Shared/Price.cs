using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record Price(Amount Rent, Amount Cleaning, Amount Deposit)
{
    public Price() : this(Amount.Zero, Amount.Zero, Amount.Zero) { }

    public Amount Total() => Rent + Cleaning + Deposit;

    public static Price Add(Price price1, Price price2) =>
        new(price1.Rent + price2.Rent, price1.Cleaning + price2.Cleaning, price1.Deposit + price2.Deposit);

    public static Price operator +(Price price1, Price price2) =>
        Add(price1, price2);
}
