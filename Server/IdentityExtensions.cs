using System;
using System.Linq;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Server
{
    internal static class IdentityExtensions
    {
        public static SignUpErrorCodes ToSignUpErrors(this IdentityResult result)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            return result.Errors.Aggregate(SignUpErrorCodes.GeneralError, (codes, error) => codes | GetErrorCode(error));

            static SignUpErrorCodes GetErrorCode(IdentityError error) => error.Code switch
            {
                nameof(IdentityErrorDescriber.DuplicateEmail) => SignUpErrorCodes.DuplicateEmail,
                nameof(IdentityErrorDescriber.PasswordRequiresDigit) => SignUpErrorCodes.PasswordRequiresDigit,
                nameof(IdentityErrorDescriber.PasswordRequiresLower) => SignUpErrorCodes.PasswordRequiresLower,
                nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => SignUpErrorCodes.PasswordRequiresNonAlphanumeric,
                nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => SignUpErrorCodes.PasswordRequiresUniqueChars,
                nameof(IdentityErrorDescriber.PasswordRequiresUpper) => SignUpErrorCodes.PasswordRequiresUpper,
                nameof(IdentityErrorDescriber.PasswordTooShort) => SignUpErrorCodes.PasswordTooShort,
                _ => SignUpErrorCodes.GeneralError
            };
        }

        public static UpdatePasswordErrorCodes ToUpdatePasswordErrors(this IdentityResult result)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            return result.Errors.Aggregate(UpdatePasswordErrorCodes.GeneralError, (codes, error) => codes | GetErrorCode(error));

            static UpdatePasswordErrorCodes GetErrorCode(IdentityError error) => error.Code switch
            {
                nameof(IdentityErrorDescriber.PasswordRequiresDigit) => UpdatePasswordErrorCodes.PasswordRequiresDigit,
                nameof(IdentityErrorDescriber.PasswordRequiresLower) => UpdatePasswordErrorCodes.PasswordRequiresLower,
                nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => UpdatePasswordErrorCodes.PasswordRequiresNonAlphanumeric,
                nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => UpdatePasswordErrorCodes.PasswordRequiresUniqueChars,
                nameof(IdentityErrorDescriber.PasswordRequiresUpper) => UpdatePasswordErrorCodes.PasswordRequiresUpper,
                nameof(IdentityErrorDescriber.PasswordTooShort) => UpdatePasswordErrorCodes.PasswordTooShort,
                _ => UpdatePasswordErrorCodes.GeneralError
            };
        }

        public static NewPasswordErrorCodes ToNewPasswordErrors(this IdentityResult result)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            return result.Errors.Aggregate(NewPasswordErrorCodes.GeneralError, (codes, error) => codes | GetErrorCode(error));

            static NewPasswordErrorCodes GetErrorCode(IdentityError error) => error.Code switch
            {
                nameof(IdentityErrorDescriber.InvalidToken) => NewPasswordErrorCodes.InvalidToken,
                nameof(IdentityErrorDescriber.PasswordRequiresDigit) => NewPasswordErrorCodes.PasswordRequiresDigit,
                nameof(IdentityErrorDescriber.PasswordRequiresLower) => NewPasswordErrorCodes.PasswordRequiresLower,
                nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => NewPasswordErrorCodes.PasswordRequiresNonAlphanumeric,
                nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => NewPasswordErrorCodes.PasswordRequiresUniqueChars,
                nameof(IdentityErrorDescriber.PasswordRequiresUpper) => NewPasswordErrorCodes.PasswordRequiresUpper,
                nameof(IdentityErrorDescriber.PasswordTooShort) => NewPasswordErrorCodes.PasswordTooShort,
                _ => NewPasswordErrorCodes.GeneralError
            };
        }
    }
}
