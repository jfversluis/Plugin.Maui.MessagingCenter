using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Concurrent;

namespace Plugin.Maui.MessagingCenter;

/// <summary>
/// Thread-safe hash set implementation
/// </summary>
internal class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>
{
    private readonly ConcurrentDictionary<T, byte> _dictionary = new();

    public void Add(T item) => _dictionary.TryAdd(item, 0);

    public bool Remove(T item) => _dictionary.TryRemove(item, out _);

    public bool Contains(T item) => _dictionary.ContainsKey(item);

    public int Count => _dictionary.Count;

    public void Dispose() => _dictionary.Clear();

    public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Static implementation of IMessagingCenter using WeakReferenceMessenger
/// </summary>
public static class MessagingCenter
{
    private static readonly IMessagingCenter _instance = new MessagingCenterImpl();

    /// <summary>
    /// Gets the default implementation of MessagingCenter
    /// </summary>
    public static IMessagingCenter Instance => _instance;

    /// <summary>
    /// Sends a message identified by a string key along with an argument payload.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <typeparam name="TArgs">Type of the message argument.</typeparam>
    /// <param name="sender">The sender object publishing the message.</param>
    /// <param name="message">The message key.</param>
    /// <param name="args">The argument to send.</param>
    public static void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
    {
        _instance.Send(sender, message, args);
    }

    /// <summary>
    /// Sends a message identified by a string key without any argument payload.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <param name="sender">The sender object publishing the message.</param>
    /// <param name="message">The message key.</param>
    public static void Send<TSender>(TSender sender, string message) where TSender : class
    {
        _instance.Send(sender, message);
    }

    /// <summary>
    /// Subscribes to receive messages identified by a string key.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender to listen for.</typeparam>
    /// <typeparam name="TArgs">Type of the message argument.</typeparam>
    /// <param name="subscriber">The subscribing object.</param>
    /// <param name="message">The message key to subscribe to.</param>
    /// <param name="callback">The callback to invoke when the message is received.</param>
    /// <param name="source">Optional sender filter; only messages from this sender will be received.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the same subscriber attempts to subscribe to the same message type and message key multiple times.
    /// Unlike the original .NET MAUI MessagingCenter, this implementation prevents duplicate subscriptions
    /// to avoid memory leaks and unexpected behavior. However, subscribing to different message keys is allowed.
    /// Unsubscribe first before subscribing again to the same message.
    /// </exception>
    /// <remarks>
    /// <para><strong>⚠️ Behavioral Difference from .NET MAUI MessagingCenter:</strong></para>
    /// <para>This implementation does not allow multiple subscriptions to the same message type and message key by the same subscriber.
    /// However, you can subscribe to multiple different message keys with the same message type.
    /// If you need multiple handlers for the same message, consider using different message names or consolidating logic into a single handler.</para>
    /// </remarks>
    public static void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class
    {
        _instance.Subscribe(subscriber, message, callback, source);
    }

    /// <summary>
    /// Subscribes to receive messages identified by a string key with no argument.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender to listen for.</typeparam>
    /// <param name="subscriber">The subscribing object.</param>
    /// <param name="message">The message key to subscribe to.</param>
    /// <param name="callback">The callback to invoke when the message is received.</param>
    /// <param name="source">Optional sender filter; only messages from this sender will be received.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the same subscriber attempts to subscribe to the same message type and message key multiple times.
    /// Unlike the original .NET MAUI MessagingCenter, this implementation prevents duplicate subscriptions
    /// to avoid memory leaks and unexpected behavior. However, subscribing to different message keys is allowed.
    /// Unsubscribe first before subscribing again to the same message.
    /// </exception>
    /// <remarks>
    /// <para><strong>Behavioral Difference from .NET MAUI MessagingCenter:</strong></para>
    /// <para>This implementation does not allow multiple subscriptions to the same message type and message key by the same subscriber.
    /// However, you can subscribe to multiple different message keys with the same message type.
    /// If you need multiple handlers for the same message, consider using different message names or consolidating logic into a single handler.</para>
    /// </remarks>
    public static void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class
    {
        _instance.Subscribe(subscriber, message, callback, source);
    }

    /// <summary>
    /// Unsubscribes from messages of a given key that include an argument payload.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <typeparam name="TArgs">Type of the message argument.</typeparam>
    /// <param name="subscriber">The subscribing object.</param>
    /// <param name="message">The message key to unsubscribe from.</param>
    public static void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
    {
        _instance.Unsubscribe<TSender, TArgs>(subscriber, message);
    }

