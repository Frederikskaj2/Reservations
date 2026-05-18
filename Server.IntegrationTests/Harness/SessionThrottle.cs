using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class SessionThrottle
{
    const int maximumConcurrency = 5;

    static readonly SemaphoreSlim semaphore = new(maximumConcurrency);

    public static Task StartSession() => semaphore.WaitAsync();

    public static void EndSession() => semaphore.Release();
}
