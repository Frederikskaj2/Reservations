using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Core;

public static partial class ValidationRule
{
    public const int MaximumAccountNumberLength = 50;
    public const int MinimumAmount = 1;
    public const int MaximumAmount = 100000;
    public const int MaximumDamagesDescriptionLength = 100;
    public const int MaximumEmailLength = 100;
    public const int MaximumFullNameLength = 100;
    public const int MaximumOwnerOrderDescriptionLength = 200;
    public const int MaximumPasswordLength = 200;
    public const int MaximumPhoneLength = 50;
    public const int MaximumReimburseDescriptionLength = 100;
    public const int MaximumReservationsPerOrder = 10;
    public static readonly Regex AccountNumberRegex = GenerateAccountNumberRegex();
    public static readonly Regex EmailRegex = GenerateEmailRegex();
    public static readonly Regex FullNameRegex = GenerateFullNameRegex();
    public static readonly Regex PhoneRegex = GeneratePhoneRegex();

    [GeneratedRegex(@"^\s*[0-9]{4}-[0-9]{4,}\s*$", RegexOptions.None, 1000)]
    private static partial Regex GenerateAccountNumberRegex();

    [GeneratedRegex(@"^.+\@.+\..+$", RegexOptions.None, 1000)]
    private static partial Regex GenerateEmailRegex();

    [GeneratedRegex(@"^\s*(?<name>(?:\p{L}|\p{P})+(?:\s+(?:\p{L}|\p{P})+)+)\s*$", RegexOptions.None, 1000)]
    private static partial Regex GenerateFullNameRegex();

    [GeneratedRegex(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$", RegexOptions.None, 1000)]
    private static partial Regex GeneratePhoneRegex();
}
