using Autofac;
using System.Diagnostics.CodeAnalysis;

namespace SortingHat.CLI
{

    [ExcludeFromCodeCoverage]
    static class Program
    {
        static void Main(string[] args) => new CompositionRoot()
                .Build()
                .Resolve<Application>()
                .Run(args);
    }
}
