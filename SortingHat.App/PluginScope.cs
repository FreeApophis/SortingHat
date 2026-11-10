using Autofac;
using SortingHat.API.Plugin;

namespace SortingHat.App;

/// <summary>The scope a host runs in: its container with every plugin's modules registered.</summary>
public static class PluginScope
{
    /// <summary>Plugins take part in every extension point of the scope exactly like the registrations of the <paramref name="container"/>.</summary>
    public static ILifetimeScope Begin(IContainer container)
        => container.BeginLifetimeScope(builder =>
        {
            builder.RegisterModule<LoggerModule>();
            container.Resolve<IPluginLoader>().RegisterModules(builder);
        });
}
