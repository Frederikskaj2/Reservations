using System.Text.RegularExpressions;

namespace Frederikskaj2.Reservations.Shared.Web;

public static class ValidationRules
{
    public const int MinimumAmount = 1;
    public const int MaximumAmount = 100000;
    public const int MaximumAccountNumberLength = 50;
    public const int MaximumDamagesDescriptionLength = 100;
    public const int MaximumEmailLength = 100;
    public const int MaximumFullNameLength = 100;
    public const int MaximumOwnerOrderDescriptionLength = 200;
    public const int MaximumPasswordLength = 200;
    public const int MaximumPhoneLength = 50;
    public const int MaximumReservationsPerOrder = 10;
    public static readonly Regex AccountNumberRegex = new(@"^\s*[0-9]{4}-[0-9]{4,}\s*$");
    public static readonly Regex EmailRegex = new(@"^.+\@.+\..+$");
    public static readonly Regex FullNameRegex = new(@"^\s*(?<name>(?:\p{L}|\p{P})+(?:\s+(?:\p{L}|\p{P})+)+)\s*$");
    public static readonly Regex PhoneRegex = new(@"^\s*\+?[0-9](?:[- ]?[0-9]+)+\s*$");
}