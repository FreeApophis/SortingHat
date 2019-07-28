using Autofac;
using System.Diagnostics.CodeAnalysis;
using SortingHat.API.Plugin;

namespace SortingHat.CLI
{

    [ExcludeFromCodeCoverage]
    static class Program
    {
        static void Main(string[] args)
        {
            var container = new CompositionRoot().Register().Build();
            var pluginLoader = container.Resolve<IPluginLoader>();
            using var scope = container.BeginLifetimeScope(builder => pluginLoader.RegisterModules(builder));

            scope.Resolve<Application>().Run(args);
        }
    }
}
