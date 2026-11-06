using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apophis.FileSystem;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;

namespace SortingHat.API.Plugin;

[UsedImplicitly]
public class PluginLoader(IExistsDirectory existsDirectory) : IPluginLoader
{
    public List<IPlugin> Plugins { get; } = [];

    private static string PluginDirectory => Path.Combine(AppContext.BaseDirectory, "plugins");

    private static string PluginFilePattern => "*.dll";

    public void RegisterModules(ContainerBuilder builder)
    {
        foreach (var assembly in GetPluginAssemblies())
        {
            // Gets the all modules from each assembly to be registered.
            //  Make sure that each module **MUST** have a parameterless constructor.
            var modules = assembly.GetTypes()
                .Where(p => typeof(IModule).IsAssignableFrom(p) && !p.IsAbstract)
                .Select(p => Activator.CreateInstance(p))
                .OfType<IModule>();

            // Registers each module.
            foreach (var module in modules)
            {
                RegisterModule(builder, module);
            }
        }
    }

    private void RegisterModule(ContainerBuilder builder, IModule module)
    {
        // A plugin module must also implement the IPlugin interface
        if (module is IPlugin plugin)
        {
            builder.RegisterModule(module);
            Plugins.Add(plugin);
        }
    }

    private IEnumerable<Assembly> GetPluginAssemblies()
    {
        if (existsDirectory.Exists(PluginDirectory))
        {
            return Directory.GetFiles(PluginDirectory, PluginFilePattern, SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom);
        }

        return [];
    }
}
