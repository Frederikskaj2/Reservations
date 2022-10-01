using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client;

public class EventAggregator
{
    readonly ILogger logger;
    readonly Dictionary<Type, HashSet<Subscription>> subscriptions = new();

    public EventAggregator(ILogger<EventAggregator> logger) => this.logger = logger;

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
        public abstract void Publish(object message);
    }

    class Subscription<T> : Subscription, IDisposable
    {
        readonly Action<T> action;
        readonly EventAggregator eventAggregator;

        public Subscription(EventAggregator eventAggregator, Action<T> action) =>
            (this.eventAggregator, this.action) = (eventAggregator, action);

        public void Dispose() => eventAggregator.Unsubscribe<T>(this);

        public override void Publish(object message) => action((T) message);
    }
}