using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using Funcky.Extensions;
using Funcky.Monads;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.Plugin;
using SortingHat.CLI;
using Spectre.Console;
using Spectre.Console.Testing;
using static Funcky.Functional;

namespace SortingHat.Test;

/// <summary>
/// The real command line application, with a real SQLite database and real files in a temporary folder.
/// </summary>
internal sealed class HatSandbox : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"SortingHat.Test.{Guid.NewGuid():N}");
    private readonly List<string> _files = [];
    private readonly TestConsole _console = new();
    private readonly DatabaseSettings _databaseSettings;
    private readonly IContainer _container;
    private readonly Lazy<ILifetimeScope> _pluginScope;
    private int _outputStart;

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

        // like hat with its output redirected: no colours, not interactive, no wrapping
        _console.Profile.Width = HatConsole.RedirectedWidth;
        compositionRoot.Builder.RegisterInstance(_console).As<IAnsiConsole>();
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

    /// <summary>The lines hat wrote in the last run.</summary>
    public IReadOnlyList<string> Lines
        => _console.Output[_outputStart..] switch
        {
            "" => [],
            var output => [.. output.TrimEnd('\n').Split('\n').Select(line => line.TrimEnd('\r'))],
        };

    /// <summary>
    /// The rows of the tables hat wrote in the last run, headers included, with their cells joined by '|', such as ':tax|1'.
    /// The cells of a table are at least three spaces apart.
    /// </summary>
    public IReadOnlyList<string> Rows
        => [.. Lines.Select(line => Regex.Split(line.Trim(), " {3,}")).Where(cells => cells.Length > 1).Select(cells => string.Join('|', cells))];

    /// <summary>The <paramref name="lines"/> the user types, each with Enter, when hat asks; hat runs in an interactive terminal from now on.</summary>
    public void Type(params IEnumerable<string> lines)
    {
        _console.Profile.Capabilities.Interactive = true;
        lines.ForEach(_console.Input.PushTextWithEnter);
    }

    /// <summary>The <paramref name="keys"/> the user presses when hat asks; hat runs in an interactive terminal from now on.</summary>
    public void Press(params IEnumerable<ConsoleKey> keys)
    {
        _console.Profile.Capabilities.Interactive = true;
        keys.ForEach(_console.Input.PushKey);
    }

    /// <summary>hat runs in a terminal that can redraw its output, such as a progress bar: interactive, with ANSI sequences.</summary>
    public void Terminal()
    {
        _console.Profile.Capabilities.Interactive = true;
        _console.EmitAnsiSequences = true;
    }

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
        _console.Dispose();

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
    /// <remarks>The <paramref name="name"/> can contain folders, such as 'scans/2018/a.pdf'; they are created.</remarks>
    public string WriteFile(string name, string content)
    {
        var path = Path.GetFullPath(Path.Combine(_root, name));

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? _root);
        File.WriteAllText(path, content);
        _files.Add(path);

        return path;
    }

    /// <summary>Creates the folder <paramref name="name"/> in the sandbox and returns its full path.</summary>
    public string CreateFolder(string name)
        => Directory.CreateDirectory(Path.Combine(_root, name)).FullName;

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
            .Where(path => Lines.Any(line => line.TrimEnd().EndsWith(path)))
            .Select(path => Path.GetFileName(path))
            .Order();
    }

    /// <summary>Runs hat with the <paramref name="arguments"/> and returns its exit code.</summary>
    public int Hat(string[] arguments)
    {
        _outputStart = _console.Output.Length;

        return InScope(scope => scope.Resolve<Application>().Run(arguments));
    }

    /// <summary>Runs <paramref name="run"/> in its own lifetime scope, like one invocation of hat, so it gets its own database connection.</summary>
    public TResult InScope<TResult>(Func<ILifetimeScope, TResult> run)
    {
        using var scope = _pluginScope.Value.BeginLifetimeScope();

        return run(scope);
    }
}
