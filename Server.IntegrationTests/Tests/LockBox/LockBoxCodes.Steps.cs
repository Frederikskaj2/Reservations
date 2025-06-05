using FluentAssertions;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.LockBox;

sealed partial class LockBoxCodes(SessionFixture session) : FeatureFixture, IScenarioSetUp, IClassFixture<SessionFixture>
{
    const int expectedLockBoxCodeCount = 10;

    State<GetLockBoxCodesResponse> getLockBoxCodesResponse1;
    State<GetLockBoxCodesResponse> getLockBoxCodesResponse2;

    GetLockBoxCodesResponse GetLockBoxCodesResponse1 => getLockBoxCodesResponse1.GetValue(nameof(GetLockBoxCodesResponse1));
    GetLockBoxCodesResponse GetLockBoxCodesResponse2 => getLockBoxCodesResponse2.GetValue(nameof(GetLockBoxCodesResponse2));

    async Task IScenarioSetUp.OnScenarioSetUp()
    {
        session.NowOffset = Period.Zero;
        await session.UpdateLockBoxCodes();
    }

    async Task WhenTheLockBoxCodesAreRetrieved() => getLockBoxCodesResponse1 = await session.GetLockBoxCodes();

    Task WhenAWeekPasses()
    {
        session.NowOffset = Period.FromWeeks(1);
        return Task.CompletedTask;
    }

    async Task WhenTheJobToUpdateLockBoxCodesExecutes() => await session.UpdateLockBoxCodes();

    async Task WhenTheLockBoxCodesAreRetrievedAgain() => getLockBoxCodesResponse2 = await session.GetLockBoxCodes();

    async Task WhenTheLockBoxCodesAreSent() => await session.SendLockBoxCodes();

    Task ThenTheOutdatedLockBoxCodesNoLongerExistAndNewLockBoxCodesAreGenerated()
    {
        var weeklyLockBoxCodes1 = GetLockBoxCodesResponse1.LockBoxCodes.ToArray();
        var weeklyLockBoxCodes2 = GetLockBoxCodesResponse2.LockBoxCodes.ToArray();

        // The first set of lock box codes always has empty values for the
        // Difference properties. To be able to compare the second set of lock box
        // codes from the first retrieval with the first set of lock box codes from
        // the second retrieval, the Difference property values have to be ignored.
        var week1LockBoxCodes1 = weeklyLockBoxCodes1[1];
        var week1LockBoxCodes2 = weeklyLockBoxCodes2[0];
        var week1LockBoxCodes1WithoutDifference = new
        {
            week1LockBoxCodes1.Date, week1LockBoxCodes1.WeekNumber, Codes = week1LockBoxCodes1.Codes.Select(code => new { code.ResourceId, code.Code }),
        };
        var week1LockBoxCodes2WithoutDifference = new
        {
            week1LockBoxCodes2.Date, week1LockBoxCodes2.WeekNumber, Codes = week1LockBoxCodes2.Codes.Select(code => new { code.ResourceId, code.Code }),
        };
        week1LockBoxCodes1WithoutDifference.Should().BeEquivalentTo(week1LockBoxCodes2WithoutDifference);

        // There's no problem comparing the remaining codes.
        weeklyLockBoxCodes1[2..].Should().BeEquivalentTo(weeklyLockBoxCodes2[1..^1]);
        return Task.CompletedTask;
    }

    Task ThenAListOfLockBoxCodesIsReturned()
    {
        var lockBoxCodes = GetLockBoxCodesResponse1.LockBoxCodes;
        ValidateLockBoxCodes(lockBoxCodes);
        return Task.CompletedTask;
    }

    async Task ThenAnEmailWithAListOfLockBoxCodesIsSent()
    {
        var emails = await session.DequeueEmails();
        var lockBoxCodesOverview = emails.LockBoxCodesOverview();
        lockBoxCodesOverview.Should().NotBeNull();
        ValidateLockBoxCodes(lockBoxCodesOverview.LockBoxCodesOverview!.Codes);
    }

    static void ValidateLockBoxCodes(IEnumerable<WeeklyLockBoxCodesDto> lockBoxCodes)
    {
        lockBoxCodes.Should().HaveCount(expectedLockBoxCodeCount);
        lockBoxCodes.Should().BeInAscendingOrder(codes => codes.Date);
        lockBoxCodes
            .SelectMany(codes => codes.Codes)
            .GroupBy(codes => codes.ResourceId, (_, values) => values.Select(code => code.Code).Distinct().Count())
            .Should().AllSatisfy(distinctCodes => distinctCodes.Should().Be(expectedLockBoxCodeCount));
    }
}
