using System;

namespace SortingHat.API.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        Version Version => GetType().Assembly.GetName().Version ?? new Version();
        string Description { get; }
    }
}
