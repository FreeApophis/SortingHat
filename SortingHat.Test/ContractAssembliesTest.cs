using System.Linq;
using System.Reflection;
using SortingHat.API.Plugin;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// The contract assemblies are the ones hat shares with every plugin. Whatever they expose must be shared as well,
/// otherwise a plugin would load its own copy and its types would not be hat's types.
/// </summary>
public sealed class ContractAssembliesTest
{
    [Fact]
    public void GivenTheContractThenEverythingItReferencesIsInTheContractOrInTheFramework()
        => Assert.Empty(ContractAssemblies.Current.Versions.Keys
            .SelectMany(name => Assembly.Load(name).GetReferencedAssemblies())
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => !ContractAssemblies.Current.Versions.ContainsKey(name) && !IsFramework(name))
            .Distinct());

    [Fact]
    public void GivenTheContractThenItHasTheVersionsHatRunsWith()
        => Assert.Equal(typeof(IPlugin).Assembly.GetName().Version, ContractAssemblies.Current.Versions["SortingHat.API"]);

    private static bool IsFramework(string name)
        => name is "netstandard" or "mscorlib" or "System"
            || name.StartsWith("System.")
            || name.StartsWith("Microsoft.Win32.");
}
