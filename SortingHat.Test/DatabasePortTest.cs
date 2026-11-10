using System;
using Autofac;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.DI;
using Xunit;

namespace SortingHat.Test;

/// <summary>The settings and the statistics of the real SQLite databases in a temporary folder.</summary>
public sealed class DatabasePortTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAKeyThatWasNeverSetThenItHasNoValue()
        => Assert.None(_hat.InScope(scope => scope.Resolve<ISettings>().Get("never-set")));

    [Fact]
    public void GivenAKeySetTwiceThenItHasTheLastValue()
    {
        _hat.InScope(scope => Set(scope.Resolve<ISettings>(), "colour", "red"));
        _hat.InScope(scope => Set(scope.Resolve<ISettings>(), "colour", "blue"));

        Assert.Some("blue", _hat.InScope(scope => scope.Resolve<ISettings>().Get("colour")));
    }

    [Fact]
    public void GivenAProjectThenTheMainDatabaseCountsItsProjects()
    {
        var before = ProjectCount();

        _hat.Hat(["new-project", "tax"]);

        Assert.Equal(before.Select(count => count + 1), ProjectCount());
    }

    [Fact]
    public void GivenATaggedFileThenTheProjectDatabaseCountsIt()
    {
        _hat.TaggedFile("a.txt", ":x");

        var statistics = _hat.InScope(scope => scope.Resolve<IProjectDatabase>().GetStatistics());

        Assert.Some(1L, statistics.GetValueOrNone("Files"));
    }

    public void Dispose()
        => _hat.Dispose();

    private Option<long> ProjectCount()
        => _hat.InScope(scope => scope.Resolve<IMainDatabase>().GetStatistics()).GetValueOrNone("Projects");

    private static bool Set(ISettings settings, string key, string value)
    {
        settings.Set(key, value);

        return true;
    }
}
