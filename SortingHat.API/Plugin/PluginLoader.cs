using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apophis.FileSystem;
using Autofac;
using Autofac.Core;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;

namespace SortingHat.API.Plugin;

/// <summary>
/// Loads every plugin folder in the <see cref="PluginDirectory"/> into its own load context and registers its modules.
/// A plugin loads completely or not at all: whatever goes wrong becomes a <see cref="PluginProblem"/>, and hat runs without that plugin.
/// </summary>
[UsedImplicitly]
public class PluginLoader(PluginDirectory pluginDirectory, IExistsDirectory existsDirectory, ContractAssemblies contractAssemblies) : IPluginLoader
{
    private readonly List<PluginProblem> _problems = [];

    public List<IPlugin> Plugins { get; } = [];

    public IReadOnlyList<PluginProblem> Problems => _problems;

    public void RegisterModules(ContainerBuilder builder)
    {
        foreach (var plugin in PluginLayout.Find(LooseFiles(), Folders()).Select(candidate => candidate.SelectMany(LoadModules)))
        {
            plugin.Switch(
                left: _problems.Add,
                right: modules => Register(builder, modules));
        }
    }

    private static Either<PluginProblem, Assembly> Compatible(PluginCandidate candidate, Assembly assembly, ContractAssemblies hat)
        => PluginCompatibility.Incompatibilities(assembly.GetReferencedAssemblies(), hat) switch
        {
            [] => Either<PluginProblem, Assembly>.Right(assembly),
            var reasons => Either<PluginProblem, Assembly>.Left(new PluginProblem(candidate.Name, string.Join("; ", reasons))),
        };

    private static IReadOnlyList<IModule> Modules(Assembly assembly)
        =>
        [
            .. assembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsClass: true } && typeof(IModule).IsAssignableFrom(type) && typeof(IPlugin).IsAssignableFrom(type))
                .Select(Activator.CreateInstance)
                .OfType<IModule>(),
        ];

    private static Either<PluginProblem, IReadOnlyList<IModule>> AtLeastOne(PluginCandidate candidate, IReadOnlyList<IModule> modules)
        => modules.Count == 0
            ? Either<PluginProblem, IReadOnlyList<IModule>>.Left(new PluginProblem(candidate.Name, "it has no plugin module, a class that is an Autofac module and an IPlugin"))
            : Either<PluginProblem, IReadOnlyList<IModule>>.Right(modules);

    /// <summary>
    /// Autofac runs a module's Load only when the scope is built, where an exception would stop hat.
    /// Running the modules into a throwaway container first catches that, and nothing of a failing module reaches the real scope.
    /// </summary>
    private static IReadOnlyList<IModule> Probe(IReadOnlyList<IModule> modules)
    {
        var probe = new ContainerBuilder();
        modules.ForEach(module => probe.RegisterModule(module));

#pragma warning disable IDISP004 // disposing the probe would dispose instances a module registers from its own fields, which the real scope still uses
        probe.Build(Autofac.Builder.ContainerBuildOptions.IgnoreStartableComponents);
#pragma warning restore IDISP004

        return modules;
    }

    /// <summary>A plugin is foreign code: any exception it throws while loading is a problem of that plugin, not of hat.</summary>
    private static Either<PluginProblem, T> Attempt<T>(PluginCandidate candidate, string failure, Func<T> step)
        where T : notnull
    {
        try
        {
            return Either<PluginProblem, T>.Right(step());
        }
        catch (Exception exception)
        {
            return Either<PluginProblem, T>.Left(new PluginProblem(candidate.Name, $"{failure}: {Message(exception)}"));
        }
    }

    private static string Message(Exception exception)
        => exception switch
        {
            TargetInvocationException { InnerException: { } inner } => Message(inner),
            ReflectionTypeLoadException typeLoad => string.Join("; ", typeLoad.LoaderExceptions.WhereNotNull().Select(Message).Distinct()),
            _ => exception.Message,
        };

    /// <summary>The plugin's references are checked before any of its types is touched, so an incompatible plugin fails with a clear reason.</summary>
    private Either<PluginProblem, IReadOnlyList<IModule>> LoadModules(PluginCandidate candidate)
        => from loaded in Attempt(candidate, "it could not be loaded", () => new PluginLoadContext(candidate.AssemblyPath, contractAssemblies).LoadFromAssemblyPath(candidate.AssemblyPath))
           from assembly in Compatible(candidate, loaded, contractAssemblies)
           from created in Attempt(candidate, "its modules could not be created", () => Modules(assembly))
           from modules in AtLeastOne(candidate, created)
           from registered in Attempt(candidate, "its modules failed to register", () => Probe(modules))
           select registered;

    private void Register(ContainerBuilder builder, IReadOnlyList<IModule> modules)
    {
        modules.ForEach(module => builder.RegisterModule(module));
        Plugins.AddRange(modules.OfType<IPlugin>());
    }

    private IEnumerable<string> LooseFiles()
        => existsDirectory.Exists(pluginDirectory.Path)
            ? Directory.GetFiles(pluginDirectory.Path)
            : [];

    private IEnumerable<PluginFolder> Folders()
        => existsDirectory.Exists(pluginDirectory.Path)
            ? Directory.GetDirectories(pluginDirectory.Path).Select(folder => new PluginFolder(folder, Directory.GetFiles(folder)))
            : [];
}
