using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Funcky.Monads;

namespace SortingHat.Desktop;

/// <summary>The Avalonia application: the theme, and the main window from the composition root when there is one (the headless tests open their own windows).</summary>
public sealed class DesktopApp : Application
{
    private readonly Option<Func<Window>> _mainWindow;

    public DesktopApp()
        : this(Option<Func<Window>>.None)
    {
    }

    internal DesktopApp(Func<Window> mainWindow)
        : this(Option.Some(mainWindow))
    {
    }

    private DesktopApp(Option<Func<Window>> mainWindow)
        => _mainWindow = mainWindow;

    public override void Initialize()
        => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow.AndThen(create => desktop.MainWindow = create());
        }

        base.OnFrameworkInitializationCompleted();
    }
}
