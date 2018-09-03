using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class PluginsCommand : ICommand
    {
        private readonly IPluginLoader _pluginLoader;

        public PluginsCommand(IPluginLoader pluginLoader)
        {
            _pluginLoader = pluginLoader;
        }
        public bool Execute(IEnumerable<string> arguments)
        {
            Console.WriteLine("Loaded Plugins:");
            foreach (var plugin in _pluginLoader.Plugins)
            {
                Console.WriteLine($"{plugin.Name} v{plugin.Version}");
                Console.WriteLine();
                Console.WriteLine($"  {plugin.Description}");
                Console.WriteLine();
            }

            return true;
        }

        public string LongCommand => "plugins";
        public string ShortCommand => null;
        public string ShortHelp => "Lists the loaded plugins";
    }
}
