using System;
using Autofac;
using SortingHat.API.DI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Questions a command asks through <see cref="IUserPrompt"/>, through the real command line application;
/// the test types or presses what the user would. Without a terminal, hat cannot ask.
/// </summary>
public sealed class UserPromptTest : IDisposable
{
    private readonly HatSandbox _hat = new(builder => builder.RegisterType<AskCommand>().As<IDeclaredCommand>());

    [Fact]
    public void GivenYThenConfirmIsYes()
    {
        _hat.Type("y");

        _hat.Hat(["ask", "confirm"]);

        Assert.Contains("answer: yes", _hat.Lines);
    }

    [Fact]
    public void GivenEnterThenConfirmIsNo()
    {
        _hat.Type(string.Empty);

        _hat.Hat(["ask", "confirm"]);

        Assert.Contains("answer: no", _hat.Lines);
    }

    [Fact]
    public void GivenNoTerminalThenConfirmCannotAsk()
    {
        _hat.Hat(["ask", "confirm"]);

        Assert.Equal(["answer: cannot ask"], _hat.Lines);
    }

    [Fact]
    public void GivenTheArrowKeysThenChooseReturnsTheChoice()
    {
        _hat.Press(ConsoleKey.DownArrow, ConsoleKey.Enter);

        _hat.Hat(["ask", "choose"]);

        // without ANSI, the test console cannot erase the list of choices, so the answer follows the last choice on its line
        Assert.Contains(_hat.Lines, line => line.EndsWith("answer: green"));
    }

    [Fact]
    public void GivenChoicesThenChooseShowsThemAsDescribedAndAsTheyAre()
    {
        _hat.Press(ConsoleKey.Enter);

        _hat.Hat(["ask", "choose"]);

        Assert.Contains(_hat.Lines, line => line.Contains("BLUE [DARK]"));
        Assert.Contains(_hat.Lines, line => line.Contains("Which [colour]?"));
    }

    [Fact]
    public void GivenNoTerminalThenChooseCannotAsk()
    {
        _hat.Hat(["ask", "choose"]);

        Assert.Equal(["answer: cannot ask"], _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();
}
