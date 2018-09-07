using System;

namespace SortingHat.API.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        Version Version { get; }
        string Description { get; }
    }
}
