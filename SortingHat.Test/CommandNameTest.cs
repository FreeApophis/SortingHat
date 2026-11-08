using System;
using System.Linq;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Commands from plugins that claim a name or an alias another command already has, through the real command line application.
/// The first command keeps it: the built-in commands come first, then the others in the order they were registered.
/// </summary>
public sealed class CommandNameTest
{
    [Fact]
    public void GivenOnlyTheBuiltInCommandsThenNoNameClashes()
    {
        using var hat = new HatSandbox();

        Assert.Equal(0, hat.Hat(["version"]));

        Assert.DoesNotContain(hat.Lines, line => line.StartsWith("Warning"));
    }

    [Fact]
    public void GivenAPluginCommandNamedLikeABuiltInAliasThenTheBuiltInCommandKeepsIt()
    {
        using var hat = new HatSandbox(builder => NamedCommand.Register(builder, "tag"));
        var path = hat.WriteFile("a.txt", "a");

        Assert.Equal(0, hat.Hat(["tag", ":x", path]));

        Assert.DoesNotContain(hat.Lines, line => line == "tag ran");
        Assert.Equal(["a.txt"], hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenAPluginCommandNamedLikeABuiltInAliasThenHatWarnsThatItIsLeftOut()
    {
        using var hat = new HatSandbox(builder => NamedCommand.Register(builder, "tag"));

        hat.Hat(["version"]);

        Assert.Contains("Warning: command 'tag' (SortingHat.Test) left out: 'tag' is already an alias of 'tag-files'.", hat.Lines);
    }

    [Fact]
    public void GivenAPluginCommandNamedLikeABuiltInCommandThenHatWarnsThatItIsLeftOut()
    {
        using var hat = new HatSandbox(builder => NamedCommand.Register(builder, "duplicate"));

        hat.Hat(["duplicate"]);

        Assert.Contains("Warning: command 'duplicate' (SortingHat.Test) left out: 'duplicate' is already the command 'duplicate'.", hat.Lines);
        Assert.Contains("There are no duplicates.", hat.Lines);
    }

    [Fact]
    public void GivenAPluginAliasOfABuiltInCommandThenOnlyTheAliasIsDropped()
    {
        using var hat = new HatSandbox(builder => NamedCommand.Register(builder, "detect", "d", "det"));

        Assert.Equal(0, hat.Hat(["det"]));
        Assert.Contains("detect ran", hat.Lines);
        Assert.Contains("Warning: alias 'd' of 'detect' (SortingHat.Test) dropped: 'd' is already an alias of 'duplicate'.", hat.Lines);

        hat.Hat(["d"]);
        Assert.Contains("There are no duplicates.", hat.Lines);
    }

    [Fact]
    public void GivenTwoPluginCommandsWithTheSameNameThenTheFirstKeepsIt()
    {
        using var hat = new HatSandbox(builder =>
        {
            NamedCommand.Register(builder, "same", "first");
            NamedCommand.Register(builder, "same", "second");
        });

        Assert.Equal(0, hat.Hat(["same"]));

        Assert.Equal(1, hat.Lines.Count(line => line == "same ran"));
        Assert.Contains("Warning: command 'same' (SortingHat.Test) left out: 'same' is already the command 'same'.", hat.Lines);
        Assert.Equal(0, hat.Hat(["first"]));
        Assert.NotEqual(0, hat.Hat(["second"]));
    }

    [Fact]
    public void GivenADroppedAliasThenTheOverviewDoesNotShowIt()
    {
        using var hat = new HatSandbox(builder => NamedCommand.Register(builder, "detect", "d", "det"));

        Assert.Equal(0, hat.Hat(["help"]));

        Assert.Contains("detect|det|The test command detect.", hat.Rows);
    }

    [Fact]
    public void GivenAnAliasEqualToItsOwnNameThenNothingIsWarned()
    {
        using var hat = new HatSandbox(builder => NamedCommand.Register(builder, "detect", "detect", "det", "det"));

        Assert.Equal(0, hat.Hat(["det"]));

        Assert.DoesNotContain(hat.Lines, line => line.StartsWith("Warning"));
    }
}
