using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Equatable | Features.Comparable | Features.Formattable)]
public readonly partial struct Amount
{
    readonly int amount;

    public static Amount Negate(Amount amount) =>
        -amount.ToInt32();

    public static Amount Add(Amount amount1, Amount amount2) =>
        amount1.ToInt32() + amount2.ToInt32();

    public static Amount Subtract(Amount amount1, Amount amount2) =>
        amount1.ToInt32() - amount2.ToInt32();

    public static Amount Multiply(int factor, Amount amount) =>
        factor*amount.ToInt32();

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

    public static readonly Amount Zero = FromInt32(0);
}
