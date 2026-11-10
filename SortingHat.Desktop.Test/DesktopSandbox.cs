using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.App;
using SortingHat.Desktop.Shell;
using SortingHat.Desktop.Views;

namespace SortingHat.Desktop.Test;

/// <summary>
/// The real desktop app in a headless window: the real composition root with a real SQLite database in a temporary folder.
/// Like HatSandbox for hat; a test adds its own registrations the way a plugin module does.
/// </summary>
internal sealed class DesktopSandbox : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"SortingHat.Desktop.Test.{Guid.NewGuid():N}");
    private readonly IContainer _container;
    private readonly ILifetimeScope _pluginScope;

    public DesktopSandbox()
        : this(_ => { })
    {
    }

    public DesktopSandbox(Action<ContainerBuilder> register)
    {
        Directory.CreateDirectory(_root);

        var compositionRoot = new DesktopCompositionRoot().Register();
        compositionRoot.Builder.RegisterInstance(new DatabaseSettings { Type = "sqlite", Name = "hat", Path = _root, DefaultProject = "Default" });
        register(compositionRoot.Builder);
        _container = compositionRoot.Build();
        _pluginScope = PluginScope.Begin(_container);

        Window = _pluginScope.Resolve<MainWindow>();
        Window.Show();
    }

    public MainWindow Window { get; }

    /// <summary>The texts the window shows in the control named <paramref name="name"/>, in order.</summary>
    public IReadOnlyList<string> Texts(string name)
    {
        Dispatcher.UIThread.RunJobs();

        return
        [
            .. Window.GetVisualDescendants()
                .OfType<Control>()
                .Where(control => control.Name == name)
                .SelectMany(control => control.GetVisualDescendants().OfType<TextBlock>())
                .Select(text => text.Text ?? string.Empty),
        ];
    }

    /// <summary>Waits until every effect has run and the window shows its result.</summary>
    public async Task Idle()
    {
        await _pluginScope.Resolve<Store>().Idle();
        Dispatcher.UIThread.RunJobs();
    }

    public void Dispose()
    {
        Window.Close();
        _pluginScope.Dispose();
        _container.Dispose();

        // the connection pool keeps the database files open
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }
}
