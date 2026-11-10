using SortingHat.Desktop.Core;
using Xunit;

namespace SortingHat.Desktop.Test;

/// <summary>The pure core of the main window, without any window.</summary>
public sealed class MainUpdateTest
{
    [Fact]
    public void GivenStartupWarningsThenTheyFollowTheMessagesSoFar()
    {
        var state = MainState.Initial with { Messages = ["earlier"] };

        var transition = MainUpdate.Update(state, new Message.StartupWarningsLoaded(["Warning: one", "Warning: two"]));

        Assert.Equal(["earlier", "Warning: one", "Warning: two"], transition.State.Messages);
        Assert.Empty(transition.Effects);
    }

    [Fact]
    public void GivenALineFromAPluginThenItIsTheLastMessage()
    {
        var transition = MainUpdate.Update(MainState.Initial, new Message.Wrote("moved a.pdf"));

        Assert.Equal(["moved a.pdf"], transition.State.Messages);
    }

    [Fact]
    public void GivenTheStartThenTheWindowLoadsTheStartupWarningsAndTheTags()
        => Assert.Equal([new Effect.LoadStartupWarnings(), new Effect.LoadTags()], MainState.Start);
}
