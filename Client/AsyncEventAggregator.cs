using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class AsyncEventAggregator
{
    readonly Dictionary<Type, HashSet<Subscription>> subscriptions = new();

    public IDisposable Subscribe<T>(Func<T, ValueTask> action) where T : notnull
    {
        var subscription = new Subscription<T>(this, action);
        var subscriptionsForMessage = GetSubscriptions<T>();
        subscriptionsForMessage.Add(subscription);
        return subscription;
    }

    public async ValueTask PublishAsync<T>(T message) where T : notnull
    {
        var type = typeof(T);
        if (subscriptions.TryGetValue(type, out var subscriptionsForMessage))
            foreach (var subscriber in subscriptionsForMessage)
                await subscriber.PublishAsync(message);
    }

    HashSet<Subscription> GetSubscriptions<T>() where T : notnull
    {
        var type = typeof(T);
        if (subscriptions.TryGetValue(type, out var subscriptionsForMessage))
            return subscriptionsForMessage;
        subscriptionsForMessage = new HashSet<Subscription>();
        subscriptions.Add(type, subscriptionsForMessage);
        return subscriptionsForMessage;
    }

    void Unsubscribe<T>(Subscription subscription)
    {
        var type = typeof(T);
        var subscriptionsForMessage = subscriptions[type];
        subscriptionsForMessage.Remove(subscription);
    }

    abstract class Subscription
    {
        public abstract ValueTask PublishAsync(object message);
    }

    class Subscription<T> : Subscription, IDisposable
    {
        readonly Func<T, ValueTask> action;
        readonly AsyncEventAggregator eventAggregator;

        public Subscription(AsyncEventAggregator eventAggregator, Func<T, ValueTask> action) =>
            (this.eventAggregator, this.action) = (eventAggregator, action);

        public void Dispose() => eventAggregator.Unsubscribe<T>(this);

        public override ValueTask PublishAsync(object message) => action((T) message);
    }
}