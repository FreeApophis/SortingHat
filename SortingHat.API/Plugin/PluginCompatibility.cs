using System;
using System.Collections.Generic;
using System.Reflection;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Plugin;

/// <summary>
/// Whether hat can serve the assemblies a plugin was built against.
/// A contract assembly must have the same major version and at least the minor version the plugin was built with (semantic versioning: minor versions only add).
/// The framework must not be older than the one the plugin was built for.
/// </summary>
internal static class PluginCompatibility
{
    private const string Framework = "System.Runtime";

    /// <summary>Why hat can't serve the <paramref name="references"/>; none when the plugin is compatible.</summary>
    public static IReadOnlyList<string> Incompatibilities(IEnumerable<AssemblyName> references, ContractAssemblies hat)
        => [.. references.WhereSelect(reference => Incompatibility(reference, hat))];

    private static Option<string> Incompatibility(AssemblyName reference, ContractAssemblies hat)
        => from name in Option.FromNullable(reference.Name)
           from needed in Option.FromNullable(reference.Version)
           from reason in name == Framework
               ? Runtime(needed, hat.Runtime)
               : hat.Versions.GetValueOrNone(name).SelectMany(has => Contract(name, needed, has))
           select reason;

    private static Option<string> Contract(string name, Version needed, Version has)
        => needed.Major != has.Major
            ? $"it was built for {name} {needed.Major}.x, hat has {has.ToString(3)}"
            : needed.Minor > has.Minor
                ? $"it needs {name} {needed.ToString(2)} or newer, hat has {has.ToString(3)}"
                : Option<string>.None;

    private static Option<string> Runtime(Version needed, Version has)
        => (needed.Major, needed.Minor).CompareTo((has.Major, has.Minor)) > 0
            ? $"it needs .NET {needed.ToString(2)}, hat runs on .NET {has.ToString(2)}"
            : Option<string>.None;
}
