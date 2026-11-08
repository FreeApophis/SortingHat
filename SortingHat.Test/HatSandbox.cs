using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apophis.CLI.Reader;
using Apophis.CLI.Writer;
using Autofac;
using Funcky.Monads;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.Plugin;
using SortingHat.CLI;
using static Funcky.Functional;

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
    private readonly Lazy<ILifetimeScope> _pluginScope;

    public HatSandbox()
        : this(_ => { })
    {
    }

    /// <summary>With <paramref name="register"/>, a test adds its own registrations, the way a plugin module does.</summary>
    public HatSandbox(Action<ContainerBuilder> register)
        : this(register, Option<string>.None)
    {
    }

    private HatSandbox(Action<ContainerBuilder> register, Option<string> pluginDirectory)
    {
        Directory.CreateDirectory(_root);
        PluginDirectory = pluginDirectory.GetOrElse(Path.Combine(_root, "plugins"));

        var compositionRoot = new CompositionRoot().Register();
        _databaseSettings = new DatabaseSettings { Type = "sqlite", Name = "hat", Path = _root, DefaultProject = "Default" };
        compositionRoot.Builder.RegisterInstance(_writer).As<IConsoleWriter>();
        compositionRoot.Builder.RegisterInstance(_reader).As<IConsoleReader>();
        compositionRoot.Builder.RegisterInstance(_databaseSettings);
        compositionRoot.Builder.RegisterInstance(new PluginDirectory(PluginDirectory));
        register(compositionRoot.Builder);
        _container = compositionRoot.Build();

        // like Program.Main, the plugins are loaded once, on the first run
        _pluginScope = new Lazy<ILifetimeScope>(() => CompositionRoot.BeginPluginScope(_container));
    }

    /// <summary>
    /// The plugins folder of this hat: empty and writable in the temporary folder, so a test can lay out plugin folders before the first run.
    /// Real plugins can't be deleted after loading, they come from <see cref="WithPluginFixtures(string)"/>.
    /// </summary>
    public string PluginDirectory { get; }

    public IReadOnlyList<string> Lines => _writer.Lines;

    /// <summary>The lines the user types when hat asks for input.</summary>
    public MemoryConsoleReader Keyboard => _reader;

    /// <summary>A hat whose plugins folder is the <paramref name="fixture"/> folder of PluginFixtures, see the project file.</summary>
    public static HatSandbox WithPluginFixtures(string fixture)
        => WithPluginFixtures(fixture, NoOperation);

    /// <summary>Like <see cref="WithPluginFixtures(string)"/>, with own registrations such as another <see cref="ContractAssemblies"/>.</summary>
    public static HatSandbox WithPluginFixtures(string fixture, Action<ContainerBuilder> register)
        => new(register, Path.Combine(AppContext.BaseDirectory, "PluginFixtures", fixture));

    public string ProjectDatabaseFile(string project)
        => Path.Combine(_databaseSettings.DbPath, $"{project}.db");

    public void Dispose()
    {
        if (_pluginScope.IsValueCreated)
        {
            _pluginScope.Value.Dispose();
        }

        _container.Dispose();

        // the connection pool keeps the database files open
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    public string TaggedFile(string name, params string[] tags)
    {
        // the content is unique per file, so every file has its own hash
        var path = WriteFile(name, name);

        Hat(["tag-files", .. tags, path]);

        return path;
    }

    /// <summary>Writes a file into the sandbox without telling hat about it; <see cref="FindFiles"/> knows it.</summary>
    public string WriteFile(string name, string content)
    {
        var path = Path.Combine(_root, name);

        File.WriteAllText(path, content);
        _files.Add(path);

        return path;
    }

    /// <summary>Writes a file with binary <paramref name="content"/> into the sandbox without telling hat about it.</summary>
    public string WriteFile(string name, byte[] content)
    {
        var path = Path.Combine(_root, name);

        File.WriteAllBytes(path, content);
        _files.Add(path);

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
        using var scope = _pluginScope.Value.BeginLifetimeScope();

        return run(scope);
    }
}
