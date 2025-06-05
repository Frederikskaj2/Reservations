using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

public interface IPasswordValidator
{
    Task<PasswordValidationError> Validate(EmailAddress email, string password);
}
