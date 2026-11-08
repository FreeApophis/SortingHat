using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SortingHat.CLI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Every command hat offers, built-in and from the real plugins, has a description and a help, through the real command line application.
/// The commands come from hat itself, so a new command is checked without being listed here.
/// </summary>
public sealed class CommandHelpTest : IDisposable
{
    private readonly HatSandbox _hat = HatSandbox.WithPluginFixtures("Working");

    [Fact]
    public void GivenTheRealPluginsThenTheirCommandsAreCheckedToo()
    {
        var names = Commands().Select(command => command.Name).ToList();

        Assert.Contains("tag-files", names);
        Assert.Contains("identify", names);
        Assert.Contains("move-rule", names);
    }

    [Fact]
    public void GivenAnyCommandThenItsDescriptionIsOneSentence()
        => Assert.All(Commands(), command => Assert.Matches(@"^[A-Z][^\r\n]*\.$", command.Description));

    [Fact]
    public void GivenAnyCommandThenHelpShowsItsUsageAndDescription()
        => Assert.All(Commands(), command =>
        {
            Assert.Equal(0, _hat.Hat(["help", command.Name]));

            Assert.Contains("Usage:", _hat.Lines);
            Assert.Contains(_hat.Lines, line => line.Contains(command.Description));
        });

    [Fact]
    public void GivenAnyCommandThenTheOverviewListsIt()
    {
        Assert.Equal(0, _hat.Hat(["help"]));

        Assert.All(Commands(), command => Assert.Contains(_hat.Rows, row => row.StartsWith($"{command.Name}|")));
    }

    public void Dispose()
        => _hat.Dispose();

    /// <summary>The commands as hat offers them, with the plugins loaded.</summary>
    private IReadOnlyList<API.DI.IDeclaredCommand> Commands()
        => _hat.InScope(scope => scope.Resolve<CommandNames>().Offered.Select(offered => offered.Command).ToList());
}
