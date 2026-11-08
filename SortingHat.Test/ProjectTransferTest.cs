using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Exports and imports projects through the real command line application, with real SQLite databases in a temporary folder.
/// Both copy through SQLite, so the copy is consistent, and neither overwrites a file.
/// </summary>
public sealed class ProjectTransferTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenChangesStillInTheWriteAheadLogThenTheExportContainsThem()
    {
        ProjectWithDatabase("myProject");
        using var reader = WriteAheadLogKeptOpen(_hat.ProjectDatabaseFile("myProject"));
        _hat.Hat(["switch-project", "myProject"]);
        _hat.TaggedFile("late.txt", ":late");
        var exported = Exported("myProject");

        Assert.Equal(0, _hat.Hat(["import", exported, "copy"]));

        _hat.Hat(["switch-project", "copy"]);
        Assert.Equal(["late.txt"], _hat.FindFiles(":late"));
    }

    [Fact]
    public void GivenTheActiveProjectThenItCanBeExported()
    {
        _hat.TaggedFile("a.txt", ":x");
        var exported = Exported("Default");

        Assert.Equal(0, _hat.Hat(["import", exported, "copy"]));

        _hat.Hat(["switch-project", "copy"]);
        Assert.Equal(["a.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenAnUnknownProjectThenExportReportsItAndWritesNothing()
    {
        var destination = Path.Combine(_hat.PluginDirectory, "..", "unknown.hdb");

        Assert.NotEqual(0, _hat.Hat(["export", "unknown", destination]));

        Assert.Equal(["There is no project with the name 'unknown'."], _hat.Lines);
        Assert.False(File.Exists(destination));
    }

    [Fact]
    public void GivenAProjectThatWasNeverUsedThenExportSaysThereIsNothingToExport()
    {
        _hat.Hat(["new-project", "empty"]);

        Assert.NotEqual(0, _hat.Hat(["export", "empty", _hat.ProjectDatabaseFile("exported")]));

        Assert.Equal(["The project 'empty' has no database yet, there is nothing to export."], _hat.Lines);
    }

    [Fact]
    public void GivenAnExistingDestinationThenExportLeavesItAlone()
    {
        ProjectWithDatabase("myProject");
        var destination = _hat.WriteFile("existing.hdb", "keep me");

        Assert.NotEqual(0, _hat.Hat(["export", "myProject", destination]));

        Assert.Contains(_hat.Lines, line => line.Contains("already exists"));
        Assert.Equal("keep me", File.ReadAllText(destination));
    }

    [Fact]
    public void GivenTheProjectNameInAnotherCaseThenExportFindsTheProject()
    {
        ProjectWithDatabase("myProject");
        var destination = _hat.ProjectDatabaseFile("exported");

        Assert.Equal(0, _hat.Hat(["export", "MYPROJECT", destination]));

        Assert.True(File.Exists(destination));
    }

    [Fact]
    public void GivenAFileThatIsNoDatabaseThenImportRefusesAndAddsNoProject()
    {
        var text = _hat.WriteFile("notes.hdb", "no database at all");

        Assert.NotEqual(0, _hat.Hat(["import", text, "copy"]));

        Assert.Contains(_hat.Lines, line => line.Contains("is no SortingHat project"));
        Assert.Equal(["* Default"], ListProjects());
        Assert.False(File.Exists(_hat.ProjectDatabaseFile("copy")));
    }

    [Fact]
    public void GivenTheMainDatabaseThenImportRefusesBecauseItIsNoProject()
    {
        ListProjects();

        Assert.NotEqual(0, _hat.Hat(["import", _hat.ProjectDatabaseFile("hat"), "copy"]));

        Assert.Contains(_hat.Lines, line => line.Contains("is no SortingHat project"));
        Assert.Equal(["* Default"], ListProjects());
    }

    [Fact]
    public void GivenAProjectOfANewerHatThenImportRefuses()
    {
        ProjectWithDatabase("myProject");
        var exported = Exported("myProject");
        Execute(exported, "INSERT INTO Revisions (Name, MigratedAt) VALUES ('29991231-0000-FromTheFuture.migration', datetime('now'))");

        Assert.NotEqual(0, _hat.Hat(["import", exported, "copy"]));

        Assert.Contains(_hat.Lines, line => line.Contains("newer"));
        Assert.Equal(["* Default", "  myProject"], ListProjects());
    }

    [Fact]
    public void GivenALeftOverDatabaseFileWithTheNewNameThenImportLeavesItAlone()
    {
        ProjectWithDatabase("myProject");
        var exported = Exported("myProject");
        var leftOver = _hat.ProjectDatabaseFile("copy");
        File.WriteAllText(leftOver, "left over");

        Assert.NotEqual(0, _hat.Hat(["import", exported, "copy"]));

        Assert.Contains(_hat.Lines, line => line.Contains("already a database file"));
        Assert.Equal("left over", File.ReadAllText(leftOver));
        Assert.Equal(["* Default", "  myProject"], ListProjects());
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        _hat.Dispose();
    }

    /// <summary>
    /// Switches the database to write-ahead logging and keeps a connection open,
    /// with a read transaction open, so hat's changes stay in the log instead of being written back into the database file.
    /// </summary>
    private static SqliteConnection WriteAheadLogKeptOpen(string databaseFile)
    {
        var connection = new SqliteConnection($"Data Source={databaseFile};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();

        // an open read transaction keeps its snapshot: no checkpoint may write newer changes back into the database file
        command.CommandText = "BEGIN; SELECT COUNT(*) FROM Tags;";
        command.ExecuteScalar();

        return connection;
    }

    private static void Execute(string databaseFile, string commandText)
    {
        using var connection = new SqliteConnection($"Data Source={databaseFile};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }

    private string Exported(string project)
    {
        var exported = _hat.ProjectDatabaseFile($"{project}-exported");

        Assert.Equal(0, _hat.Hat(["export", project, exported]));

        return exported;
    }

    /// <summary>A project database file only exists once the project has been used.</summary>
    private void ProjectWithDatabase(string project)
    {
        _hat.Hat(["new-project", project]);
        _hat.Hat(["switch-project", project]);
        _hat.TaggedFile($"{project}.txt", ":x");
        _hat.Hat(["switch-project", "Default"]);
    }

    private string[] ListProjects()
    {
        _hat.Hat(["list-projects"]);

        return [.. _hat.Lines];
    }
}
