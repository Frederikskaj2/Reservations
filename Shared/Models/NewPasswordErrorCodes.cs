using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared
{
    [SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "Using the name 'None' for the default value would be confusing.")]
    [Flags]
    public enum NewPasswordErrorCodes
    {
        GeneralError,
        PasswordRequiresDigit = 1,
        PasswordRequiresLower = 2,
        PasswordRequiresNonAlphanumeric = 4,
        PasswordRequiresUniqueChars = 8,
        PasswordRequiresUpper = 16,
        PasswordTooShort = 32,
        InvalidToken = 64,
    }
}