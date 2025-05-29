# Behavior Differences from .NET MAUI MessagingCenter

This document outlines the key behavioral differences between this plugin's MessagingCenter implementation and the original .NET MAUI MessagingCenter.

## Multiple Subscriptions to Same Message

### .NET MAUI MessagingCenter Behavior
- **Allows multiple subscriptions**: The same subscriber can subscribe to the same message type multiple times
- **All callbacks invoked**: When a message is sent, all registered callbacks for that subscriber are invoked
- **Manual management required**: Developers must be careful to avoid unintended duplicate subscriptions

Example (would work in .NET MAUI MessagingCenter):
```csharp
var subscriber = new object();
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test", (sender, args) => Console.WriteLine("Handler 1"));
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test", (sender, args) => Console.WriteLine("Handler 2"));
MessagingCenter.Send(this, "test", "message"); // Both handlers would be called
```

### This Plugin's Behavior
- **Prevents duplicate subscriptions**: Throws `InvalidOperationException` when attempting to subscribe the same recipient to the same message type multiple times
- **Stricter subscription management**: Forces developers to unsubscribe before subscribing again
- **Based on WeakReferenceMessenger**: Inherits this behavior from the underlying `CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger`

Example (throws exception in this plugin):
```csharp
var subscriber = new object();
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test", (sender, args) => Console.WriteLine("Handler 1"));
// This will throw InvalidOperationException: "The target recipient has already subscribed to the target message."
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test", (sender, args) => Console.WriteLine("Handler 2"));
```

## Subscribe-Unsubscribe-Subscribe Pattern

### Both Implementations
Both implementations correctly support the subscribe-unsubscribe-subscribe pattern:

```csharp
var subscriber = new object();

// Subscribe
MessagingCenter.Subscribe<MyClass>(subscriber, "test", sender => Console.WriteLine("Received"));

// Send (message received)
MessagingCenter.Send(this, "test");

// Unsubscribe
MessagingCenter.Unsubscribe<MyClass>(subscriber, "test");

// Send (message not received)
MessagingCenter.Send(this, "test");

// Subscribe again
MessagingCenter.Subscribe<MyClass>(subscriber, "test", sender => Console.WriteLine("Received again"));

// Send (message received)
MessagingCenter.Send(this, "test");
```

## Recommendations

### If you need multiple handlers for the same message
Instead of multiple subscriptions, consider:

1. **Single subscription with multiple actions**:
```csharp
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test", (sender, args) => {
    HandleFirst(args);
    HandleSecond(args);
});
```

2. **Different message names**:
```csharp
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test1", FirstHandler);
MessagingCenter.Subscribe<MyClass, string>(subscriber, "test2", SecondHandler);
```

3. **Multiple subscribers**:
```csharp
MessagingCenter.Subscribe<MyClass, string>(subscriber1, "test", FirstHandler);
MessagingCenter.Subscribe<MyClass, string>(subscriber2, "test", SecondHandler);
```

### Benefits of stricter behavior
- **Prevents accidental duplicate subscriptions** that could cause unexpected behavior
- **Clearer subscription management** - you know exactly what's subscribed
- **Reduced memory usage** - no accidental handler accumulation
- **Earlier error detection** - duplicate subscription attempts are caught immediately

## Migration from .NET MAUI MessagingCenter

If you're migrating from .NET MAUI MessagingCenter and encounter `InvalidOperationException` for duplicate subscriptions:

1. **Review your subscription logic** to ensure you're not subscribing multiple times unintentionally
2. **Add proper unsubscribe calls** before re-subscribing
3. **Consider if multiple handlers are actually needed** or if the logic can be consolidated
