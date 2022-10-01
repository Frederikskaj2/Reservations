using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

public class SeedData
{
    readonly DatabaseInitializer databaseInitializer;
    readonly ILogger logger;

    public SeedData(DatabaseInitializer databaseInitializer, ILogger<SeedData> logger)
    {
        this.databaseInitializer = databaseInitializer;
        this.logger = logger;
    }

    public async ValueTask InitializeAsync()
    {
        var now = DateTime.UtcNow.ToString("O");
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
                            Email = "martin@liversage.com",
                            NormalizedEmail = "MARTIN@LIVERSAGE.COM",
                            IsConfirmed = true
                        }
                    },
                    FullName = "Martin Liversage",
                    Phone = "31 18 97 93",
                    Created = now,
                    ApartmentId = 96,
                    Security = new
                    {
                        HashedPassword = "AQAAAAEAAE4gAAAAEOkfIzv6fqI9jsW7n4LtVVlQQKk/LyguHifXnfXdPOSX1+2sFNl1R8RYcBKJiF3E0A==",
                        NextRefreshTokenId = 1,
                        RefreshTokens = System.Array.Empty<object>()
                    },
                    AccountNumber = "9999-8888777766",
                    EmailSubscriptions = EmailSubscriptions.NewOrder | EmailSubscriptions.SettlementRequired | EmailSubscriptions.CleaningScheduleUpdated,
                    Roles = Roles.OrderHandling | Roles.Bookkeeping | Roles.UserAdministration | Roles.Cleaning | Roles.LockBoxCodes,
                    Audits = new[]
                    {
                        new {
                            Timestamp = now,
                            UserId = 1,
                            Type = UserAuditType.SignUp
                        }
                    }
                },
                "User").ToEitherAsync()

            from _3 in databaseInitializer.CreateAsync(
                "MARTIN@LIVERSAGE.COM",
                new
                {
                    NormalizedEmail = "MARTIN@LIVERSAGE.COM",
                    UserId = 1
                },
                "UserEmail").ToEitherAsync()

            select unit;

        await either.MatchAsync(
            _ => { },
            status => HandleErrorStatus(status, logger));

        static Task HandleErrorStatus(HttpStatusCode status, ILogger logger)
        {
            switch (status)
            {
                case HttpStatusCode.Conflict:
                    logger.LogDebug("Skipping seed data creation, data is already created");
                    break;
                case < (HttpStatusCode) 300:
                    throw new SeedDataException($"Seed data creation failed with status {status}.");
            }
            return Task.CompletedTask;
        }
    }
}
