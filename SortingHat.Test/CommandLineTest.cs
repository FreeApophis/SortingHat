using System;
using System.Linq;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Parses command lines through the real command line application, with a real SQLite database in a temporary folder.
/// </summary>
public sealed class CommandLineTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenACommandThatSucceedsThenTheExitCodeIsZero()
        => Assert.Equal(0, _hat.Hat(["list-projects"]));

    [Fact]
    public void GivenACommandThatFailsThenTheExitCodeIsNotZero()
        => Assert.NotEqual(0, _hat.Hat(["remove-project", "typo"]));

    [Fact]
    public void GivenAnUnknownCommandThenItIsReportedAndTheExitCodeIsNotZero()
    {
        Assert.NotEqual(0, _hat.Hat(["fly"]));
        Assert.Contains(_hat.Lines, line => line.Contains("'fly'"));
    }

    [Fact]
    public void GivenAnUnknownOptionThenItIsReportedAndTheCommandDoesNotRun()
    {
        Assert.NotEqual(0, _hat.Hat(["new-project", "myProject", "--dryrun"]));
        Assert.Contains(_hat.Lines, line => line.Contains("'--dryrun'"));

        _hat.Hat(["list-projects"]);
        Assert.Equal(["* Default"], _hat.Lines);
    }

    [Theory]
    [InlineData("help", "--verbose")]
    [InlineData("help", "-v")]
    public void GivenAnOptionAfterTheCommandThenTheCommandSeesIt(string first, string second)
    {
        Assert.Equal(0, _hat.Hat([first, second]));
        Assert.Contains(_hat.Lines, line => line.StartsWith("=>"));
    }

    [Fact]
    public void GivenNoOptionThenTheCommandDoesNotSeeIt()
    {
        _hat.Hat(["help"]);

        Assert.DoesNotContain(_hat.Lines, line => line.StartsWith("=>"));
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    public void GivenTheHelpOptionOnACommandThenItsHelpIsShownInsteadOfRunningIt(string help)
    {
        Assert.Equal(0, _hat.Hat(["new-project", "myProject", help]));
        Assert.Contains(_hat.Lines, line => line.Contains("Creates a new project database with a given name."));

        _hat.Hat(["list-projects"]);
        Assert.Equal(["* Default"], _hat.Lines);
    }

    [Fact]
    public void GivenAnArgumentThatLooksLikeAnOptionAfterTheSeparatorThenItIsAnArgument()
    {
        Assert.Equal(0, _hat.Hat(["new-project", "--", "-dash"]));

        _hat.Hat(["list-projects"]);
        Assert.Contains("  -dash", _hat.Lines);
    }

    [Fact]
    public void GivenAShortCommandThenItRunsTheCommand()
    {
        var path = _hat.TaggedFile("a.txt");

        Assert.Equal(0, _hat.Hat(["tag", ":x", path]));
        Assert.Equal(["a.txt"], _hat.FindFiles(":x").ToList());
    }

    public void Dispose()
        => _hat.Dispose();
}
