using Liversage.Primitives;
using System;
using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Users;

[Primitive]
public readonly partial struct AccountNumber
{
    readonly string value;

    public AccountNumber(string accountNumber)
    {
        if (!IsValid(accountNumber))
            throw new ArgumentException("Invalid account number.", nameof(accountNumber));
        value = accountNumber;
    }

    public static bool IsValid(string accountNumber) => AccountNumberRegex.IsMatch(accountNumber);

    [GeneratedRegex("^[0-9]{4}-[0-9]{2,10}$", RegexOptions.None, 1000)]
    static partial Regex AccountNumberRegex { get; }
}
