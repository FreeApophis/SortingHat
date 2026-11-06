using System;
using Autofac;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Tags files through the real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
public sealed class TagFilesTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenTaggingFailsHalfwayThenNothingOfItIsStored()
    {
        Execute("CREATE TRIGGER FailFileTags BEFORE INSERT ON FileTags BEGIN SELECT RAISE(ABORT, 'tagging fails halfway'); END;");

        Record.Exception(() => _hat.TaggedFile("a.txt", ":x"));

        Assert.Equal(0L, Count("Files"));
        Assert.Equal(0L, Count("FileNames"));
        Assert.Equal(0L, Count("FilePaths"));
        Assert.Equal(0L, Count("Tags"));
    }

    [Fact]
    public void GivenAFileTaggedTwiceThenItsRowsAreKept()
    {
        var path = _hat.TaggedFile("a.txt", ":x");
        var rows = Ids();

        _hat.Hat(["tag-files", ":x", path]);

        Assert.Equal(rows, Ids());
    }

    public void Dispose()
        => _hat.Dispose();

    private (object? FileTag, object? FileName, object? FilePath) Ids()
        => (Scalar("SELECT Id FROM FileTags"), Scalar("SELECT Id FROM FileNames"), Scalar("SELECT Id FROM FilePaths"));

    private object? Count(string table)
        => Scalar($"SELECT COUNT(*) FROM {table}");

    private object? Scalar(string query)
        => _hat.InScope(scope => scope.Resolve<SQLiteProjectDatabase>().ExecuteScalar(query));

    private void Execute(string commandText)
        => _hat.InScope(scope =>
        {
            scope.Resolve<SQLiteProjectDatabase>().ExecuteNonQuery(commandText);

            return true;
        });
}
