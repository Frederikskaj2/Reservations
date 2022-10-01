namespace Frederikskaj2.Reservations.Shared.Core;

public record UserInformation(UserId UserId, EmailAddress Email, string FullName, string Phone, ApartmentId? ApartmentId);
