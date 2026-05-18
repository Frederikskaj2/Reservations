using Bogus;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

static class Generate
{
    static readonly Faker faker = new("sv");
    static readonly Random random = new();

    static readonly Faker<PaymentInformation> paymentInformationFaker = new Faker<PaymentInformation>()
        .CustomInstantiator(_ => new(PaymentId(), Amount(), AccountNumber()));

    static readonly Faker<ReservationDescription> reservationDescriptionFaker = new Faker<ReservationDescription>()
        .CustomInstantiator(_ => new(ResourceId(), Date()));

    static string AccountNumber() => $"{faker.Finance.Account(4)}-{faker.Finance.Account()}";
    public static Amount Amount() => Users.Amount.FromInt32(50*faker.Random.Int(1, 100));
    public static Amount AmountBelow(Amount maximum) => Users.Amount.FromInt32(faker.Random.Int(1, (int) maximum.ToDecimal()));
    public static string DamagesDescription() => faker.Random.String2(10, 100);
    public static LocalDate Date() => LocalDate.FromDateOnly(faker.Date.FutureDateOnly());
    public static EmailAddress Email() => EmailAddress.FromString(faker.Internet.Email());
    public static string FullName() => faker.Name.FullName();

    public static EntryCode EntryCode()
    {
        while (true)
        {
            var entryCode = random.GetString("123456789", 6);
            if (Orders.EntryCode.IsValid(entryCode))
                return Orders.EntryCode.FromString(entryCode);
        }
    }

    public static LocalDate Month()
    {
        var date = LocalDate.FromDateOnly(faker.Date.PastDateOnly());
        return new(date.Year, date.Month, 1);
    }

    public static OrderId OrderId() => Users.OrderId.FromInt32(faker.Random.Int(100, 9999));
    public static PaymentId PaymentId() => PaymentIdEncoder.FromUserId(UserId.FromInt32(faker.Random.Int(1, 200)));
    public static PaymentInformation PaymentInformation() => paymentInformationFaker.Generate();
    public static ReservationDescription ReservationDescription() => reservationDescriptionFaker.Generate();
    public static ResourceId ResourceId() => Resources.All[faker.Random.Int(0, Resources.All.Count - 1)].ResourceId;
    public static Uri Url() => new(faker.Internet.Url());
}
