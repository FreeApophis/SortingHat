using Apophis.CLI.Writer;
using Autofac;
using SortingHat.API.DI;
using SortingHat.App;
using SortingHat.Desktop.Shell;
using SortingHat.Desktop.Views;

namespace SortingHat.Desktop;

/// <summary>The object graph of the desktop app: the shared host and what is the window's own.</summary>
internal sealed class DesktopCompositionRoot
{
    public ContainerBuilder Builder { get; } = new();

    public DesktopCompositionRoot Register()
    {
        Builder.RegisterModule<HostModule>();

        Builder.RegisterType<MessagePanelWriter>().As<IConsoleWriter>().InstancePerMatchingLifetimeScope(PluginScope.Tag);
        Builder.RegisterType<NoUserPrompt>().As<IUserPrompt>().SingleInstance();

        // one store per window, in the scope of the plugins, so its effects see the plugins' registrations
        Builder.RegisterType<EffectRunner>().AsSelf().InstancePerMatchingLifetimeScope(PluginScope.Tag);
        Builder.RegisterType<Store>().AsSelf().InstancePerMatchingLifetimeScope(PluginScope.Tag);
        Builder.RegisterType<MainViewModel>().AsSelf().InstancePerMatchingLifetimeScope(PluginScope.Tag);
        Builder.Register(context => MainWindow(context.Resolve<MainViewModel>())).AsSelf();

        return this;
    }

    public IContainer Build()
        => Builder.Build();

    private static MainWindow MainWindow(MainViewModel viewModel)
    {
        viewModel.Start();

        return new MainWindow { DataContext = viewModel };
    }
}
