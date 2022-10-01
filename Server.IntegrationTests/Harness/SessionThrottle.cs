using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class SessionThrottle
{
    const int maximumConcurrency = 6;
    
    static readonly SemaphoreSlim semaphore = new(maximumConcurrency);

    public static Task StartSessionAsync() => semaphore.WaitAsync();

    public static void EndSession() => semaphore.Release();
}
