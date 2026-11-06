using System;
using System.IO;
using Autofac;
using SortingHat.API.DI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Runs the general commands (help, plugins, repair, statistics, version) through the real command line application,
/// with a real SQLite database in a temporary folder and an old-style command like a plugin could still contribute it.
/// </summary>
public sealed class GeneralCommandTest : IDisposable
{
    private const string OverviewHeader = "<command> [<arguments>] [<options>]";

    private readonly HatSandbox _hat = new(builder => builder.RegisterType<EchoLegacyCommand>().As<ICommand>());

    private static readonly string[] CommandNames =
    [
        "help", "plugins", "repair", "statistics", "version",
        "add-tags", "list-tags", "move-tags", "remove-tags", "rename-tag",
        "copy-files", "duplicate", "file-info", "find-files", "move-files", "tag-files", "untag-files",
        "new-project", "remove-project", "export", "import", "list-projects", "switch-project",
        "echo",
    ];

    public static TheoryData<string> Commands()
        => [.. CommandNames];

    [Fact]
    public void GivenNoCommandThenGiveHint()
    {
        Assert.NotEqual(0, _hat.Hat([]));
        Assert.Single(_hat.Lines, line => line.StartsWith("Maybe run '") && line.EndsWith(" help'"));
    }

    [Fact]
    public void GivenHelpThenTheOverviewListsEveryCommand()
    {
        Assert.Equal(0, _hat.Hat(["help"]));

        Assert.Contains(_hat.Lines, line => line.Contains(OverviewHeader));
        Assert.All(CommandNames, command => Assert.Contains(_hat.Lines, line => line.TrimStart().StartsWith(command)));
    }

    [Fact]
    public void GivenOnlyDeclaredCommandsThenTheOverviewOffersNoGlobalOptions()
    {
        _hat.Hat(["help"]);

        Assert.DoesNotContain(_hat.Lines, line => line.Contains("--dry-run") || line.Contains("--recursive"));
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public void GivenHelpWithACommandThenOnlyItsHelpIsShown(string command)
    {
        Assert.Equal(0, _hat.Hat(["help", command]));

        Assert.Contains(_hat.Lines, line => line.Contains($" {command}"));
        Assert.Contains("Usage:", _hat.Lines);
        Assert.DoesNotContain(_hat.Lines, line => line.Contains(OverviewHeader));
    }

    [Theory]
    [InlineData("ff", "find-files")]
    [InlineData("e", "echo")]
    public void GivenHelpWithAnAliasThenTheHelpOfItsCommandIsShown(string alias, string command)
    {
        Assert.Equal(0, _hat.Hat(["help", alias]));
        Assert.Contains(_hat.Lines, line => line.Contains($" {command}"));
    }

    [Fact]
    public void GivenHelpWithAnUnknownCommandThenItIsReported()
    {
        Assert.NotEqual(0, _hat.Hat(["help", "fly"]));
        Assert.Contains(_hat.Lines, line => line.Contains("'fly'"));
    }

    [Fact]
    public void GivenAnOldStyleCommandThenItStillRuns()
    {
        Assert.Equal(0, _hat.Hat(["echo", "a", "b"]));
        Assert.Equal(["a b"], _hat.Lines);
    }

    [Fact]
    public void GivenNoPluginsThenPluginsSaysSo()
    {
        Assert.Equal(0, _hat.Hat(["plugins"]));
        Assert.Equal(["No plugins loaded"], _hat.Lines);
    }

    [Fact]
    public void GivenAnArgumentThenPluginsRejectsIt()
        => Assert.NotEqual(0, _hat.Hat(["plugins", "extra"]));

    [Fact]
    public void GivenVersionThenItShowsTheProgramAndItsVersion()
    {
        Assert.Equal(0, _hat.Hat(["v"]));
        Assert.StartsWith("SortingHat ", _hat.Lines[0]);
    }

    [Fact]
    public void GivenATaggedFileThenStatisticsCountsIt()
    {
        _hat.TaggedFile("a.txt", ":x");

        Assert.Equal(0, _hat.Hat(["stat"]));
        Assert.Contains(_hat.Lines, line => line.Contains("Files") && line.TrimEnd().EndsWith("= 1"));
    }

    [Fact]
    public void GivenADeletedFileThenRepairReportsItAsMissingWithoutClaimingToRemoveIt()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        File.Delete(path);

        Assert.Equal(0, _hat.Hat(["repair"]));
        Assert.Contains(_hat.Lines, line => line.Contains(path) && line.Contains("missing"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("removed"));
    }

    public void Dispose()
        => _hat.Dispose();
}
