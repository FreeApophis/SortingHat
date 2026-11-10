using System.Threading.Tasks;
using Autofac;
using Avalonia.Headless.XUnit;
using SortingHat.App;
using Xunit;

namespace SortingHat.Desktop.Test;

public sealed class StartupTest
{
    [AvaloniaFact]
    public async Task GivenAnUnknownLogLevelThenTheWindowShowsTheWarning()
    {
        using var desktop = new DesktopSandbox(builder => builder.RegisterInstance(new LogSettings { MinimumLevel = "Loud" }));

        await desktop.Idle();

        Assert.Contains(desktop.Texts("MessagePanel"), line => line.StartsWith("Warning:") && line.Contains("'Loud'"));
    }

    [AvaloniaFact]
    public async Task GivenNoProblemsThenTheWindowShowsNoMessages()
    {
        using var desktop = new DesktopSandbox();

        await desktop.Idle();

        Assert.Empty(desktop.Texts("MessagePanel"));
    }
}
