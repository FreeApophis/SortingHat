using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Plugin;

/// <summary>SortingHat.API and everything it references: the assemblies hat shares with every plugin.</summary>
internal static class ContractAssemblies
{
    public static IReadOnlySet<string> Names { get; } = Closure(typeof(IPlugin).Assembly);

    private static IReadOnlySet<string> Closure(Assembly root)
    {
        var names = new HashSet<string>();
        var pending = new Stack<Assembly>([root]);

        while (pending.TryPop(out var assembly))
        {
            if (assembly.GetName().Name is { } name && names.Add(name))
            {
                assembly.GetReferencedAssemblies()
                    .Where(reference => reference.Name is { } referenceName && !names.Contains(referenceName))
                    .WhereSelect(LoadOrNone)
                    .ToList()
                    .ForEach(pending.Push);
            }
        }

        return names;
    }

    // an optional reference that is not there is not part of the contract
    private static Option<Assembly> LoadOrNone(AssemblyName name)
    {
        try
        {
            return Assembly.Load(name);
        }
        catch (FileNotFoundException)
        {
            return Option<Assembly>.None;
        }
    }
}
