using System.Reflection;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.CLI;

/// <summary>The name hat runs under, for hints such as "run 'hat help'": the name of the entry assembly, 'hat' when there is none.</summary>
[UsedImplicitly]
internal sealed class ApplicationName
{
    public string Name { get; } = EntryAssemblyName().GetOrElse("hat");

    private static Option<string> EntryAssemblyName()
        => from assembly in Option.FromNullable(Assembly.GetEntryAssembly())
           from name in Option.FromNullable(assembly.GetName().Name)
           select name;
}
