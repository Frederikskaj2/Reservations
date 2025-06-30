using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class JobsExtensions
{
    public static async ValueTask RunConfirmOrders(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/confirm-orders/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask RunDeleteUsers(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/delete-users/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask RunFinishOwnerOrders(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/finish-owner-orders/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask RunSendLockBoxCodes(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/send-lock-box-codes/run");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask RunUpdateCleaningSchedule(this SessionFixture session)
    {
        var response = await session.AdministratorPost("jobs/update-cleaning-schedule/run");
        response.EnsureSuccessStatusCode();
    }
}
