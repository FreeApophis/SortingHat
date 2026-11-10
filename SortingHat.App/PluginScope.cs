using Autofac;
using SortingHat.API.Plugin;

namespace SortingHat.App;

/// <summary>The scope a host runs in: its container with every plugin's modules registered.</summary>
public static class PluginScope
{
    /// <summary>The tag of the scope, for what a host keeps once per run with the plugins, such as the desktop app's window state.</summary>
    public static readonly object Tag = nameof(PluginScope);

    /// <summary>Plugins take part in every extension point of the scope exactly like the registrations of the <paramref name="container"/>.</summary>
    public static ILifetimeScope Begin(IContainer container)
        => container.BeginLifetimeScope(Tag, builder =>
        {
            builder.RegisterModule<LoggerModule>();
            container.Resolve<IPluginLoader>().RegisterModules(builder);
        });
}
