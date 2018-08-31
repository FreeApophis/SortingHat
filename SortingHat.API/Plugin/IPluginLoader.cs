using System.Collections.Generic;

namespace SortingHat.API.Plugin
{
    public interface IPluginLoader
    {
        List<IPlugin> Plugins { get; }
        void Load(string pluginPath);
    }
}