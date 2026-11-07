using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apophis.FileSystem;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;

namespace SortingHat.API.Plugin;

/// <summary>Loads every plugin folder in the <see cref="PluginDirectory"/> into its own load context and registers its modules.</summary>
[UsedImplicitly]
public class PluginLoader(PluginDirectory pluginDirectory, IExistsDirectory existsDirectory) : IPluginLoader
{
    private readonly List<PluginProblem> _problems = [];

    public List<IPlugin> Plugins { get; } = [];

    public IReadOnlyList<PluginProblem> Problems => _problems;

    public void RegisterModules(ContainerBuilder builder)
    {
        foreach (var plugin in PluginLayout.Find(LooseFiles(), Folders()))
        {
            plugin.Switch(
                left: _problems.Add,
                right: candidate => RegisterModules(builder, candidate));
        }
    }

    private static IEnumerable<IModule> Modules(Assembly assembly)
        => assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsClass: true } && typeof(IModule).IsAssignableFrom(type) && typeof(IPlugin).IsAssignableFrom(type))
            .Select(Activator.CreateInstance)
            .OfType<IModule>();

    private void RegisterModules(ContainerBuilder builder, PluginCandidate candidate)
    {
        var context = new PluginLoadContext(candidate.AssemblyPath, ContractAssemblies.Names);

        foreach (var module in Modules(context.LoadFromAssemblyPath(candidate.AssemblyPath)))
        {
            builder.RegisterModule(module);
            Plugins.Add((IPlugin)module);
        }
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
