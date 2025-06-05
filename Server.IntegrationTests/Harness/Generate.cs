using Bogus;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public static class Generate
{
    static readonly Faker faker = new();

    static readonly Faker<SignUpRequest> signUpRequestFaker = new Faker<SignUpRequest>()
        .RuleFor(request => request.Email, Email)
        .RuleFor(request => request.FullName, FullName)
        .RuleFor(request => request.Phone, Phone)
        .RuleFor(request => request.ApartmentId, () => ApartmentId())
        .RuleFor(request => request.Password, Password);

    public static string Email() => faker.Internet.Email();
    public static string FullName() => faker.Name.FullName();
    public static string Phone() => faker.Phone.PhoneNumber("2#######");
    public static ApartmentId ApartmentId() => Users.ApartmentId.FromInt32(faker.Random.Int(1, 157));
    public static string Password() => faker.Internet.Password();
    public static string AccountNumber() => $"{faker.Finance.Account(4)}-{faker.Finance.Account()}";
    public static SignUpRequest SignUpRequest() => signUpRequestFaker.Generate();
}
