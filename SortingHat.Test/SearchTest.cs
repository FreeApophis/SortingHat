using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Writer;
using Autofac;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.CLI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Searches through the real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
public sealed class SearchTest : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"SortingHat.Test.{Guid.NewGuid():N}");
    private readonly List<string> _files = [];
    private readonly MemoryConsoleWriter _writer = new();
    private readonly IContainer _container;

    public SearchTest()
    {
        Directory.CreateDirectory(_root);

        var compositionRoot = new CompositionRoot().Register();
        compositionRoot.Builder.RegisterInstance(_writer).As<IConsoleWriter>();
        compositionRoot.Builder.RegisterInstance(new DatabaseSettings { Type = "sqlite", Name = "hat", Path = _root, DefaultProject = "Default" });
        _container = compositionRoot.Build();
    }

    [Fact]
    public void GivenATagThenTheFilesWithThatTagAreFound()
    {
        TaggedFile("a.txt", ":x");
        TaggedFile("b.txt", ":y");

        Assert.Equal(["a.txt"], FindFiles(":x"));
    }

    [Fact]
    public void GivenOrThenTheFilesWithEitherTagAreFound()
    {
        TaggedFile("a.txt", ":x");
        TaggedFile("b.txt", ":y");
        TaggedFile("c.txt", ":z");

        Assert.Equal(["a.txt", "b.txt"], FindFiles(":x or :y"));
    }

    [Fact]
    public void GivenAndThenOnlyTheFilesWithBothTagsAreFound()
    {
        TaggedFile("a.txt", ":x", ":y");
        TaggedFile("b.txt", ":x");

        Assert.Equal(["a.txt"], FindFiles(":x and :y"));
    }

    [Fact]
    public void GivenAndNotThenTheFilesWithTheExcludedTagAreNotFound()
    {
        TaggedFile("a.txt", ":x", ":y");
        TaggedFile("b.txt", ":x");

        Assert.Equal(["b.txt"], FindFiles(":x and not :y"));
    }

    [Fact]
    public void GivenNotThenFilesWithoutAnyTagAreFound()
    {
        TaggedFile("a.txt", ":x", ":y");
        TaggedFile("b.txt", ":x");
        TaggedFile("c.txt");

        Assert.Equal(["b.txt", "c.txt"], FindFiles("not :y"));
    }

    [Fact]
    public void GivenAParentTagThenTheFilesTaggedWithItsChildTagsAreFound()
    {
        TaggedFile("a.txt", ":photo:holiday:2018");
        TaggedFile("b.txt", ":photo");
        TaggedFile("c.txt", ":other");

        Assert.Equal(["a.txt", "b.txt"], FindFiles(":photo"));
    }

    [Fact]
    public void GivenAChildTagThenTheFilesTaggedWithItsParentTagAreNotFound()
    {
        TaggedFile("a.txt", ":photo:holiday");
        TaggedFile("b.txt", ":photo");

        Assert.Equal(["a.txt"], FindFiles(":photo:holiday"));
    }

    [Fact]
    public void GivenNotAParentTagThenTheFilesTaggedWithItsChildTagsAreNotFound()
    {
        TaggedFile("a.txt", ":photo:holiday");
        TaggedFile("b.txt", ":other");

        Assert.Equal(["b.txt"], FindFiles("not :photo"));
    }

    [Fact]
    public void GivenAnUnknownTagThenNoFilesAreFoundAndTheTagIsReported()
    {
        TaggedFile("a.txt", ":x");

        Assert.Empty(FindFiles(":typo"));
        Assert.Contains("Unknown tag: :typo", _writer.Lines);
    }

    public void Dispose()
    {
        _container.Dispose();

        // the connection pool keeps the database files open
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    private void TaggedFile(string name, params string[] tags)
    {
        var path = Path.Combine(_root, name);

        // the content is unique per file, so every file has its own hash
        File.WriteAllText(path, name);
        _files.Add(path);

        Hat(["tag-files", .. tags, path]);
    }

    private IEnumerable<string> FindFiles(string query)
    {
        _writer.Lines.Clear();

        Hat(["find-files", query]);

        return _files
            .Where(path => _writer.Lines.Any(line => line.TrimEnd().EndsWith(path)))
            .Select(path => Path.GetFileName(path))
            .Order();
    }

    private void Hat(string[] arguments)
    {
        using var scope = _container.BeginLifetimeScope();

        scope.Resolve<ArgumentParser>().Execute(arguments);
    }
}
