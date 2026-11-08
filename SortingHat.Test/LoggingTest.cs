using System.IO;
using Autofac;
using SortingHat.API.DI;
using SortingHat.CLI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// hat's log, through the real command line application: a log file per day in the 'logs' folder next to the databases.
/// The console shows short messages, the log keeps the details.
/// </summary>
public sealed class LoggingTest
{
    [Fact]
    public void GivenAnUnexpectedErrorThenTheConsoleShowsAShortMessageWithoutTheStackTrace()
    {
        using var hat = new HatSandbox(builder => builder.RegisterType<ThrowingCommand>().As<IDeclaredCommand>());

        Assert.NotEqual(0, hat.Hat(["crash"]));

        Assert.Contains(hat.Lines, line => line.Contains("broke on purpose") && line.Contains(hat.LogFolder));
        Assert.DoesNotContain(hat.Lines, line => line.TrimStart().StartsWith("at "));
    }

    [Fact]
    public void GivenAnUnexpectedErrorThenTheLogHasItsStackTrace()
    {
        using var hat = new HatSandbox(builder => builder.RegisterType<ThrowingCommand>().As<IDeclaredCommand>());

        hat.Hat(["crash"]);

        Assert.Contains("broke on purpose", hat.Log);
        Assert.Contains($"{nameof(ThrowingCommand)}.{nameof(ThrowingCommand.ExecuteAsync)}", hat.Log);
    }

    [Fact]
    public void GivenTaggingThenTheLogRecordsWhatWasTagged()
    {
        using var hat = new HatSandbox();
        var path = hat.TaggedFile("a.txt", ":tax");

        Assert.Contains($"{path}", hat.Log);
        Assert.Contains(":tax", hat.Log);
    }

    [Fact]
    public void GivenAClassOfAPluginThenItsLogEntriesNameIt()
    {
        using var hat = HatSandbox.WithPluginFixtures("Working");
        var tax = hat.CreateFolder("tax");
        hat.Hat(["move-rule", ":tax", tax]);

        hat.TaggedFile("a.txt", ":tax");

        Assert.Contains("SortingHat.Plugin.MoveOnTag.MoveOnTagHandler", hat.Log);
        Assert.Contains(Path.Combine(tax, "a.txt"), hat.Log);
    }

    [Fact]
    public void GivenAClassOfHatThenItsLogEntriesNameIt()
    {
        using var hat = new HatSandbox();

        hat.TaggedFile("a.txt", ":tax");

        Assert.Contains("SortingHat.CLI.Commands.Files.TagFileCommand", hat.Log);
    }

    [Fact]
    public void GivenAMinimumLevelOfWarningThenTaggingIsNotLogged()
    {
        using var hat = new HatSandbox(builder => builder.RegisterInstance(new LogSettings { MinimumLevel = "Warning" }));

        hat.TaggedFile("a.txt", ":tax");

        Assert.DoesNotContain(":tax", hat.Log);
    }

    [Fact]
    public void GivenALevelThatIsNoLevelThenHatWarnsAndLogsInformation()
    {
        using var hat = new HatSandbox(builder => builder.RegisterInstance(new LogSettings { MinimumLevel = "Loud" }));

        Assert.Equal(0, hat.Hat(["tag-files", ":tax", hat.WriteFile("a.txt", "a")]));

        Assert.Contains(hat.Lines, line => line.StartsWith("Warning:") && line.Contains("'Loud'") && line.Contains("Information"));
        Assert.Contains(":tax", hat.Log);
    }
}
