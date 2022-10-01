using Blazorise;
using Frederikskaj2.Reservations.Shared.Web;

namespace Frederikskaj2.Reservations.Client;

static class Validator
{
    public static void ValidateAccountNumber(ValidatorEventArgs e)
    {
        var value = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Oplys kontonummer hvortil depositum kan udbetales";
        }
        else if (IsAccountNumberTooLong(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Kontonummeret er for langt";
        }
        else if (!IsAccountNumberValid(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv et kontonummer på formen 1111-2222333344 - start med registreringsnummer";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    public static void ValidateAcceptTerms(ValidatorEventArgs e) =>
        e.Status = (bool) e.Value ? ValidationStatus.Success : ValidationStatus.Error;

    public static void ValidateEmail(ValidatorEventArgs e)
    {
        var value = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv din mail";
        }
        else if (IsEmailTooLong(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Teksten er for lang";
        }
        else if (!IsEmailValid(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv en korrekt mail";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    public static void ValidateFullName(ValidatorEventArgs e)
    {
        var value = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv dit fulde navn";
        }
        else if (IsFullNameTooLong(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Teksten er for lang";
        }
        else if (!IsFullNameValid(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv dit fulde navn";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    public static void ValidatePassword(ValidatorEventArgs e, string? missingMessage = null)
    {
        var value = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = missingMessage ?? "Angiv din adgangskode";
        }
        else if (IsPasswordTooLong(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Adgangskoden er for lang";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    public static void ValidatePhone(ValidatorEventArgs e)
    {
        var value = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv dit telefonnummer";
        }
        else if (IsPhoneTooLong(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Teksten er for lang";
        }
        else if (!IsPhoneValid(value))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv et korrekt telefonnummer";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    public static void ValidateAmount(ValidatorEventArgs e)
    {
        var value = (int) e.Value;
        if (value < ValidationRules.MinimumAmount)
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv et beløb større end nul.";
        }
        else if (value > ValidationRules.MaximumAmount)
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = $"Angiv et beløb som ikke overstiger {ValidationRules.MaximumAmount}.";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    public static void ValidateOwnerOrderDescription(ValidatorEventArgs e)
    {
        var description = (string) e.Value;
        if (string.IsNullOrWhiteSpace(description))
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Angiv formålet med din bestilling.";
        }
        else if (description.Length > ValidationRules.MaximumOwnerOrderDescriptionLength)
        {
            e.Status = ValidationStatus.Error;
            e.ErrorText = "Teksten er for lang.";
        }
        else
            e.Status = ValidationStatus.Success;
    }

    static bool IsAccountNumberTooLong(string? value) => value is not null && value.Length > ValidationRules.MaximumAccountNumberLength;
    static bool IsAccountNumberValid(string? value) => value is not null && ValidationRules.AccountNumberRegex.IsMatch(value);
    static bool IsEmailTooLong(string? value) => value is not null && value.Length > ValidationRules.MaximumEmailLength;
    static bool IsEmailValid(string? value) => value is not null && ValidationRules.EmailRegex.IsMatch(value);
    static bool IsFullNameTooLong(string? value) => value is not null && value.Length > ValidationRules.MaximumFullNameLength;
    static bool IsFullNameValid(string? value) => value is not null && ValidationRules.FullNameRegex.IsMatch(value);
    static bool IsPasswordTooLong(string? value) => value is not null && value.Length > ValidationRules.MaximumPasswordLength;
    static bool IsPhoneTooLong(string? value) => value is not null && value.Length > ValidationRules.MaximumPhoneLength;
    static bool IsPhoneValid(string? value) => value is not null && ValidationRules.PhoneRegex.IsMatch(value);
}