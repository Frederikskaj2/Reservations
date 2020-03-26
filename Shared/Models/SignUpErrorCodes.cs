using System;

namespace Frederikskaj2.Reservations.Shared
{
    [Flags]
    public enum SignUpErrorCodes
    {
        GeneralError,
        PasswordRequiresDigit = 1,
        PasswordRequiresLower = 2,
        PasswordRequiresNonAlphanumeric = 4,
        PasswordRequiresUniqueChars = 8,
        PasswordRequiresUpper = 16,
        PasswordTooShort = 32,
        DuplicateEmail = 64
    }
}