    /// <summary>
    /// Unsubscribes from messages of a given key with no argument.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <param name="subscriber">The subscribing object.</param>
    /// <param name="message">The message key to unsubscribe from.</param>
    public static void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
    {
        _instance.Unsubscribe<TSender>(subscriber, message);
    }
}

/// <summary>
/// Internal implementation of IMessagingCenter using WeakReferenceMessenger.
/// This implementation prevents duplicate subscriptions to the same message type by the same subscriber,
/// which is a behavioral difference from the original .NET MAUI MessagingCenter.
/// </summary>
internal class MessagingCenterImpl : IMessagingCenter
{
    private readonly WeakReferenceMessenger _messenger = WeakReferenceMessenger.Default;
    private readonly ConcurrentDictionary<WeakReference, ConcurrentHashSet<string>> _unsubscribed = new();
    private readonly ConcurrentDictionary<object, object> _strongReferences = new();

    public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(message);

        var msg = new MessagingCenterMessage<TSender, TArgs>(sender, message, args);
        var token = CreateSubscriptionKey<TSender, TArgs>(message);
        _messenger.Send<MessagingCenterMessage<TSender, TArgs>, string>(msg, token);
    }

    public void Send<TSender>(TSender sender, string message) where TSender : class
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(message);

        var msg = new MessagingCenterMessage<TSender>(sender, message);
        var token = CreateSubscriptionKey<TSender>(message);
        _messenger.Send<MessagingCenterMessage<TSender>, string>(msg, token);
    }

    public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(callback);

        // Clear any previous unsubscribed status for this subscription
        var subscriptionKey = CreateSubscriptionKey<TSender, TArgs>(message);
        ClearUnsubscribedStatus(subscriber, subscriptionKey);

        // Check if the callback is a closure that captures the subscriber
        if (IsClosure(callback) && CapturesSubscriber(callback, subscriber))
        {
            // If the callback is a closure that captures the subscriber, 
            // we need to maintain a strong reference to the subscriber
            var strongRefKey = $"{subscriber.GetHashCode()}:{message}:{typeof(TSender).FullName}:{typeof(TArgs).FullName}";
            _strongReferences[strongRefKey] = subscriber;
        }

        // Use the subscription key as a token to make each message+subscriber combination unique
        _messenger.Register<MessagingCenterMessage<TSender, TArgs>, string>(subscriber, subscriptionKey, (recipient, msg) =>
        {
            if (msg.Message == message && (source == null || ReferenceEquals(msg.Sender, source)))
            {
                // Check if this subscriber has been explicitly unsubscribed from this message type
                if (!IsUnsubscribed(subscriber, subscriptionKey))
                {
                    callback(msg.Sender, msg.Args);
                }
            }
        });
    }

    public void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(callback);

        // Clear any previous unsubscribed status for this subscription
        var subscriptionKey = CreateSubscriptionKey<TSender>(message);
        ClearUnsubscribedStatus(subscriber, subscriptionKey);

        // Check if the callback is a closure that captures the subscriber
        if (IsClosure(callback) && CapturesSubscriber(callback, subscriber))
        {
            // If the callback is a closure that captures the subscriber, 
            // we need to maintain a strong reference to the subscriber
            var strongRefKey = $"{subscriber.GetHashCode()}:{message}:{typeof(TSender).FullName}:NoArgs";
            _strongReferences[strongRefKey] = subscriber;
        }

        // Use the subscription key as a token to make each message+subscriber combination unique
        _messenger.Register<MessagingCenterMessage<TSender>, string>(subscriber, subscriptionKey, (recipient, msg) =>
        {
            if (msg.Message == message && (source == null || ReferenceEquals(msg.Sender, source)))
            {
                // Check if this subscriber has been explicitly unsubscribed from this message type
                if (!IsUnsubscribed(subscriber, subscriptionKey))
                {
                    callback(msg.Sender);
                }
            }
        });
    }

    public void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(message);

        var subscriptionKey = CreateSubscriptionKey<TSender, TArgs>(message);
        MarkAsUnsubscribed(subscriber, subscriptionKey);

        // Remove strong reference if it exists
        var strongRefKey = $"{subscriber.GetHashCode()}:{message}:{typeof(TSender).FullName}:{typeof(TArgs).FullName}";
        _strongReferences.TryRemove(strongRefKey, out _);

        // Unregister from the messenger using the specific token for this message
        _messenger.Unregister<MessagingCenterMessage<TSender, TArgs>, string>(subscriber, subscriptionKey);
    }

    public void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(message);

        var subscriptionKey = CreateSubscriptionKey<TSender>(message);
        MarkAsUnsubscribed(subscriber, subscriptionKey);

        // Remove strong reference if it exists
        var strongRefKey = $"{subscriber.GetHashCode()}:{message}:{typeof(TSender).FullName}:NoArgs";
        _strongReferences.TryRemove(strongRefKey, out _);

        // Unregister from the messenger using the specific token for this message
        _messenger.Unregister<MessagingCenterMessage<TSender>, string>(subscriber, subscriptionKey);
    }

    private bool IsUnsubscribed(object subscriber, string subscriptionKey)
    {
        // Clean up dead references first
        CleanupDeadReferences();
        
        foreach (var kvp in _unsubscribed)
        {
            if (kvp.Key.Target == subscriber && kvp.Value.Contains(subscriptionKey))
            {
                return true;
            }
        }
        return false;
    }

    private void MarkAsUnsubscribed(object subscriber, string subscriptionKey)
    {
        // Clean up dead references first
        CleanupDeadReferences();
        
        // Find existing weak reference for this subscriber
        WeakReference existingRef = null;
        foreach (var kvp in _unsubscribed)
        {
            if (kvp.Key.Target == subscriber)
            {
                existingRef = kvp.Key;
                break;
            }
        }

        if (existingRef != null)
        {
            // Add to existing set
            _unsubscribed[existingRef].Add(subscriptionKey);
        }
        else
        {
            // Create new weak reference and set
            var weakRef = new WeakReference(subscriber);
            var newSet = new ConcurrentHashSet<string>();
            newSet.Add(subscriptionKey);
            _unsubscribed[weakRef] = newSet;
        }
    }

    private void ClearUnsubscribedStatus(object subscriber, string subscriptionKey)
    {
        // Clean up dead references first
        CleanupDeadReferences();
        
        // Find existing weak reference for this subscriber
        foreach (var kvp in _unsubscribed)
        {
            if (kvp.Key.Target == subscriber)
            {
                kvp.Value.Remove(subscriptionKey);
                
                // If the set is empty, remove the weak reference entirely
                if (kvp.Value.Count == 0)
                {
                    _unsubscribed.TryRemove(kvp.Key, out _);
                }
                break;
            }
        }
    }

    private void CleanupDeadReferences()
    {
        var deadRefs = new List<WeakReference>();
        foreach (var kvp in _unsubscribed)
        {
            if (!kvp.Key.IsAlive)
            {
                deadRefs.Add(kvp.Key);
            }
        }
        
        foreach (var deadRef in deadRefs)
        {
            _unsubscribed.TryRemove(deadRef, out _);
        }
    }

    private static string CreateSubscriptionKey<TSender, TArgs>(string message)
    {
        return $"{typeof(TSender).FullName}:{typeof(TArgs).FullName}:{message}";
    }

    private static string CreateSubscriptionKey<TSender>(string message)
    {
        return $"{typeof(TSender).FullName}:NoArgs:{message}";
    }

    private static bool IsClosure(Delegate callback)
    {
        // A closure is identified by the target being a compiler-generated class
        return callback.Target != null && callback.Target.GetType().Name.Contains("<>");
    }

    private static bool CapturesSubscriber(Delegate callback, object subscriber)
    {
        if (callback.Target == null) return false;

        // Check if any field in the closure object references the subscriber
        var closureType = callback.Target.GetType();
        var fields = closureType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            var value = field.GetValue(callback.Target);
            if (ReferenceEquals(value, subscriber))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Base interface for messaging center messages to enable type-safe message handling
/// </summary>
internal interface IMessagingCenterMessage
{
    string MessageKey { get; }
}

/// <summary>
/// Message wrapper for MessagingCenter with arguments
/// </summary>
internal class MessagingCenterMessage<TSender, TArgs> : IMessagingCenterMessage where TSender : class
{
    public MessagingCenterMessage(TSender sender, string message, TArgs args)
    {
        Sender = sender;
        Message = message;
        Args = args;
    }

    public TSender Sender { get; }
    public string Message { get; }
    public TArgs Args { get; }
    public string MessageKey => Message;
}

/// <summary>
/// Message wrapper for MessagingCenter without arguments
/// </summary>
internal class MessagingCenterMessage<TSender> : IMessagingCenterMessage where TSender : class
{
    public MessagingCenterMessage(TSender sender, string message)
    {
        Sender = sender;
        Message = message;
    }

    public TSender Sender { get; }
    public string Message { get; }
    public string MessageKey => Message;
}
