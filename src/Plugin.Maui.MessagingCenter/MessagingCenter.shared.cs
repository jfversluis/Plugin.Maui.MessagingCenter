
using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;

namespace Plugin.Maui.MessagingCenter;

public class MessagingCenter : IMessagingCenter
{
    public static IMessagingCenter Instance { get; } = new MessagingCenter();

    public static void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
    {
        Instance.Send(sender, message, args);
    }

    void IMessagingCenter.Send<TSender, TArgs>(TSender sender, string message, TArgs args)
    {
        ArgumentNullException.ThrowIfNull(sender);
        WeakReferenceMessenger.Default.Send(new GenericMessage<TArgs>(message, args));
    }

    public static void Send<TSender>(TSender sender, string message) where TSender : class
    {
        Instance.Send(sender, message);
    }

    void IMessagingCenter.Send<TSender>(TSender sender, string message)
    {
        ArgumentNullException.ThrowIfNull(sender);
        WeakReferenceMessenger.Default.Send(new GenericMessage(message));
    }

    public static void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class
    {
        Instance.Subscribe(subscriber, message, callback, source);
    }

    void IMessagingCenter.Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(callback);

        WeakReferenceMessenger.Default.Register<GenericMessage<TArgs>>(subscriber, (r, m) =>
        {
            if (m.Message == message)
            {
                callback((TSender)r, m.Value);
            }
        });
    }

    public static void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class
    {
        Instance.Subscribe(subscriber, message, callback, source);
    }

    void IMessagingCenter.Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(callback);

        WeakReferenceMessenger.Default.Register<GenericMessage>(subscriber, (r, m) =>
        {
            if (m.Message == message)
            {
                callback((TSender)r);
            }
        });
    }

    public static void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
    {
        Instance.Unsubscribe<TSender, TArgs>(subscriber, message);
    }

    void IMessagingCenter.Unsubscribe<TSender, TArgs>(object subscriber, string message)
    {
        WeakReferenceMessenger.Default.Unregister<GenericMessage<TArgs>>(subscriber);
    }

    public static void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
    {
        Instance.Unsubscribe<TSender>(subscriber, message);
    }

    void IMessagingCenter.Unsubscribe<TSender>(object subscriber, string message)
    {
        WeakReferenceMessenger.Default.Unregister<GenericMessage>(subscriber);
    }
}
