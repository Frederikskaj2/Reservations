using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Infrastructure;

class PasswordValidator : IPasswordValidator
{
    readonly PasswordPolicyOptions options;
    readonly IRemotePasswordChecker remotePasswordChecker;

    public PasswordValidator(IOptions<PasswordOptions> options, IRemotePasswordChecker remotePasswordChecker)
    {
        this.remotePasswordChecker = remotePasswordChecker;

        this.options = options.Value.Policy;
    }

    public async Task<PasswordValidationError> ValidateAsync(EmailAddress email, string password)
    {
        if (string.Equals(email.ToString(), password, StringComparison.OrdinalIgnoreCase))
            return PasswordValidationError.SameAsEmail;

        if (password.Length < options.MinimumLength)
            return PasswordValidationError.TooShort;

        var exposedCount = await remotePasswordChecker.GetPasswordExposedCount(password);
        if (exposedCount > options.MaximumExposedCount)
            return PasswordValidationError.Exposed;

        return PasswordValidationError.None;
    }
}
