using Microsoft.Extensions.Logging;

namespace TBTNavigation;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureMauiHandlers(handlers =>
			{
#if ANDROID
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, TBTNavigation.Platforms.Android.CustomMapHandler>();
#elif IOS
            handlers.AddHandler<Microsoft.Maui.Controls.Maps.Map, TBTNavigation.Platforms.iOS.CustomMapHandler>();
#endif
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
