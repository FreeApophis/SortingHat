using System;
using Autofac;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Runs the migrations embedded in this test assembly on a real SQLite database in a temporary folder.
/// </summary>
public sealed class RevisionMigratorTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAMigrationThatFailsHalfwayThenNothingOfItIsApplied()
    {
        _hat.InScope(scope =>
        {
            var migrator = new RevisionMigrator(NullLogger<RevisionMigrator>.Instance, typeof(RevisionMigratorTest).Assembly);

            return Assert.Throws<SqliteException>(() => migrator.Migrate(scope.Resolve<SQLiteProjectDatabase>()));
        });

        Assert.Equal(0L, Count("SELECT COUNT(*) FROM sqlite_master WHERE name = 'Broken'"));
        Assert.Equal(0L, Count("SELECT COUNT(*) FROM Revisions WHERE Name LIKE '%FailsHalfway%'"));
    }

    public void Dispose()
        => _hat.Dispose();

    private object? Count(string query)
        => _hat.InScope(scope => scope.Resolve<SQLiteProjectDatabase>().ExecuteScalar(query));
}
