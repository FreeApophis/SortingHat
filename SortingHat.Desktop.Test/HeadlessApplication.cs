using Avalonia;
using Avalonia.Headless;
using SortingHat.Desktop.Test;

[assembly: AvaloniaTestApplication(typeof(HeadlessApplication))]

namespace SortingHat.Desktop.Test;

/// <summary>The real desktop app without a screen: every [AvaloniaFact] runs on its UI thread.</summary>
public static class HeadlessApplication
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<DesktopApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
