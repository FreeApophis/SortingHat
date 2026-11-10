using System.Threading.Tasks;
using Autofac;
using SortingHat.App;
using Xunit;

namespace SortingHat.Desktop.Test;

public sealed class StartupTest
{
    [Fact]
    public Task GivenAnUnknownLogLevelThenTheWindowShowsTheWarning()
        => HeadlessApplication.Run(async () =>
        {
            await using var desktop = new DesktopSandbox(builder => builder.RegisterInstance(new LogSettings { MinimumLevel = "Loud" }));

            await desktop.Idle();

            Assert.Contains(desktop.Texts("MessagePanel"), line => line.StartsWith("Warning:") && line.Contains("'Loud'"));
        });

    [Fact]
    public Task GivenNoProblemsThenTheWindowShowsNoMessages()
        => HeadlessApplication.Run(async () =>
        {
            await using var desktop = new DesktopSandbox();

            await desktop.Idle();

            Assert.Empty(desktop.Texts("MessagePanel"));
        });
}
