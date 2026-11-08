using System;
using System.IO;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Runs the general commands (help, plugins, repair, statistics, version) through the real command line application,
/// with a real SQLite database in a temporary folder.
/// </summary>
public sealed class GeneralCommandTest : IDisposable
{
    private const string OverviewHeader = "<command> [<arguments>] [<options>]";

    private readonly HatSandbox _hat = new();

    private static readonly string[] CommandNames =
    [
        "help", "plugins", "repair", "statistics", "version",
        "add-tags", "list-tags", "move-tags", "remove-tags", "rename-tag",
        "copy-files", "duplicate", "file-info", "find-files", "move-files", "tag-files", "untag-files",
        "new-project", "remove-project", "export", "import", "list-projects", "switch-project",
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
    [InlineData("tags", "list-tags")]
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
        Assert.Contains("Files|1", _hat.Rows);
    }

    [Fact]
    public void GivenADeletedFileThenRepairRemovesItsPath()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        _hat.TaggedFile("b.txt", ":x");
        File.Delete(path);

        Assert.Equal(0, _hat.Hat(["repair"]));
        Assert.Contains(_hat.Lines, line => line.Contains("removed") && line.Contains(path));

        Assert.Equal(["b.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenARepairedDatabaseThenRepairFindsNothingMore()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        File.Delete(path);
        _hat.Hat(["repair"]);

        Assert.Equal(0, _hat.Hat(["repair"]));
        Assert.Contains("Every path in the database exists.", _hat.Lines);
    }

    [Theory]
    [InlineData("--dry-run")]
    [InlineData("-n")]
    public void GivenADryRunThenRepairShowsTheMissingPathsButKeepsThem(string dryRun)
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        _hat.TaggedFile("b.txt", ":x");
        File.Delete(path);

        Assert.Equal(0, _hat.Hat(["repair", dryRun]));
        Assert.Contains(_hat.Lines, line => line.Contains("would remove") && line.Contains(path));
        Assert.Contains("Dry run: nothing was removed.", _hat.Lines);

        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenAFileMovedOutsideHatThenAfterRepairStoringItAgainBringsBackItsTags()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var newPath = Path.Combine(Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, "elsewhere")).FullName, "a.txt");
        File.Move(path, newPath);

        _hat.Hat(["repair"]);
        _hat.Hat(["tag-files", newPath]);

        _hat.Hat(["ff", ":x"]);
        Assert.Contains(_hat.Lines, line => line.TrimEnd().EndsWith(newPath));
        Assert.DoesNotContain(_hat.Lines, line => line.TrimEnd().EndsWith(path));
    }

    [Fact]
    public void GivenTheHelpOfRepairThenItOffersADryRun()
    {
        Assert.Equal(0, _hat.Hat(["repair", "--help"]));
        Assert.Contains(_hat.Lines, line => line.Contains("-n, --dry-run"));
    }

    public void Dispose()
        => _hat.Dispose();
}
