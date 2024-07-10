namespace Plugin.Maui.MessagingCenter.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		MessagingCenter.Send<MainPage>(this, "Hi");
	}

	private void Subscribe_Clicked(object sender, EventArgs e)
	{
		MessagingCenter.Subscribe<MainPage>(this, "Hi", (sender) =>
		{
			// Do something whenever the "Hi" message is received
			DisplayAlert("Message Received", "Hi", "OK");
		});
	}

	private void Unsubscribe_Clicked(object sender, EventArgs e)
	{
		MessagingCenter.Unsubscribe<MainPage>(this, "Hi");
	}
}
