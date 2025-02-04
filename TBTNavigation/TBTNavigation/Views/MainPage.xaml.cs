namespace TBTNavigation;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		Navigation.PushAsync(new EnRoutePage());
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new EnRoutePage());
	}
}

