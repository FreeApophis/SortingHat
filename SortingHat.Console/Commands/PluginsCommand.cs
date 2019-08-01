using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;
using System.Collections.Generic;
using System.Linq;
using apophis.CLI.Writer;
using Funcky.Monads;

namespace SortingHat.CLI.Commands
{
    [UsedImplicitly]
    internal class PluginsCommand : ICommand
    {
        private readonly IConsoleWriter _consoleWriter;
        private readonly IPluginLoader _pluginLoader;

        public PluginsCommand(IConsoleWriter consoleWriter, IPluginLoader pluginLoader)
        {
            _consoleWriter = consoleWriter;
            _pluginLoader = pluginLoader;
        }

        public string LongCommand => "plugins";
        public Option<string> ShortCommand => Option<string>.None();
        public string ShortHelp => "Lists the loaded plugins";
        public CommandGrouping CommandGrouping => CommandGrouping.General;

        public bool Execute(IEnumerable<string> arguments, IOptionParser options)
        {
            if (arguments.Any())
            {
                _consoleWriter.WriteLine("Too many arguments given, the plugin command does not take any arguments.");
                return false;
            }

            ListPlugins();

            return true;
        }

        private void ListPlugins()
        {
            if (_pluginLoader.Plugins.Any())
            {
                _consoleWriter.WriteLine("Loaded Plugins:");
                foreach (var plugin in _pluginLoader.Plugins)
                {
                    _consoleWriter.WriteLine($"{plugin.Name} v{plugin.Version}");
                    _consoleWriter.WriteLine();
                    _consoleWriter.WriteLine($"  {plugin.Description}");
                    _consoleWriter.WriteLine();
                }
            }
            else
            {
                _consoleWriter.WriteLine("No plugins loaded");
            }
        }
    }
}
