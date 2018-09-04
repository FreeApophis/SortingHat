using Autofac.Core;
using Autofac;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace SortingHat.API.Plugin
{
    [UsedImplicitly]
    public class PluginLoader : IPluginLoader
    {
        public List<IPlugin> Plugins { get; } = new List<IPlugin>();

        private string PluginDirectory => Path.Combine(AppContext.BaseDirectory, "plugins");
        private string PluginFilePattern => "*.dll";

        public void RegisterModules(ContainerBuilder builder)
        {
            foreach (var assembly in GetPluginAssemblies())
            {
                //  Gets the all modules from each assembly to be registered.
                //  Make sure that each module **MUST** have a parameterless constructor.
                var modules = assembly.GetTypes()
                    .Where(p => typeof(IModule).IsAssignableFrom(p) && !p.IsAbstract)
                    .Select(p => (IModule)Activator.CreateInstance(p));

                //  Regsiters each module.
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
            return Directory.GetFiles(PluginDirectory, PluginFilePattern, SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom);
        }
    }
}
