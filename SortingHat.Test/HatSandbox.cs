using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Reader;
using Apophis.CLI.Writer;
using Autofac;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.CLI;

namespace SortingHat.Test;

/// <summary>
/// The real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
internal sealed class HatSandbox : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"SortingHat.Test.{Guid.NewGuid():N}");
    private readonly List<string> _files = [];
    private readonly MemoryConsoleWriter _writer = new();
    private readonly MemoryConsoleReader _reader = new();
    private readonly DatabaseSettings _databaseSettings;
    private readonly IContainer _container;

    public HatSandbox()
    {
        Directory.CreateDirectory(_root);

        var compositionRoot = new CompositionRoot().Register();
        _databaseSettings = new DatabaseSettings { Type = "sqlite", Name = "hat", Path = _root, DefaultProject = "Default" };
        compositionRoot.Builder.RegisterInstance(_writer).As<IConsoleWriter>();
        compositionRoot.Builder.RegisterInstance(_reader).As<IConsoleReader>();
        compositionRoot.Builder.RegisterInstance(_databaseSettings);
        _container = compositionRoot.Build();
    }

    public IReadOnlyList<string> Lines => _writer.Lines;

    /// <summary>The lines the user types when hat asks for input.</summary>
    public MemoryConsoleReader Keyboard => _reader;

    public string ProjectDatabaseFile(string project)
        => Path.Combine(_databaseSettings.DbPath, $"{project}.db");

    public void Dispose()
    {
        _container.Dispose();

        // the connection pool keeps the database files open
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    public string TaggedFile(string name, params string[] tags)
    {
        var path = Path.Combine(_root, name);

        // the content is unique per file, so every file has its own hash
        File.WriteAllText(path, name);
        _files.Add(path);

        Hat(["tag-files", .. tags, path]);

        return path;
    }

    public IEnumerable<string> FindFiles(string query)
    {
        Hat(["find-files", query]);

        return _files
            .Where(path => _writer.Lines.Any(line => line.TrimEnd().EndsWith(path)))
            .Select(path => Path.GetFileName(path))
            .Order();
    }

    /// <summary>Runs hat with the <paramref name="arguments"/> and returns its exit code.</summary>
    public int Hat(string[] arguments)
    {
        _writer.Lines.Clear();

        return InScope(scope => scope.Resolve<Application>().Run(arguments));
    }

    /// <summary>Runs <paramref name="run"/> in its own lifetime scope, like one invocation of hat, so it gets its own database connection.</summary>
    public TResult InScope<TResult>(Func<ILifetimeScope, TResult> run)
    {
        using var scope = _container.BeginLifetimeScope();

        return run(scope);
    }
}
