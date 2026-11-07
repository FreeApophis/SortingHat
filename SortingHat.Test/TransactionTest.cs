using System;
using Autofac;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Transactions on a real SQLite database in a temporary folder.
/// </summary>
public sealed class TransactionTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenWorkThatSucceedsThenItsChangesAreCommitted()
    {
        _hat.InScope(scope => scope.Resolve<SQLiteProjectDatabase>().InTransaction(() => InsertTag(scope, "x")));

        Assert.Equal(1L, TagCount());
    }

    [Fact]
    public void GivenWorkThatFailsThenItsChangesAreRolledBack()
    {
        _hat.InScope(scope => Assert.Throws<InvalidOperationException>(() => scope.Resolve<SQLiteProjectDatabase>().InTransaction(() => Fail(InsertTag(scope, "x")))));

        Assert.Equal(0L, TagCount());
    }

    [Fact]
    public void GivenANestedTransactionThenItIsRolledBackWithTheOuterTransaction()
    {
        _hat.InScope(scope =>
        {
            var db = scope.Resolve<SQLiteProjectDatabase>();

            return Assert.Throws<InvalidOperationException>(() => db.InTransaction(() => Fail(db.InTransaction(() => InsertTag(scope, "x")))));
        });

        Assert.Equal(0L, TagCount());
    }

    public void Dispose()
        => _hat.Dispose();

    private static bool InsertTag(ILifetimeScope scope, string name)
    {
        scope.Resolve<SQLiteProjectDatabase>().ExecuteNonQuery("INSERT INTO Tags (Name, NameKey) VALUES (@name, upper(@name))", new Microsoft.Data.Sqlite.SqliteParameter("@name", name));

        return true;
    }

    private static bool Fail(bool ignored)
        => throw new InvalidOperationException("The work failed.");

    private object? TagCount()
        => _hat.InScope(scope => scope.Resolve<SQLiteProjectDatabase>().ExecuteScalar("SELECT COUNT(*) FROM Tags"));
}
