using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.CLI.Commands;

[UsedImplicitly]
internal class PluginsCommand(IConsoleWriter consoleWriter, IPluginLoader pluginLoader) : IDeclaredCommand
{
    public CommandGrouping CommandGrouping => CommandGrouping.General;
    public string Name => "plugins";
    public string Description => "Lists the loaded plugins";
    public string Details => "Plugins are the DLLs in the 'plugins' folder next to hat.";

    public Task<bool> ExecuteAsync(CommandInput input, CancellationToken cancellationToken)
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

        return Task.FromResult(true);
    }
}
