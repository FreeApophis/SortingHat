using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Funcky.Extensions;
using Funcky.Monads;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.App;
using SortingHat.Desktop.Core;
using SortingHat.Desktop.Shell;
using SortingHat.Desktop.Views;
using Xunit;

namespace SortingHat.Desktop.Test;

/// <summary>
/// The real desktop app in a headless window: the real composition root with a real SQLite database and real files in a temporary folder.
/// Like HatSandbox for hat; a test adds its own registrations the way a plugin module does.
/// The window opens on first use, so a test can prepare the database first.
/// </summary>
internal sealed class DesktopSandbox : IAsyncDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"SortingHat.Desktop.Test.{Guid.NewGuid():N}");
    private readonly IContainer _container;
    private readonly ILifetimeScope _pluginScope;
    private readonly Lazy<MainWindow> _window;

    public DesktopSandbox()
        : this(_ => { })
    {
    }

    public DesktopSandbox(Action<ContainerBuilder> register)
    {
        Directory.CreateDirectory(_root);

        var compositionRoot = new DesktopCompositionRoot().Register();
        compositionRoot.Builder.RegisterInstance(new DatabaseSettings { Type = "sqlite", Name = "hat", Path = _root, DefaultProject = "Default" });

        // the user pauses typing at once
        compositionRoot.Builder.RegisterInstance(new SearchPause(TimeSpan.Zero));
        register(compositionRoot.Builder);
        _container = compositionRoot.Build();
        _pluginScope = PluginScope.Begin(_container);
        _window = new Lazy<MainWindow>(Open);
    }

    public MainWindow Window => _window.Value;

    /// <summary>Writes a file into the sandbox and tags it, like 'hat tag-files'; returns its full path.</summary>
    public async Task<string> TaggedFile(string name, params string[] tags)
    {
        var path = Path.Combine(_root, name);
        await System.IO.File.WriteAllTextAsync(path, name);

        await using var scope = _pluginScope.BeginLifetimeScope();
        var file = (await scope.Resolve<IFileScanner>().Examine(path)).Current;
        var fileStore = scope.Resolve<IFile>();
        fileStore.Store(file);
        tags.Select(tag => Assert.Right(Tag.Parse(tag))).ForEach(tag => fileStore.Tag(file, tag));

        return path;
    }

    /// <summary>The user types <paramref name="text"/> into the query bar.</summary>
    public Task Type(string text)
    {
        Control<TextBox>("QueryBox").Focus();
        Window.KeyTextInput(text);

        return Idle();
    }

    /// <summary>The user selects the result at <paramref name="index"/>.</summary>
    public Task SelectResult(int index)
    {
        Control<ListBox>("Results").SelectedIndex = index;

        return Idle();
    }

    /// <summary>The user clicks the button named <paramref name="name"/>.</summary>
    public Task Click(string name)
    {
        Control<Button>(name).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        return Idle();
    }

    /// <summary>The user clicks the tag <paramref name="tag"/> in the tag tree.</summary>
    public async Task ChooseTag(string tag)
    {
        await Idle();
        Control<TreeView>("TagTree").SelectedItem = TreeItem(tag).DataContext;
        await Idle();
    }

    /// <summary>The user opens the context menu of the tag <paramref name="tag"/> in the tag tree and clicks <paramref name="header"/>.</summary>
    public async Task TagMenu(string tag, string header)
    {
        await Idle();
        var item = TreeItem(tag);
        var target = Assert.Single(item.GetVisualDescendants().OfType<Control>(), control => control.ContextMenu is not null && control.DataContext == item.DataContext);
        var menu = Assert.Some(Option.FromNullable(target.ContextMenu));
        menu.Open(target);
        Assert.Single(menu.Items.OfType<MenuItem>(), item => Equals(item.Header, header)).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        menu.Close();
        await Idle();
    }

    /// <summary>The user replaces the text of the text box named <paramref name="name"/>.</summary>
    public Task Enter(string name, string text)
    {
        Control<TextBox>(name).Text = text;

        return Idle();
    }

    /// <summary>The texts the window shows in the control named <paramref name="name"/>, in order.</summary>
    public IReadOnlyList<string> Texts(string name)
        => [.. Control<Control>(name).GetSelfAndVisualDescendants().OfType<TextBlock>().Where(text => text.IsEffectivelyVisible).Select(text => text.Text ?? string.Empty)];

    /// <summary>Waits until every effect has run and the window shows its result.</summary>
    public async Task Idle()
    {
        var window = Window;
        await _pluginScope.Resolve<Store>().Idle();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    /// <summary>Waits for the effects still running, they use the database.</summary>
    public async ValueTask DisposeAsync()
    {
        await _pluginScope.Resolve<Store>().Idle();

        if (_window.IsValueCreated)
        {
            Window.Close();
        }

        await _pluginScope.DisposeAsync();
        await _container.DisposeAsync();

        // the connection pool keeps the database files open
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    private MainWindow Open()
    {
        var window = _pluginScope.Resolve<MainWindow>();
        window.Show();

        return window;
    }

    /// <summary>The item of the tag tree showing the tag <paramref name="tag"/>, such as ':tax:2018'.</summary>
    private TreeViewItem TreeItem(string tag)
        => Assert.Single(Control<TreeView>("TagTree").GetVisualDescendants().OfType<TreeViewItem>(), item => item.DataContext is TagTreeNode node && node.Tag.FullName == tag);

    private T Control<T>(string name)
        where T : Control
    {
        Dispatcher.UIThread.RunJobs();
        Window.UpdateLayout();

        return Assert.Some(Option.FromNullable(Window.FindControl<T>(name)));
    }
}
