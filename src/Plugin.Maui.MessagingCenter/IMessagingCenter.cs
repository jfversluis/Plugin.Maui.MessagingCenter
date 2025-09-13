namespace Plugin.Maui.MessagingCenter;

/// <summary>
/// Provides a publish/subscribe messaging API for decoupled communication between components.
/// </summary>
/// <remarks>
/// <para><strong>Important Behavioral Difference from .NET MAUI MessagingCenter:</strong></para>
/// <para>This implementation prevents duplicate subscriptions to the same message type and message key by the same subscriber.
/// An <see cref="InvalidOperationException"/> will be thrown if you attempt to subscribe multiple times
/// to the same message type and key with the same subscriber object. However, different message keys are allowed.</para>
/// <para>For detailed behavioral differences, see the project documentation.</para>
/// </remarks>
public interface IMessagingCenter
{
    /// <summary>
    /// Sends a message identified by a string key along with an argument payload.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <typeparam name="TArgs">Type of the message argument.</typeparam>
    /// <param name="sender">The sender object publishing the message.</param>
    /// <param name="message">The message key.</param>
    /// <param name="args">The argument to send.</param>
    void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;

    /// <summary>
    /// Sends a message identified by a string key without any argument payload.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <param name="sender">The sender object publishing the message.</param>
    /// <param name="message">The message key.</param>
    void Send<TSender>(TSender sender, string message) where TSender : class;

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
    /// </exception>
    void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class;

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
    /// </exception>
    void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class;

    /// <summary>
    /// Unsubscribes from messages of a given key that include an argument payload.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <typeparam name="TArgs">Type of the message argument.</typeparam>
    /// <param name="subscriber">The subscribing object.</param>
    /// <param name="message">The message key to unsubscribe from.</param>
    void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class;

    /// <summary>
    /// Unsubscribes from messages of a given key with no argument.
    /// </summary>
    /// <typeparam name="TSender">Type of the sender.</typeparam>
    /// <param name="subscriber">The subscribing object.</param>
    /// <param name="message">The message key to unsubscribe from.</param>
    void Unsubscribe<TSender>(object subscriber, string message) where TSender : class;
}