using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using Array = System.Array;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class SeedData
{
    public const string AdministratorEmail = "administrator@lokaler.frederikskaj2.dk";
    public const string AdministratorPassword = "gffk2";
    public static readonly UserId AdministratorUserId = UserId.FromInt32(1);
    public static readonly TestResource BanquetFacilities = new(ResourceId.FromInt32(1));
    public static readonly TestResource Frederik = new(ResourceId.FromInt32(2));
    public static readonly TestResource Kaj = new(ResourceId.FromInt32(3));

    public static Task CreateAdministrator(DatabaseInitializer databaseInitializer)
    {
        var normalizeEmail = EmailAddress.NormalizeEmail(AdministratorEmail);
        var either =

            from _1 in databaseInitializer.Create(
                "User",
                new
                {
                    Name = "User",
                    Id = 1,
                },
                "NextId").ToEitherAsync()

            from _2 in databaseInitializer.Create(
                "1",
                new
                {
                    UserId = 1,
                    Flags = 0,
                    Emails = new[]
                    {
                        new
                        {
                            Email = AdministratorEmail,
                            NormalizedEmail = normalizeEmail,
                            IsConfirmed = true,
                        },
                    },
                    FullName = "Admin Admin",
                    Phone = "98 76 54 32",
                    Created = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                    Security = new
                    {
                        HashedPassword = "AQAAAAEAAE4gAAAAEOkfIzv6fqI9jsW7n4LtVVlQQKk/LyguHifXnfXdPOSX1+2sFNl1R8RYcBKJiF3E0A==",
                        NextRefreshTokenId = 1,
                        RefreshTokens = Array.Empty<object>(),
                    },
                    EmailSubscriptions = 15,
                    Roles = 0x1FE,
                },
                "User").ToEitherAsync()

            from _3 in databaseInitializer.Create(
                normalizeEmail,
                new
                {
                    NormalizedEmail = normalizeEmail,
                    UserId = 1,
                },
                "UserEmail").ToEitherAsync()

            select unit;

        return either.MatchAsync(_ => { }, HandleErrorStatus);
    }

    static Task HandleErrorStatus(HttpStatusCode status)
    {
        switch (status)
        {
            case HttpStatusCode.Conflict:
                break;
            case < HttpStatusCode.Ambiguous:
                throw new SeedDataException($"Seed data creation failed with status {status}.");
        }
        return Task.CompletedTask;
    }
}
