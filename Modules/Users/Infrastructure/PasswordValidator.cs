using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

class PasswordValidator(IOptions<PasswordOptions> options, RemotePasswordChecker remotePasswordChecker) : IPasswordValidator
{
    readonly PasswordPolicyOptions options = options.Value.Policy;

    public async Task<PasswordValidationError> Validate(EmailAddress email, string password)
    {
        if (string.Equals(email.ToString(), password, StringComparison.OrdinalIgnoreCase))
            return PasswordValidationError.SameAsEmail;

        if (password.Length < options.MinimumLength)
            return PasswordValidationError.TooShort;

        var exposedCount = await remotePasswordChecker.GetPasswordExposedCount(password);
        return exposedCount <= options.MaximumExposedCount ? PasswordValidationError.None : PasswordValidationError.Exposed;
    }
}
