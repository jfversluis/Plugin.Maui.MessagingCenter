namespace Plugin.Maui.MessagingCenter;

internal class GenericMessage(string message)
{
    public string Message { get; } = message;
}

internal class GenericMessage<T>(string message, T value)
{
    public string Message { get; } = message + "args";
    public T Value { get; } = value;
}