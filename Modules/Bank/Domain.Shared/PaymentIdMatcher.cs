using Frederikskaj2.Reservations.Users;
using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Bank;

public static partial class PaymentIdMatcher
{
    public static bool HasPaymentId(string value) => PaymentIdRegex.IsMatch(value);

    public static bool TryGetPaymentId(string value, out PaymentId paymentId)
    {
        var match = PaymentIdRegex.Match(value);
        if (match.Success)
        {
            paymentId = new(WhitespaceRegex.Replace(match.Value, "").ToUpperInvariant());
            return true;
        }
        paymentId = default;
        return false;
    }

    // Notice that not all letters or numbers are allowed in payment IDs, but this
    // matcher is trying to be lenient.
    [GeneratedRegex(@"\bB\s*-\s*[A-Z0-9]{4}\b", RegexOptions.IgnoreCase, 1000, "en-US")]
    static partial Regex PaymentIdRegex { get; }

    [GeneratedRegex(@"\s+", RegexOptions.None, 1000, "en-US")]
    static partial Regex WhitespaceRegex { get; }
}
