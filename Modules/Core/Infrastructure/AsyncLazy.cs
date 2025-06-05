using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Core;

public sealed class AsyncLazy<T>
{
    readonly Lock gate = new();
    readonly Func<Task<T>> taskFactory;
    Lazy<Task<T>> instance;

    public AsyncLazy(Func<Task<T>> factory)
    {
        taskFactory = RetryOnFailure(factory);
        instance = new(taskFactory);
    }

    public bool IsValueCreated
    {
        get
        {
            lock (gate)
                return instance.IsValueCreated;
        }
    }

    public Task<T> Value
    {
        get
        {
            lock (gate)
                return instance.Value;
        }
    }

    Func<Task<T>> RetryOnFailure(Func<Task<T>> factory) =>
        async () =>
        {
            try
            {
                return await factory().ConfigureAwait(false);
            }
            catch
            {
                lock (gate)
                    instance = new(taskFactory);
                throw;
            }
        };

    public TaskAwaiter<T> GetAwaiter() =>
        Value.GetAwaiter();

    public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) =>
        Value.ConfigureAwait(continueOnCapturedContext);
}
