using System;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Avalonia;
using SortingHat.App;
using SortingHat.Desktop.Views;

namespace SortingHat.Desktop;

/// <summary>Builds the object graph with the plugins and runs the window; like hat's Program.Main, it cannot be tested.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        using var container = new DesktopCompositionRoot().Register().Build();
        using var scope = PluginScope.Begin(container);

        return AppBuilder.Configure(() => new DesktopApp(scope.Resolve<MainWindow>))
            .UsePlatformDetect()
            .WithInterFont()
            .StartWithClassicDesktopLifetime(args);
    }
}
