using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SkiaSharpTest;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("archristy.ttf", "ARChristy");
				fonts.AddFont("DejaVuSansMono.ttf", "DejaVuSansMono");
				fonts.AddFont("DejaVuSansMono-Bold.ttf", "DejaVuSansMonoBold");
				fonts.AddFont("DejaVuSansMono-BoldOblique.ttf", "DejaVuSansMonoBoldOblique");
				fonts.AddFont("DejaVuSansMono-Oblique.ttf", "DejaVuSansMonoOblique");
				fonts.AddFont("diablo_h.ttf", "Diablo");
				fonts.AddFont("endr.ttf", "Endor");
				fonts.AddFont("Immortal-Regular.ttf", "Immortal");
				fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
				fonts.AddFont("Lato-Bold.ttf", "LatoBold");
				fonts.AddFont("shxi.ttf", "Xizor");
				fonts.AddFont("uwch.ttf", "Underwood");
			});

#if WINDOWS
		builder.ConfigureLifecycleEvents(events =>
		{
			events.AddWindows(windowsLifecycleBuilder =>
			{
				windowsLifecycleBuilder.OnWindowCreated(window =>
				{
					window.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);
				});
			});
		});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
