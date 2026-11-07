using System;
using System.IO;
using Autofac;
using SortingHat.DB;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Files that change after hat has hashed them, through the real command line application with a real SQLite database and real files.
/// hat prints "Hashing: '...'" whenever it reads a file to hash it, so the tests see whether it trusted the stored hash.
/// </summary>
public sealed class StaleHashTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAnUnchangedFileThenTaggingItAgainTrustsTheStoredHash()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);

        _hat.Hat(["tag-files", ":y", path]);

        Assert.DoesNotContain(_hat.Lines, IsHashing);
    }

    [Fact]
    public void GivenAnEditedFileThenNewTagsGoToTheNewContent()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        _hat.Hat(["tag-files", ":y", path]);
        _hat.Hat(["untag-files", ":y", path]);

        Assert.Empty(_hat.FindFiles(":y"));
    }

    [Fact]
    public void GivenAnEditedFileThenItsPathBelongsOnlyToTheNewContent()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        _hat.Hat(["tag-files", path]);

        Assert.Equal(1L, Scalar("SELECT COUNT(*) FROM FilePaths"));
    }

    [Fact]
    public void GivenAnEditThatKeepsTheSizeButNotTheTimestampThenTheFileIsHashedAgain()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "CONTENT");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(-30));

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Contains(_hat.Lines, IsHashing);
    }

    [Fact]
    public void GivenAFileWrittenRightBeforeItWasHashedThenItIsHashedAgainNextTime()
    {
        var path = _hat.WriteFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Contains(_hat.Lines, IsHashing);
    }

    [Fact]
    public void GivenAPathWithoutFingerprintThenItIsHashedAgain()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        Execute("UPDATE FilePaths SET Size = NULL, ModifiedAt = NULL, VerifiedAt = NULL");

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Contains(_hat.Lines, IsHashing);
    }

    [Fact]
    public void GivenAnEditedFileThenItKeepsItsTags()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        _hat.Hat(["tag-files", path]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenAnEditedFileTaggedAgainThenItHasItsOldAndItsNewTags()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Equal(["a.txt"], _hat.FindFiles(":x and :y"));
    }

    [Fact]
    public void GivenAnEditedFileThenTaggingReportsTheChange()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.WriteAllText(path, "edited and longer content");

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Contains(_hat.Lines, line => line.Contains(path) && line.Contains("has changed"));
    }

    [Fact]
    public void GivenAFileWithANewTimestampButTheSameContentThenNoChangeIsReported()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(-30));

        _hat.Hat(["tag-files", ":y", path]);

        Assert.Contains(_hat.Lines, IsHashing);
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("has changed"));
    }

    [Fact]
    public void GivenAnEditThatKeepsSizeAndTimestampThenRepairRehashFindsIt()
    {
        var path = HiddenEdit("a.txt", ":x");

        Assert.Equal(0, _hat.Hat(["repair", "--rehash"]));
        Assert.Contains(_hat.Lines, line => line.Contains(path) && line.Contains("has changed"));

        _hat.Hat(["tag-files", ":y", path]);
        _hat.Hat(["untag-files", ":y", path]);
        Assert.Empty(_hat.FindFiles(":y"));
        Assert.Equal(["a.txt"], _hat.FindFiles(":x"));
    }

    [Fact]
    public void GivenADryRunThenRepairRehashReportsTheChangeButKeepsTheOldHash()
    {
        var path = HiddenEdit("a.txt", ":x");

        Assert.Equal(0, _hat.Hat(["repair", "--rehash", "--dry-run"]));
        Assert.Contains(_hat.Lines, line => line.Contains(path) && line.Contains("has changed"));

        _hat.Hat(["repair", "--rehash"]);
        Assert.Contains(_hat.Lines, line => line.Contains(path) && line.Contains("has changed"));
    }

    [Fact]
    public void GivenUnchangedFilesThenRepairRehashReportsNoChange()
    {
        var path = OldFile("a.txt", "content");
        _hat.Hat(["tag-files", ":x", path]);

        Assert.Equal(0, _hat.Hat(["repair", "--rehash"]));
        Assert.DoesNotContain(_hat.Lines, line => line.Contains("has changed"));
    }

    [Fact]
    public void GivenTheHelpOfRepairThenItOffersToRehash()
    {
        Assert.Equal(0, _hat.Hat(["repair", "--help"]));
        Assert.Contains(_hat.Lines, line => line.Contains("--rehash"));
    }

    public void Dispose()
        => _hat.Dispose();

    /// <summary>A tagged file whose content changes while its size and timestamp stay, like with tools that keep the date.</summary>
    private string HiddenEdit(string name, string tag)
    {
        var path = OldFile(name, "content");
        _hat.Hat(["tag-files", tag, path]);

        var timestamp = File.GetLastWriteTimeUtc(path);
        File.WriteAllText(path, "CONTENT");
        File.SetLastWriteTimeUtc(path, timestamp);

        return path;
    }

    private static bool IsHashing(string line)
        => line.StartsWith("Hashing:");

    /// <summary>A file last written an hour ago, so its fingerprint is trustworthy right away.</summary>
    private string OldFile(string name, string content)
    {
        var path = _hat.WriteFile(name, content);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddHours(-1));

        return path;
    }

    private object? Scalar(string query)
        => _hat.InScope(scope => scope.Resolve<SQLiteProjectDatabase>().ExecuteScalar(query));

    private void Execute(string commandText)
        => _hat.InScope(scope =>
        {
            scope.Resolve<SQLiteProjectDatabase>().ExecuteNonQuery(commandText);

            return true;
        });
}
