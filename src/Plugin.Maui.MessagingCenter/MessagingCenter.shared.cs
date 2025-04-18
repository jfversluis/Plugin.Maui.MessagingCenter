using System;
using System.Collections.Generic;

namespace Plugin.Maui.MessagingCenter
{
    /// <summary>
    /// Provides a static, decoupled publish-subscribe messaging service with weak-reference semantics,
    /// acting as a drop-in replacement for .NET MAUI MessagingCenter. Subscribers are held weakly and
    /// can be garbage-collected when no longer referenced.
    /// </summary>
    public static class MessagingCenter
    {
        /// <summary>
        /// Default <see cref="IMessagingCenter"/> adapter exposing the static pub/sub API.
        /// </summary>
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

        /// <summary>
        /// Subscribes to receive messages of a given key with an argument payload.
        /// </summary>
        /// <typeparam name="TSender">Type of the sender publishing the message.</typeparam>
        /// <typeparam name="TArgs">Type of the message argument.</typeparam>
        /// <param name="subscriber">Object subscribing to the message.</param>
        /// <param name="message">The message key to subscribe to.</param>
        /// <param name="callback">Action to invoke when the message is received.</param>
        /// <param name="source">Optional sender filter; only invoke if sender equals this value.</param>
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

        /// <summary>
        /// Subscribes to receive messages of a given key without arguments.
        /// </summary>
        /// <typeparam name="TSender">Type of the sender publishing the message.</typeparam>
        /// <param name="subscriber">Object subscribing to the message.</param>
        /// <param name="message">The message key to subscribe to.</param>
        /// <param name="callback">Action to invoke when the message is received.</param>
        /// <param name="source">Optional sender filter; only invoke if sender equals this value.</param>
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

        /// <summary>
        /// Unsubscribes from messages of a given key with an argument payload.
        /// </summary>
        /// <typeparam name="TSender">Type of the sender.</typeparam>
        /// <typeparam name="TArgs">Type of the message argument.</typeparam>
        /// <param name="subscriber">The subscriber to unregister.</param>
        /// <param name="message">The message key to unsubscribe from.</param>
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

        /// <summary>
        /// Unsubscribes from messages of a given key without arguments.
        /// </summary>
        /// <typeparam name="TSender">Type of the sender.</typeparam>
        /// <param name="subscriber">The subscriber to unregister.</param>
        /// <param name="message">The message key to unsubscribe from.</param>
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

        /// <summary>
        /// Sends a message with an argument payload to all active subscribers of the specified key.
        /// </summary>
        /// <typeparam name="TSender">Type of the sender.</typeparam>
        /// <typeparam name="TArgs">Type of the message argument.</typeparam>
        /// <param name="sender">The sender publishing the message.</param>
        /// <param name="message">The message key to send.</param>
        /// <param name="args">The argument payload.</param>
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

        /// <summary>
        /// Sends a message without arguments to all active subscribers of the specified key.
        /// </summary>
        /// <typeparam name="TSender">Type of the sender.</typeparam>
        /// <param name="sender">The sender publishing the message.</param>
        /// <param name="message">The message key to send.</param>
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