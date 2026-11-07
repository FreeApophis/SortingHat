using System;
using System.Linq;
using Autofac;
using Microsoft.Data.Sqlite;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Tags ignore case and keep their spelling, like file names on Windows: ':Tax' and ':tax' are one tag.
/// Through the real command line application, with a real SQLite database in a temporary folder.
/// </summary>
public sealed class TagCaseTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Theory]
    [InlineData(":TAX:2018")]
    [InlineData(":tax:2018")]
    [InlineData(":Tax")]
    public void GivenATagInAnotherCaseThenASearchFindsItsFiles(string query)
    {
        _hat.TaggedFile("a.txt", ":Tax:2018");

        Assert.Equal(["a.txt"], _hat.FindFiles(query));
    }

    [Fact]
    public void GivenATagInAnotherCaseThenTaggingUsesTheExistingTagWithItsSpelling()
    {
        _hat.TaggedFile("a.txt", ":Tax");
        _hat.TaggedFile("b.txt", ":TAX");

        Assert.Equal(["* :Tax"], TagLines());
        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":tax"));
    }

    [Fact]
    public void GivenANameBeyondAsciiInAnotherCaseThenItIsTheSameTag()
    {
        _hat.TaggedFile("a.txt", ":Müller");

        Assert.Equal(["a.txt"], _hat.FindFiles(":MÜLLER"));
    }

    [Fact]
    public void GivenANewSpellingOfTheSameNameThenRenameTagChangesOnlyTheSpelling()
    {
        _hat.TaggedFile("a.txt", ":tax");

        Assert.Equal(0, _hat.Hat(["rename-tag", ":TAX", "Tax"]));

        Assert.Equal(["* :Tax"], TagLines());
    }

    [Fact]
    public void GivenOlderTagsThatDifferOnlyInCaseThenTheyAreMergedIntoTheOldestWithTheirFilesAndChildren()
    {
        _hat.TaggedFile("a.txt", ":tax:Q1");
        _hat.TaggedFile("b.txt", ":Older-x:q1");
        _hat.TaggedFile("c.txt", ":Older-x");
        Rename("Older-x", "TAX");

        Convert();

        Assert.Equal(["* :tax", "* :tax:Q1"], TagLines());
        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":tax:q1"));
        Assert.Equal(["a.txt", "b.txt", "c.txt"], _hat.FindFiles(":TAX"));
    }

    public void Dispose()
        => _hat.Dispose();

    private string[] TagLines()
    {
        _hat.Hat(["list-tags"]);

        return [.. _hat.Lines.Where(line => line.StartsWith("* ")).Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2)).Select(parts => string.Join(' ', parts))];
    }

    /// <summary>Older hat versions kept tags that differ only in case apart; the name changes, its key stays as it was.</summary>
    private void Rename(string name, string olderName)
        => _hat.InScope(scope =>
        {
            scope.Resolve<SQLiteProjectDatabase>().ExecuteNonQuery("UPDATE Tags SET Name = @olderName WHERE Name = @name", new SqliteParameter("@name", name), new SqliteParameter("@olderName", olderName));

            return true;
        });

    private void Convert()
        => _hat.InScope(scope =>
        {
            var db = scope.Resolve<SQLiteProjectDatabase>();

            return db.InTransaction(() =>
            {
                new TagNameKeyMigration().Run(db);

                return true;
            });
        });
}
