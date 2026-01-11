using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Core;

public static partial class ValidationRule
{
    public const int MaximumAccountNumberLength = 15;
    public const int MinimumAmount = 1;
    public const int MaximumAmount = 100000;
    public const int MaximumDamagesDescriptionLength = 100;
    public const int MaximumEmailLength = 100;
    public const int MaximumFullNameLength = 100;
    public const int MaximumOwnerOrderDescriptionLength = 200;
    public const int MaximumPasswordLength = 200;
    public const int MaximumPayOutNoteLength = 1000;
    public const int MaximumPhoneLength = 50;
    public const int MaximumReimburseDescriptionLength = 100;
    public const int MaximumReservationsPerOrder = 10;

    [GeneratedRegex(@"^\s*[0-9]{4}-[0-9]{2,10}\s*$", RegexOptions.None, 1000)]
    public static partial Regex AccountNumberRegex { get; }

    [GeneratedRegex(@"^.+\@.+\..+$", RegexOptions.None, 1000)]
    public static partial Regex EmailRegex { get; }

    [GeneratedRegex(@"^\s*(?<name>(?:\p{L}|\p{P})+(?:\s+(?:\p{L}|\p{P})+)+)\s*$", RegexOptions.None, 1000)]
    public static partial Regex FullNameRegex { get; }

    [GeneratedRegex(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$", RegexOptions.None, 1000)]
    public static partial Regex PhoneRegex { get; }
}
