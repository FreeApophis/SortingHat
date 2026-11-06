using System;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Manages projects through the real command line application, with a real SQLite database in a temporary folder.
/// </summary>
public sealed class ProjectCommandTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAValidNameThenNewProjectCreatesTheProject()
    {
        _hat.Hat(["new-project", "myProject"]);

        Assert.Contains("  myProject", ListProjects());
    }

    [Fact]
    public void GivenNoNameThenNewProjectAsksForOne()
    {
        _hat.Hat(["new-project"]);

        Assert.Equal(["Not enough arguments given, please give a name for the project you want to create."], _hat.Lines);
    }

    [Fact]
    public void GivenTwoNamesThenNewProjectCreatesNone()
    {
        _hat.Hat(["new-project", "a", "b"]);

        Assert.Equal(["Too many arguments given, only one project at a time can be created."], _hat.Lines);
        Assert.Equal(["* Default"], ListProjects());
    }

    [Theory]
    [InlineData("a/b")]
    [InlineData("hat")]
    [InlineData("default")]
    public void GivenAnInvalidNameThenNewProjectReportsItAndCreatesNothing(string name)
    {
        _hat.Hat(["new-project", name]);

        Assert.Single(_hat.Lines, line => line.Contains($"'{name}'"));
        Assert.Equal(["* Default"], ListProjects());
    }

    [Fact]
    public void GivenTheNameOfTheMainDatabaseThenImportLeavesTheMainDatabaseAlone()
    {
        var notADatabase = _hat.TaggedFile("other.db");

        _hat.Hat(["import", notADatabase, "hat"]);

        Assert.Single(_hat.Lines, line => line.Contains("'hat'"));

        // an overwritten main database could not list its projects any more
        Assert.Equal(["* Default"], ListProjects());
    }

    public void Dispose()
        => _hat.Dispose();

    private string[] ListProjects()
    {
        _hat.Hat(["list-projects"]);

        return [.. _hat.Lines];
    }
}
