using System;
using Autofac;
using SortingHat.API.DI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Runs a command that declares its arguments and options, registered like a plugin would register it, through the real command line application.
/// </summary>
public sealed class DeclaredCommandTest : IDisposable
{
    private readonly HatSandbox _hat = new(builder => builder.RegisterType<GreetCommand>().As<IDeclaredCommand>());

    [Fact]
    public void GivenItsArgumentThenTheCommandRuns()
    {
        Assert.Equal(0, _hat.Hat(["greet", "Harry"]));
        Assert.Equal(["Hello, Harry!"], _hat.Lines);
    }

    [Theory]
    [InlineData("--loud")]
    [InlineData("-l")]
    public void GivenItsOptionThenTheCommandSeesIt(string loud)
    {
        Assert.Equal(0, _hat.Hat(["greet", loud, "Harry"]));
        Assert.Equal(["HELLO, HARRY!"], _hat.Lines);
    }

    [Fact]
    public void GivenAnAliasThenTheCommandRuns()
    {
        Assert.Equal(0, _hat.Hat(["hi", "Harry"]));
        Assert.Equal(["Hello, Harry!"], _hat.Lines);
    }

    [Fact]
    public void GivenACommandThatFailsThenTheExitCodeIsNotZero()
        => Assert.NotEqual(0, _hat.Hat(["greet", "nobody"]));

    [Fact]
    public void GivenAMissingArgumentThenItIsReportedAndTheCommandDoesNotRun()
    {
        Assert.NotEqual(0, _hat.Hat(["greet"]));
        Assert.DoesNotContain(_hat.Lines, line => line.StartsWith("Hello"));
    }

    [Fact]
    public void GivenTooManyArgumentsThenTheyAreReportedAndTheCommandDoesNotRun()
    {
        Assert.NotEqual(0, _hat.Hat(["greet", "Harry", "Ron"]));
        Assert.Contains(_hat.Lines, line => line.Contains("'Ron'"));
        Assert.DoesNotContain(_hat.Lines, line => line.StartsWith("Hello"));
    }

    [Theory]
    [InlineData("--verbose")]
    [InlineData("--lould")]
    public void GivenAnOptionTheCommandDoesNotDeclareThenItIsReportedAndTheCommandDoesNotRun(string option)
    {
        Assert.NotEqual(0, _hat.Hat(["greet", "Harry", option]));
        Assert.Contains(_hat.Lines, line => line.Contains($"'{option}'"));
        Assert.DoesNotContain(_hat.Lines, line => line.StartsWith("Hello"));
    }

    [Fact]
    public void GivenTheHelpOptionThenTheDeclaredArgumentsAndOptionsAreDescribed()
    {
        Assert.Equal(0, _hat.Hat(["greet", "-h"]));

        Assert.Contains(_hat.Lines, line => line.Contains("Greets someone."));
        Assert.Contains(_hat.Lines, line => line.Contains("Everyone deserves a greeting, even the Slytherins."));
        Assert.Contains(_hat.Lines, line => line.Contains("<who>") && line.Contains("The one to greet."));
        Assert.Contains(_hat.Lines, line => line.Contains("-l, --loud") && line.Contains("Greets in capitals."));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("--verbose"));
    }

    [Fact]
    public void GivenTheHelpCommandThenTheOverviewListsTheCommand()
    {
        Assert.Equal(0, _hat.Hat(["help"]));
        Assert.Contains(_hat.Lines, line => line.Contains("greet") && line.Contains("hi") && line.Contains("Greets someone."));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("Slytherins"));
    }

    [Fact]
    public void GivenTheHelpCommandForTheCommandThenItsHelpIsShown()
    {
        Assert.Equal(0, _hat.Hat(["help", "greet"]));
        Assert.Contains(_hat.Lines, line => line.Contains("<who>") && line.Contains("The one to greet."));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("<command> [<arguments>] [<options>]"));
    }

    public void Dispose()
        => _hat.Dispose();
}
