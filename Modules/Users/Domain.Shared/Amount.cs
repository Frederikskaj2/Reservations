using System;

namespace Frederikskaj2.Reservations.Users;

public readonly struct Amount : IEquatable<Amount>, IComparable<Amount>, IComparable, IFormattable
{
    readonly int cents;

    Amount(int cents) => this.cents = cents;

    public static Amount FromInt32(int value) => new(100*value);

    public static Amount FromDecimal(decimal value)
    {
        if (1000M*value%10M is not 0M)
            throw new ArgumentException("Amount has more than two fractional digits.", nameof(value));
        return new((int) (100M*value));
    }

    public decimal ToDecimal() => cents/100M;

    public static implicit operator Amount(int value) => FromInt32(value);

    public static implicit operator Amount(decimal value) => FromDecimal(value);

    public override string ToString() => $"{nameof(cents)}: {cents}";

    public string ToString(IFormatProvider formatProvider) =>
        cents%100 is 0 ? (cents/100).ToString("N0", formatProvider) : (cents/100M).ToString("N2", formatProvider);

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        (cents/100M).ToString(format, formatProvider);

    public bool Equals(Amount other) => cents == other.cents;

    public override bool Equals(object? obj) => obj is Amount value && Equals(value);

    public override int GetHashCode() => cents.GetHashCode();

    public static bool operator ==(Amount value1, Amount value2) => value1.Equals(value2);

    public static bool operator !=(Amount value1, Amount value2) => !(value1 == value2);

    public int CompareTo(Amount other) => cents.CompareTo(other.cents);

    int IComparable.CompareTo(object? obj)
    {
        if (obj is not Amount other)
            throw new ArgumentException("Object must be of type Amount.", nameof(obj));
        return cents.CompareTo(other.cents);
    }

    public static bool operator <(Amount value1, Amount value2) => value1.CompareTo(value2) < 0;

    public static bool operator <=(Amount value1, Amount value2) => value1.CompareTo(value2) <= 0;

    public static bool operator >(Amount value1, Amount value2) => !(value1 <= value2);

    public static bool operator >=(Amount value1, Amount value2) => !(value1 < value2);

    public static Amount Negate(Amount amount) =>
        new(-amount.cents);

    public static Amount Add(Amount amount1, Amount amount2) =>
        new(amount1.cents + amount2.cents);

    public static Amount Subtract(Amount amount1, Amount amount2) =>
        new(amount1.cents - amount2.cents);

    public static Amount Multiply(int factor, Amount amount) =>
        new(factor*amount.cents);

    public static Amount Remainder(Amount amount, int divisor) =>
        new(amount.cents%(100*divisor));

    public static Amount Min(Amount amount1, Amount amount2) =>
        amount1 < amount2 ? amount1 : amount2;

    public static Amount Max(Amount amount1, Amount amount2) =>
        amount1 > amount2 ? amount1 : amount2;

    public static Amount operator -(Amount amount) =>
        Negate(amount);

    public static Amount operator +(Amount amount1, Amount amount2) =>
        Add(amount1, amount2);

    public static Amount operator -(Amount amount1, Amount amount2) =>
        Subtract(amount1, amount2);

    public static Amount operator *(int factor, Amount amount) =>
        Multiply(factor, amount);

    public static Amount operator %(Amount amount, int divisor) =>
        Remainder(amount, divisor);

    public static Amount Abs(Amount amount) =>
        amount < Zero ? -amount : amount;

    public static readonly Amount Zero = FromInt32(0);
}
