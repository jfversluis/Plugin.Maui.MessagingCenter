using System;
using System.Collections.Generic;

namespace Plugin.Maui.MessagingCenter
{
    /// <summary>
    /// Drop-in replacement for .NET MAUI MessagingCenter.
    /// </summary>
    public static class MessagingCenter
    {
        // Factory for consumers needing the IMessagingCenter interface
        public static IMessagingCenter Instance { get; } = new MessagingCenterAdapter();

        // Holds all subscriptions per message key
        private class Subscription
        {
            public WeakReference SubscriberRef;
            public Delegate Callback;  // Action<TSender, TArgs> or Action<TSender>
            public object SourceFilter;
        }

        private static readonly Dictionary<string, List<Subscription>> _subscriptions = new();

        private static string GetKey<TSender, TArgs>(string message) =>
            $"{message}|{typeof(TSender).FullName}|{typeof(TArgs).FullName}";
        private static string GetKey<TSender>(string message) =>
            $"{message}|{typeof(TSender).FullName}|";

        public static void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (callback is null) throw new ArgumentNullException(nameof(callback));

            // Instance method on the subscriber itself should not keep it alive
            if (callback.Target == subscriber)
                return;
            var key = GetKey<TSender, TArgs>(message);
            var sub = new Subscription { SubscriberRef = new WeakReference(subscriber), Callback = callback, SourceFilter = source };
            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(key, out var list))
                {
                    list = new List<Subscription>();
                    _subscriptions[key] = list;
                }
                list.Add(sub);
            }
        }

        public static void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (callback is null) throw new ArgumentNullException(nameof(callback));

            // Instance method on the subscriber itself should not keep it alive
            if (callback.Target == subscriber)
                return;

            var key = GetKey<TSender>(message);
            var sub = new Subscription { SubscriberRef = new WeakReference(subscriber), Callback = callback, SourceFilter = source };
            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(key, out var list))
                {
                    list = new List<Subscription>();
                    _subscriptions[key] = list;
                }
                list.Add(sub);
            }
        }

        public static void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            if (message is null) throw new ArgumentNullException(nameof(message));

            var key = GetKey<TSender, TArgs>(message);
            lock (_subscriptions)
            {
                if (_subscriptions.TryGetValue(key, out var list))
                    list.RemoveAll(s => s.SubscriberRef.Target == subscriber);
            }
        }

        public static void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
        {
            if (subscriber is null) throw new ArgumentNullException(nameof(subscriber));
            if (message is null) throw new ArgumentNullException(nameof(message));

            var key = GetKey<TSender>(message);
            lock (_subscriptions)
            {
                if (_subscriptions.TryGetValue(key, out var list))
                    list.RemoveAll(s => s.SubscriberRef.Target == subscriber);
            }
        }

        public static void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            if (message is null) throw new ArgumentNullException(nameof(message));

            var key = GetKey<TSender, TArgs>(message);
            List<Subscription> snapshot;
            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(key, out var list)) return;
                snapshot = new List<Subscription>(list);
            }

            foreach (var sub in snapshot)
            {
                // still alive?
                if (!(sub.SubscriberRef.Target is object tok)) continue;
                // still subscribed?
                lock (_subscriptions)
                {
                    if (!_subscriptions.TryGetValue(key, out var list) || !list.Contains(sub))
                        continue;
                }
                // source filter
                if (sub.SourceFilter is null || Equals(sub.SourceFilter, sender))
                {
                    ((Action<TSender, TArgs>)sub.Callback)(sender, args);
                }
            }
        }

        public static void Send<TSender>(TSender sender, string message) where TSender : class
        {
            if (sender is null) throw new ArgumentNullException(nameof(sender));
            if (message is null) throw new ArgumentNullException(nameof(message));

            var key = GetKey<TSender>(message);
            List<Subscription> snapshot;
            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(key, out var list)) return;
                snapshot = new List<Subscription>(list);
            }

            foreach (var sub in snapshot)
            {
                if (!(sub.SubscriberRef.Target is object tok)) continue;
                lock (_subscriptions)
                {
                    if (!_subscriptions.TryGetValue(key, out var list) || !list.Contains(sub))
                        continue;
                }
                if (sub.SourceFilter is null || Equals(sub.SourceFilter, sender))
                {
                    ((Action<TSender>)sub.Callback)(sender);
                }
            }
        }
    }
}