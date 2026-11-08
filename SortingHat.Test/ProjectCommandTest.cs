using System;
using System.IO;
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
        Assert.NotEqual(0, _hat.Hat(["new-project"]));
        Assert.Contains(_hat.Lines, line => line.Contains("project-name"));
    }

    [Fact]
    public void GivenTwoNamesThenNewProjectCreatesNone()
    {
        Assert.NotEqual(0, _hat.Hat(["new-project", "a", "b"]));
        Assert.Contains(_hat.Lines, line => line.Contains("'b'"));
        Assert.Equal(["* Default"], ListProjects());
    }

    [Fact]
    public void GivenAnExistingProjectThenSwitchProjectMakesItActive()
    {
        _hat.Hat(["new-project", "myProject"]);

        Assert.Equal(0, _hat.Hat(["switch-project", "myProject"]));
        Assert.Equal(["  Default", "* myProject"], ListProjects());
    }

    [Fact]
    public void GivenNoNameThenSwitchProjectAsksForOneInsteadOfCrashing()
    {
        Assert.NotEqual(0, _hat.Hat(["switch-project"]));
        Assert.Contains(_hat.Lines, line => line.Contains("project-name"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("Sequence contains no elements"));
    }

    [Fact]
    public void GivenNoFileThenImportAsksForOneInsteadOfCrashing()
    {
        Assert.NotEqual(0, _hat.Hat(["import"]));
        Assert.Contains(_hat.Lines, line => line.Contains("project-file"));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("not implemented"));
    }

    [Fact]
    public void GivenAnExportedProjectThenImportAddsItUnderANewName()
    {
        ProjectWithDatabase("myProject");
        var exported = _hat.ProjectDatabaseFile("exported");

        Assert.Equal(0, _hat.Hat(["export", "myProject", exported]));
        Assert.Equal(0, _hat.Hat(["import", exported, "copy"]));

        _hat.Hat(["switch-project", "copy"]);
        Assert.Equal(["myProject.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenTheHelpOfRemoveProjectThenItWarnsAboutDeletingTheDatabase()
    {
        Assert.Equal(0, _hat.Hat(["remove-project", "--help"]));
        Assert.Contains(_hat.Lines, line => line.Contains("BE CAREFUL"));
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

    [Theory]
    [InlineData("y")]
    [InlineData("Y")]
    public void GivenConfirmationThenRemoveProjectDeletesTheProjectAndItsDatabase(string answer)
    {
        ProjectWithDatabase("myProject");
        _hat.Type(answer);

        _hat.Hat(["remove-project", "myProject"]);

        Assert.Contains("Removing the project 'myProject' deletes its database with all its files and tags. The files on disk stay where they are.", _hat.Lines);
        Assert.Equal(["* Default"], ListProjects());
        Assert.False(File.Exists(_hat.ProjectDatabaseFile("myProject")));
    }

    [Theory]
    [InlineData("n")]
    [InlineData("")]
    public void GivenNoConfirmationThenRemoveProjectKeepsTheProjectAndItsDatabase(string answer)
    {
        ProjectWithDatabase("myProject");
        _hat.Type(answer);

        _hat.Hat(["remove-project", "myProject"]);

        Assert.Equal(["* Default", "  myProject"], ListProjects());
        Assert.True(File.Exists(_hat.ProjectDatabaseFile("myProject")));
    }

    [Fact]
    public void GivenAnAnswerThatIsNeitherYesNorNoThenRemoveProjectAsksAgain()
    {
        ProjectWithDatabase("myProject");
        _hat.Type("yep", "n");

        _hat.Hat(["remove-project", "myProject"]);

        Assert.Equal(["* Default", "  myProject"], ListProjects());
    }

    [Fact]
    public void GivenNoInteractiveTerminalThenRemoveProjectKeepsTheProjectAndNamesTheOption()
    {
        ProjectWithDatabase("myProject");

        Assert.NotEqual(0, _hat.Hat(["remove-project", "myProject"]));

        Assert.Contains(_hat.Lines, line => line.Contains("interactive terminal") && line.Contains("--yes"));
        Assert.Equal(["* Default", "  myProject"], ListProjects());
    }

    [Fact]
    public void GivenYesThenRemoveProjectRemovesWithoutAsking()
    {
        ProjectWithDatabase("myProject");

        Assert.Equal(0, _hat.Hat(["remove-project", "--yes", "myProject"]));

        Assert.Equal(["* Default"], ListProjects());
        Assert.False(File.Exists(_hat.ProjectDatabaseFile("myProject")));
    }

    [Fact]
    public void GivenAProjectWithoutDatabaseThenRemoveProjectRemovesIt()
    {
        _hat.Hat(["new-project", "myProject"]);
        _hat.Type("y");

        _hat.Hat(["remove-project", "myProject"]);

        Assert.Equal(["* Default"], ListProjects());
    }

    [Fact]
    public void GivenTheNameInAnotherCaseThenRemoveProjectRemovesTheProject()
    {
        ProjectWithDatabase("myProject");
        _hat.Type("y");

        _hat.Hat(["remove-project", "MYPROJECT"]);

        Assert.Equal(["* Default"], ListProjects());
        Assert.False(File.Exists(_hat.ProjectDatabaseFile("myProject")));
    }

    [Fact]
    public void GivenTheActiveProjectThenRemoveProjectRefusesWithoutAsking()
    {
        _hat.Type("y");

        _hat.Hat(["remove-project", "Default"]);

        Assert.Equal(["The project 'Default' is in use, switch to another project before you remove it."], _hat.Lines);
        Assert.Equal(["* Default"], ListProjects());
    }

    [Fact]
    public void GivenAnUnknownProjectThenRemoveProjectReportsIt()
    {
        _hat.Hat(["remove-project", "typo"]);

        Assert.Equal(["There is no project with the name 'typo'."], _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();

    /// <summary>A project database file only exists once the project has been used.</summary>
    private void ProjectWithDatabase(string project)
    {
        _hat.Hat(["new-project", project]);
        _hat.Hat(["switch-project", project]);
        _hat.TaggedFile($"{project}.txt", ":x");
        _hat.Hat(["switch-project", "Default"]);

        Assert.True(File.Exists(_hat.ProjectDatabaseFile(project)));
    }

    private string[] ListProjects()
    {
        _hat.Hat(["list-projects"]);

        return [.. _hat.Lines];
    }
}
