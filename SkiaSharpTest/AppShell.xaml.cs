using System.Reflection;
using SkiaSharp;

namespace SkiaSharpTest;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		var version = typeof(SKSurface).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		if (string.IsNullOrEmpty(version))
		{
			version = typeof(SKSurface).Assembly.GetName().Version?.ToString();
		}
		else if (version.Contains('+'))
		{
			version = version.Substring(0, version.IndexOf('+'));
		}

		var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
		MainShellContent.Title = $"SkiaSharp Performance Test (v{version}) - {displayInfo.Width}x{displayInfo.Height} - [Mock GnollHack]";
	}
}
