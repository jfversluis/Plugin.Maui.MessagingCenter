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

## API Usage

After you have installed this library, you can either add the using statement at the top of each file where you are using MessagingCenter (`using Plugin.Maui.MessagingCenter;`), or opt for a [global using](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/using-directive#global-modifier) statement so you only need to do it once.

The API can be used the same as the .NET MAUI MessagingCenter APIs. You probably came here because you are already using the MessagingCenter in your .NET MAUI app, so you probably don't need more explanation. If you do need a reference, please find the documentation on the .NET MAUI MessagingCenter [here](https://learn.microsoft.com/dotnet/maui/fundamentals/messagingcenter) and the MVVM Toolkit Messenger [here](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/messenger).

## Acknowledgements

The bubble icon has kindly been provided by [Smashicons](https://www.freepik.com/icon/chat_134719#fromView=keyword&page=1&position=1&uuid=c6e6ca66-ab2d-44de-85e9-6138fbc90df6)
