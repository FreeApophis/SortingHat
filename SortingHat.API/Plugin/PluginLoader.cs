using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using apophis.FileSystem;

namespace SortingHat.API.Plugin
{
    [UsedImplicitly]
    public class PluginLoader : IPluginLoader
    {
        private readonly IExistsDirectory _existsDirectory;

        public PluginLoader(IExistsDirectory existsDirectory)
        {
            _existsDirectory = existsDirectory;
        }

        public List<IPlugin> Plugins { get; } = new List<IPlugin>();

        private static string PluginDirectory => Path.Combine(PathToExecutable(), "plugins");

        private static string PathToExecutable()
        {
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        private static string PluginFilePattern => "*.dll";

        public void RegisterModules(ContainerBuilder builder)
        {
            foreach (var assembly in GetPluginAssemblies())
            {
                //  Gets the all modules from each assembly to be registered.
                //  Make sure that each module **MUST** have a parameterless constructor.
                var modules = assembly.GetTypes()
                    .Where(p => typeof(IModule).IsAssignableFrom(p) && !p.IsAbstract)
                    .Select(p => (IModule)Activator.CreateInstance(p));

                //  Registers each module.
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
            if (_existsDirectory.Exists(PluginDirectory))
            {
                return Directory.GetFiles(PluginDirectory, PluginFilePattern, SearchOption.TopDirectoryOnly)
                    .Select(Assembly.LoadFrom);
            }

            return Enumerable.Empty<Assembly>();
        }
    }
}
