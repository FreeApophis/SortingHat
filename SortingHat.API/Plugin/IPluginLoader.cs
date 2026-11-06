using System.Collections.Generic;
using Autofac;

namespace SortingHat.API.Plugin;

public interface IPluginLoader
{
    List<IPlugin> Plugins { get; }

    void RegisterModules(ContainerBuilder builder);
}