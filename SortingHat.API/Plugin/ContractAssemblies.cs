using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Apophis.CLI.Writer;
using Apophis.FileSystem;
using Autofac;
using Funcky.Extensions;
using Funcky.Lexer;
using Funcky.Monads;

namespace SortingHat.API.Plugin;

/// <summary>
/// The assemblies hat shares with every plugin, with the versions hat has: SortingHat.API and every assembly its public types expose.
/// A plugin always gets them from hat, so its types are hat's types, and it must have been built against a version hat can serve.
/// </summary>
/// <param name="Versions">The version hat has of every contract assembly, by assembly name.</param>
/// <param name="Runtime">The .NET version hat runs on.</param>
public sealed record ContractAssemblies(IReadOnlyDictionary<string, Version> Versions, Version Runtime)
{
    /// <summary>The contract of this hat. ContractAssembliesTest checks that it covers everything these assemblies reference outside the framework.</summary>
    public static ContractAssemblies Current { get; } = new(
        new Dictionary<string, Version>(
            new[]
            {
                typeof(IPlugin),
                typeof(IConsoleWriter),
                typeof(IExistsFile),
                typeof(Apophis.Utils.EnumerableExtensions),
                typeof(ContainerBuilder),
                typeof(Option<>),
                typeof(ILexemeWalker),
            }
            .Select(type => type.Assembly.GetName())
            .WhereSelect(NameAndVersion)),
        Environment.Version);

    public bool Contains(string assemblyName)
        => Versions.ContainsKey(assemblyName);

    private static Option<KeyValuePair<string, Version>> NameAndVersion(AssemblyName assembly)
        => from name in Option.FromNullable(assembly.Name)
           from version in Option.FromNullable(assembly.Version)
           select KeyValuePair.Create(name, version);
}
