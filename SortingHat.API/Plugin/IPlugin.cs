using System;
using System.Collections.Generic;
using Autofac;
using SortingHat.API.DI;

namespace SortingHat.API.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        Version Version { get; }
        string Description { get; }
    }
}
