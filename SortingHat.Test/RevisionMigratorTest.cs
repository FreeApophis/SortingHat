using System;
using Autofac;
using Microsoft.Data.Sqlite;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Runs migrations, SQL scripts and migrations in C#, on a real SQLite database in a temporary folder.
/// </summary>
public sealed class RevisionMigratorTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAMigrationThatFailsHalfwayThenNothingOfItIsApplied()
    {
        _hat.InScope(scope =>
        {
            var migrator = new RevisionMigrator(Serilog.Core.Logger.None, typeof(RevisionMigratorTest).Assembly, []);

            return Assert.Throws<SqliteException>(() => migrator.Migrate(scope.Resolve<SQLiteProjectDatabase>()));
        });

        Assert.Equal(0L, Count("SELECT COUNT(*) FROM sqlite_master WHERE name = 'Broken'"));
        Assert.Equal(0L, Count("SELECT COUNT(*) FROM Revisions WHERE Name LIKE '%FailsHalfway%'"));
    }

    [Fact]
    public void GivenACodeMigrationThenItRunsOnceAndIsRecorded()
    {
        var migration = new CodeMigration("29990101-0000-AddsARow", MigrationType.Project, db => db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Counted (Id INTEGER); INSERT INTO Counted (Id) VALUES (1);"));

        Migrate(migration);
        Migrate(migration);

        Assert.Equal(1L, Count("SELECT COUNT(*) FROM Counted"));
        Assert.Equal(1L, Count("SELECT COUNT(*) FROM Revisions WHERE Name = '29990101-0000-AddsARow'"));
    }

    [Fact]
    public void GivenACodeMigrationThatFailsHalfwayThenNothingOfItIsApplied()
    {
        var migration = new CodeMigration("29990101-0000-FailsHalfway", MigrationType.Project, db =>
        {
            db.ExecuteNonQuery("CREATE TABLE BrokenByCode (Id INTEGER)");
            throw new InvalidOperationException("fails halfway");
        });

        Assert.Throws<InvalidOperationException>(() => Migrate(migration));

        Assert.Equal(0L, Count("SELECT COUNT(*) FROM sqlite_master WHERE name = 'BrokenByCode'"));
        Assert.Equal(0L, Count("SELECT COUNT(*) FROM Revisions WHERE Name = '29990101-0000-FailsHalfway'"));
    }

    [Fact]
    public void GivenACodeMigrationForTheMainDatabaseThenTheProjectDatabaseSkipsIt()
    {
        Migrate(new CodeMigration("29990101-0000-MainOnly", MigrationType.Main, db => db.ExecuteNonQuery("CREATE TABLE MainOnly (Id INTEGER)")));

        Assert.Equal(0L, Count("SELECT COUNT(*) FROM sqlite_master WHERE name = 'MainOnly'"));
    }

    public void Dispose()
        => _hat.Dispose();

    /// <summary>The real migrations of hat, which already ran in the sandbox, and the code migration of the test.</summary>
    private void Migrate(ICodeMigration migration)
        => _hat.InScope(scope =>
        {
            new RevisionMigrator(Serilog.Core.Logger.None, typeof(RevisionMigrator).Assembly, [migration]).Migrate(scope.Resolve<SQLiteProjectDatabase>());

            return true;
        });

    private object? Count(string query)
        => _hat.InScope(scope => scope.Resolve<SQLiteProjectDatabase>().ExecuteScalar(query));
}
