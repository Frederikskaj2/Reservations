namespace Frederikskaj2.Reservations.Users;

public record UserIdentityDto(UserId UserId, EmailAddress Email, string FullName, string Phone, ApartmentId? ApartmentId);
