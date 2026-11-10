using System.Diagnostics.CodeAnalysis;
using Autofac;
using SortingHat.App;

namespace SortingHat.CLI;

/// <summary>
/// This is the main entry point into the program, and cannot be tested.
/// It just creates the object-tree and invokes the real application instance.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    private static int Main(string[] args)
    {
        // Create the composition root and build the container
        using var container = new CompositionRoot().Register().Build();

        // The container has all the classes loaded which are not dynamically loaded
        // at this point we resolve the plugin loader to register the missing types
        using var scope = PluginScope.Begin(container);

        // this scope includes now all the plugins, and we can run the application
        return scope.Resolve<Application>().Run(args);
    }
}
