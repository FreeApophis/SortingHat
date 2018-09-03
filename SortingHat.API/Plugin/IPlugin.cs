using System;
using System.Collections.Generic;
using System.ComponentModel;
using SortingHat.API.DI;

namespace SortingHat.API.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        Version Version { get; }
        string Description { get; }

        void Register(List<ICommand> pluginCommands);
    }
}
