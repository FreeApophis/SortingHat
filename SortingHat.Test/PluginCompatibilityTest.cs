using System;
using System.Collections.Generic;
using System.Reflection;
using SortingHat.API.Plugin;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Whether hat can serve the assemblies a plugin was built against: the contract assemblies by semantic versioning, the framework by its version.
/// </summary>
public sealed class PluginCompatibilityTest
{
    private static readonly ContractAssemblies Hat = new(
        new Dictionary<string, Version> { ["SortingHat.API"] = new(2, 3, 1, 0), ["Autofac"] = new(9, 3, 4, 0) },
        new Version(10, 0, 5));

    [Theory]
    [InlineData("SortingHat.API, Version=2.3.0.0")]
    [InlineData("SortingHat.API, Version=2.0.0.0")]
    [InlineData("SortingHat.API, Version=2.3.9.0")]
    [InlineData("Autofac, Version=9.0.0.0")]
    [InlineData("MetadataExtractor, Version=99.0.0.0")]
    [InlineData("System.Runtime, Version=8.0.0.0")]
    [InlineData("System.Runtime, Version=10.0.0.0")]
    public void GivenAReferenceHatCanServeThenThePluginIsCompatible(string reference)
        => Assert.Empty(PluginCompatibility.Incompatibilities([new AssemblyName(reference)], Hat));

    [Theory]
    [InlineData("SortingHat.API, Version=2.4.0.0", "it needs SortingHat.API 2.4 or newer, hat has 2.3.1")]
    [InlineData("SortingHat.API, Version=1.9.0.0", "it was built for SortingHat.API 1.x, hat has 2.3.1")]
    [InlineData("SortingHat.API, Version=3.0.0.0", "it was built for SortingHat.API 3.x, hat has 2.3.1")]
    [InlineData("Autofac, Version=10.0.0.0", "it was built for Autofac 10.x, hat has 9.3.4")]
    [InlineData("System.Runtime, Version=11.0.0.0", "it needs .NET 11.0, hat runs on .NET 10.0")]
    public void GivenAReferenceHatCannotServeThenThePluginIsIncompatible(string reference, string reason)
        => Assert.Equal([reason], PluginCompatibility.Incompatibilities([new AssemblyName(reference)], Hat));

    [Fact]
    public void GivenSeveralReferencesHatCannotServeThenEveryOneIsReported()
        => Assert.Equal(2, PluginCompatibility.Incompatibilities([new AssemblyName("SortingHat.API, Version=3.0.0.0"), new AssemblyName("Autofac, Version=10.0.0.0")], Hat).Count);
}
