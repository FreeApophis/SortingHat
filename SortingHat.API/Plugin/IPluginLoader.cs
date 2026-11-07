using System.Collections.Generic;
using Autofac;

namespace SortingHat.API.Plugin;

public interface IPluginLoader
{
    List<IPlugin> Plugins { get; }

    /// <summary>What in the plugins folder was not loaded, and why.</summary>
    IReadOnlyList<PluginProblem> Problems { get; }

    void RegisterModules(ContainerBuilder builder);
}
