using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class JobsExtensions
{
    public static async ValueTask ConfirmOrders(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/confirm-orders/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask DeleteUsers(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/delete-users/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask FinishOwnerOrders(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/finish-owner-orders/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask UpdateCleaningSchedule(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/update-cleaning-schedule/run");
        response.EnsureSuccessStatusCode();
    }
}
