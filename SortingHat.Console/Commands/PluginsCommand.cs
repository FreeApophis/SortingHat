using System.Collections.Generic;
using System.Linq;
using Apophis.CLI.Writer;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class PluginsCommand(IConsoleWriter consoleWriter, IPluginLoader pluginLoader) : ICommand
{
    public string LongCommand => "plugins";
    public Option<string> ShortCommand => Option<string>.None;
    public string ShortHelp => "Lists the loaded plugins";
    public CommandGrouping CommandGrouping => CommandGrouping.General;

    public bool Execute(IEnumerable<string> arguments, IOptionParser options)
    {
        if (arguments.Any())
        {
            consoleWriter.WriteLine("Too many arguments given, the plugin command does not take any arguments.");
            return false;
        }

        return ListPlugins();
    }

    private bool ListPlugins()
    {
        if (pluginLoader.Plugins.Any())
        {
            consoleWriter.WriteLine("Loaded Plugins:");
            foreach (var plugin in pluginLoader.Plugins)
            {
                consoleWriter.WriteLine($"{plugin.Name} v{plugin.Version}");
                consoleWriter.WriteLine();
                consoleWriter.WriteLine($"  {plugin.Description}");
                consoleWriter.WriteLine();
            }
        }
        else
        {
            consoleWriter.WriteLine("No plugins loaded");
        }

        return true;
    }
}
