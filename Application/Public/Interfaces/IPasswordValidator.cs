using Frederikskaj2.Reservations.Shared.Core;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application;

public interface IPasswordValidator
{
    Task<PasswordValidationError> ValidateAsync(EmailAddress email, string password);
}
