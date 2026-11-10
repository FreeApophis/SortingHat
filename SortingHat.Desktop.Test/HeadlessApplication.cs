using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless;
using Funcky;
using SortingHat.Desktop.Test;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(HeadlessApplication))]

namespace SortingHat.Desktop.Test;

/// <summary>The real desktop app without a screen: every test that opens a window runs on its UI thread through <see cref="Run"/>.</summary>
public static class HeadlessApplication
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<DesktopApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());

    /// <summary>Runs <paramref name="test"/> on the UI thread of the headless app and waits for it.</summary>
    public static Task Run(Func<Task> test)
        => HeadlessUnitTestSession
            .GetOrStartForAssembly(typeof(HeadlessApplication).Assembly)
            .Dispatch(
                async () =>
                {
                    await test();
                    return Unit.Value;
                },
                TestContext.Current.CancellationToken);
}
