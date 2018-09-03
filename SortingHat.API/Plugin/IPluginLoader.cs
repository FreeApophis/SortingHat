using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.API.Plugin
{
    public interface IPluginLoader
    {
        List<IPlugin> Plugins { get; }
        List<ICommand> Commands { get; }
        void Load(string pluginPath);
    }
}