namespace Frederikskaj2.Reservations.Users;

public record SignUpRequest
{
    public SignUpRequest()
    {
    }

    public SignUpRequest(string? email, string? fullName, string? phone, ApartmentId? apartmentId, string? password)
    {
        Email = email;
        FullName = fullName;
        Phone = phone;
        ApartmentId = apartmentId;
        Password = password;
    }

    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public ApartmentId? ApartmentId { get; set; }
    public string? Password { get; set; }
}
