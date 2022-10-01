using Frederikskaj2.Reservations.Shared.Core;
using System;
using System.Net;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using Array = System.Array;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

record TestResource(ResourceId ResourceId);

static class TestData
{
    public const string AdministratorEmail = "administrator@lokaler.frederikskaj2.dk";
    public const string AdministratorPassword = "gffk2";
    public static readonly UserId AdministratorUserId = UserId.FromInt32(1);
    public static readonly TestResource BanquetFacilities = new(ResourceId.FromInt32(1));
    public static readonly TestResource Frederik = new(ResourceId.FromInt32(2));
    public static readonly TestResource Kaj = new(ResourceId.FromInt32(3));

    public static async Task CreateAdministratorAsync(DatabaseInitializer databaseInitializer)
    {
        var normalizeEmail = EmailAddress.NormalizeEmail(AdministratorEmail);
        var either =

            from _1 in databaseInitializer.CreateAsync(
                "User",
                new
                {
                    Name = "User",
                    Id = 1
                },
                "NextId").ToEitherAsync()

            from _2 in databaseInitializer.CreateAsync(
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
                            IsConfirmed = true
                        }
                    },
                    FullName = "Admin Admine",
                    Phone = "98 76 54 32",
                    Created = DateTime.UtcNow.ToString("O"),
                    Security = new
                    {
                        HashedPassword = "AQAAAAEAAE4gAAAAEOkfIzv6fqI9jsW7n4LtVVlQQKk/LyguHifXnfXdPOSX1+2sFNl1R8RYcBKJiF3E0A==",
                        NextRefreshTokenId = 1,
                        RefreshTokens = Array.Empty<object>()
                    },
                    EmailSubscriptions = 15,
                    Roles = 0xFE
                },
                "User").ToEitherAsync()

            from _3 in databaseInitializer.CreateAsync(
                normalizeEmail,
                new
                {
                    NormalizedEmail = normalizeEmail,
                    UserId = 1
                },
                "UserEmail").ToEitherAsync()

            select unit;

        await either.MatchAsync(_ => { }, HandleErrorStatus);

        static Task HandleErrorStatus(HttpStatusCode status)
        {
            switch (status)
            {
                case HttpStatusCode.Conflict:
                    break;
                case < (HttpStatusCode) 300:
                    throw new TestDataException($"Seed data creation failed with status {status}.");
            }
            return Task.CompletedTask;
        }
    }
}
