using System.Reflection;
using System.Runtime.Loader;

namespace SortingHat.API.Plugin;

/// <summary>
/// Every plugin gets its own load context, so it loads its own dependencies from its folder, with the versions it was built with.
/// The contract assemblies always come from hat, otherwise the plugin's IPlugin would be another type than hat's.
/// </summary>
internal sealed class PluginLoadContext(string assemblyPath, ContractAssemblies contractAssemblies)
    : AssemblyLoadContext(assemblyPath)
{
    private readonly AssemblyDependencyResolver _resolver = new(assemblyPath);

    // returning null is the AssemblyLoadContext contract for "ask hat's default context"
    protected override Assembly? Load(AssemblyName assemblyName)
        => assemblyName.Name is { } name && !contractAssemblies.Contains(name) && _resolver.ResolveAssemblyToPath(assemblyName) is { } path
            ? LoadFromAssemblyPath(path)
            : null;

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
        => _resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path
            ? LoadUnmanagedDllFromPath(path)
            : nint.Zero;
}
