using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client;

public class EventAggregator(ILogger<EventAggregator> logger)
{
    readonly Dictionary<Type, HashSet<ISubscription>> subscriptions = new();

    public IDisposable Subscribe<T>(Action<T> action) where T : notnull
    {
        var subscription = new Subscription<T>(this, action);
        var subscriptionsForMessage = GetSubscriptions<T>();
        subscriptionsForMessage.Add(subscription);
        return subscription;
    }

    public void Publish<T>(T message) where T : notnull
    {
        logger.LogDebug("Publish {Message}", message);
        var type = typeof(T);
        if (subscriptions.TryGetValue(type, out var subscriptionsForMessage))
            foreach (var subscriber in subscriptionsForMessage)
                subscriber.Publish(message);
    }

    HashSet<ISubscription> GetSubscriptions<T>() where T : notnull
    {
        var type = typeof(T);
        if (subscriptions.TryGetValue(type, out var subscriptionsForMessage))
            return subscriptionsForMessage;
        subscriptionsForMessage = [];
        subscriptions.Add(type, subscriptionsForMessage);
        return subscriptionsForMessage;
    }

    void Unsubscribe<T>(ISubscription subscription)
    {
        var type = typeof(T);
        var subscriptionsForMessage = subscriptions[type];
        subscriptionsForMessage.Remove(subscription);
    }

    interface ISubscription
    {
        void Publish(object message);
    }

    sealed class Subscription<T>(EventAggregator eventAggregator, Action<T> action) : ISubscription, IDisposable
    {
        public void Dispose() => eventAggregator.Unsubscribe<T>(this);

        public void Publish(object message) => action((T) message);
    }
}
