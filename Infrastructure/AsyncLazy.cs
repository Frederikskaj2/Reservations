using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Infrastructure;

public sealed class AsyncLazy<T>
{
    readonly object gate = new();
    readonly Func<Task<T>> taskFactory;
    Lazy<Task<T>> instance;

    public AsyncLazy(Func<Task<T>> factory)
    {
        taskFactory = RetryOnFailure(factory);
        instance = new Lazy<Task<T>>(taskFactory);
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

    public Task<T> Task
    {
        get
        {
            lock (gate)
                return instance.Value;
        }
    }

    Func<Task<T>> RetryOnFailure(Func<Task<T>> factory) => async () =>
    {
        try
        {
            return await factory().ConfigureAwait(false);
        }
        catch
        {
            lock (gate)
                instance = new Lazy<Task<T>>(taskFactory);
            throw;
        }
    };

    public TaskAwaiter<T> GetAwaiter() =>
        Task.GetAwaiter();

    public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) =>
        Task.ConfigureAwait(continueOnCapturedContext);
}