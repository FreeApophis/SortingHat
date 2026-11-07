using System;
using Autofac;
using Microsoft.Data.Sqlite;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Tagging and searching share one grammar: every tag hat can create can be searched. Tag names of older databases are converted to it once.
/// Through the real command line application, with a real SQLite database in a temporary folder.
/// </summary>
public sealed class TagGrammarTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Theory]
    [InlineData(":v1.2")]
    [InlineData(":C++:snake_case")]
    [InlineData(":Steuererklärung:2018-03")]
    [InlineData(":Camera:NIKON-CORPORATION")]
    public void GivenATagHatAcceptsThenASearchFindsIt(string tag)
    {
        _hat.TaggedFile("a.txt", tag);

        Assert.Equal(["a.txt"], _hat.FindFiles(tag));
    }

    [Fact]
    public void GivenAnOlderTagNameWithSpacesThenItIsConvertedAndCanBeSearched()
    {
        _hat.TaggedFile("a.txt", ":Camera:NIKON-x");
        Rename("NIKON-x", "NIKON CORPORATION");

        Convert();

        Assert.Equal(["a.txt"], _hat.FindFiles(":Camera:NIKON-CORPORATION"));
    }

    [Fact]
    public void GivenAnOlderTagNameThatBecomesTheNameOfASiblingThenBothAreMergedWithTheirFilesAndChildren()
    {
        _hat.TaggedFile("a.txt", ":Camera:NIKON-x:D90");
        _hat.TaggedFile("b.txt", ":Camera:NIKON-CORPORATION:D90");
        _hat.TaggedFile("c.txt", ":Camera:NIKON-x");
        Rename("NIKON-x", "NIKON CORPORATION");

        Convert();

        Assert.Equal(["a.txt", "b.txt"], _hat.FindFiles(":Camera:NIKON-CORPORATION:D90"));
        Assert.Equal(["a.txt", "b.txt", "c.txt"], _hat.FindFiles(":Camera:NIKON-CORPORATION"));
        Assert.Equal(0, _hat.Hat(["list-tags"]));
        Assert.Single(_hat.Lines, line => line.Contains(":Camera:NIKON-CORPORATION:D90"));
    }

    [Fact]
    public void GivenAnOlderTagNameWithoutAnyCharacterOfATagNameThenItIsCalledUnnamed()
    {
        _hat.TaggedFile("a.txt", ":x:y");
        Rename("y", "&&");

        Convert();

        Assert.Equal(["a.txt"], _hat.FindFiles(":x:unnamed"));
    }

    public void Dispose()
        => _hat.Dispose();

    /// <summary>Older hat versions stored any name, as if the tag had been created before the grammar.</summary>
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

            // an older database runs both migrations, in their order
            return db.InTransaction(() =>
            {
                new TagNameGrammarMigration().Run(db);
                new TagNameKeyMigration().Run(db);

                return true;
            });
        });
}
