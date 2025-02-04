namespace TBTNavigation;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("EnRoutePage", typeof(EnRoutePage));
	}
}
