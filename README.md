![](nuget.png)
# Plugin.Maui.MessagingCenter

`Plugin.Maui.MessagingCenter` provides a drop-in compatible replacement for the .NET MAUI MessagingCenter which has been deprecated and will be removed in the near future. This is a wrapper library that uses the method signatures of the .NET MAUI MessagingCenter but under the hood uses the MVVM Toolkit WeakReferenceMessenger which is known for its superb performance!

Please note that you probably want to adopt the [MVVM Toolkit Messenger](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/messenger) APIs as they provide more functionality.

> [!IMPORTANT]  
> This library is meant to make the migration from Xamarin.Forms to .NET MAUI easier, not to provide a long-term solution.

## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.MessagingCenter.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.MessagingCenter/)

Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.MessagingCenter).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.MessagingCenter`, or through the NuGet Package Manager in Visual Studio.

## Getting Started

### 1. Add the Using Statement

After installation, add the using statement to your files:

```csharp
using Plugin.Maui.MessagingCenter;
```

Or add it globally in your `GlobalUsings.cs` file:

```csharp
global using Plugin.Maui.MessagingCenter;
```

That's it! You can now use the `MessagingCenter` class just like you did in .NET MAUI. Since the API is compatible with the .NET MAUI MessagingCenter, you can use it in the same way as before, there should be no need to change your existing code.

### 2. Basic Usage Examples

#### Sending Messages with Arguments

```csharp
// Send a message with data
MessagingCenter.Send<MainPage, string>(this, "LocationUpdate", "New York");

// In another class, subscribe to receive the message
MessagingCenter.Subscribe<MainPage, string>(this, "LocationUpdate", (sender, location) =>
{
    // Handle the location update
    DisplayAlert("Location", $"New location: {location}", "OK");
});
```

#### Sending Messages without Arguments

```csharp
// Send a simple notification message
MessagingCenter.Send<MainPage>(this, "RefreshData");

// Subscribe to the notification
MessagingCenter.Subscribe<MainPage>(this, "RefreshData", (sender) =>
{
    // Refresh your data
    LoadData();
});
```

#### Using with ViewModels

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public void NotifyDataChanged()
    {
        // Send message from ViewModel
        MessagingCenter.Send<MainViewModel, DataModel>(this, "DataUpdated", newData);
    }
}

public partial class DetailPage : ContentPage
{
    public DetailPage()
    {
        InitializeComponent();
        
        // Subscribe to ViewModel messages
        MessagingCenter.Subscribe<MainViewModel, DataModel>(this, "DataUpdated", (sender, data) =>
        {
            // Update UI with new data
            UpdateDisplay(data);
        });
    }
}
```

### 3. Important: Unsubscribing

Always unsubscribe when your object is disposed to prevent memory leaks:

```csharp
public partial class MyPage : ContentPage
{
    protected override void OnDisappearing()
    {
        // Unsubscribe from all messages this page subscribed to
        MessagingCenter.Unsubscribe<MainViewModel, DataModel>(this, "DataUpdated");
        MessagingCenter.Unsubscribe<MainPage>(this, "RefreshData");
        
        base.OnDisappearing();
    }
}
```

### 4. Sender Filtering

You can filter messages to only receive them from specific senders:

```csharp
// Only receive messages from a specific instance
var specificViewModel = new MainViewModel();
MessagingCenter.Subscribe<MainViewModel, string>(this, "StatusUpdate", 
    (sender, status) => {
        // Handle status update
    }, specificViewModel); // Only from this specific instance
```

## API Usage

The API is compatible with the .NET MAUI MessagingCenter APIs. For more detailed documentation, see the .NET MAUI MessagingCenter [reference](https://learn.microsoft.com/dotnet/maui/fundamentals/messagingcenter) and the MVVM Toolkit Messenger [documentation](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/messenger).

## Important Behavioral Differences

⚠️ **This implementation has stricter subscription behavior than the original .NET MAUI MessagingCenter:**

- **Multiple subscriptions to the same message type by the same subscriber will throw an `InvalidOperationException`**
- This prevents accidental duplicate subscriptions and potential memory leaks
- If you need multiple handlers, consider using different message names or consolidating logic into a single handler

For detailed information about behavioral differences, see [BEHAVIOR_DIFFERENCES.md](BEHAVIOR_DIFFERENCES.md).

## Acknowledgements

The bubble icon has kindly been provided by [Smashicons](https://www.freepik.com/icon/chat_134719#fromView=keyword&page=1&position=1&uuid=c6e6ca66-ab2d-44de-85e9-6138fbc90df6)
