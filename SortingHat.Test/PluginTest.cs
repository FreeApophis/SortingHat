using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Plugins in the 'plugins' folder next to hat, through the real command line application.
/// Every plugin has its own folder with the plugin and its own dependencies: plugins/SortingHat.Plugin.Exif/SortingHat.Plugin.Exif.dll.
/// </summary>
public sealed class PluginTest
{
    private const string Working = "Working";
    private const string Broken = "Broken";

    [Fact]
    public void GivenPluginsInTheirOwnFoldersThenHatLoadsThem()
    {
        using var hat = HatSandbox.WithPluginFixtures(Working);

        Assert.Equal(0, hat.Hat(["plugins"]));

        Assert.Contains(hat.Lines, line => line.StartsWith("Exif Tagger v"));
        Assert.Contains(hat.Lines, line => line.StartsWith("File Type Plugin v"));
        Assert.DoesNotContain(hat.Lines, line => line.StartsWith("Warning"));
    }

    [Fact]
    public void GivenAPluginWithItsOwnDependenciesThenItsCommandsUseThem()
    {
        using var hat = HatSandbox.WithPluginFixtures(Working);
        var image = hat.WriteFile("test.png", string.Empty);
        using (var resource = typeof(PluginTest).Assembly.GetManifestResourceStream("SortingHat.Test.Resources.test.png") ?? throw new InvalidOperationException("test.png is missing"))
        using (var file = File.Create(image))
        {
            resource.CopyTo(file);
        }

        Assert.Equal(0, hat.Hat(["exif", image]));

        Assert.Contains(hat.Lines, line => line.StartsWith("PNG-IHDR - Image Width"));
    }

    [Fact]
    public void GivenADllDirectlyInThePluginsFolderThenHatWarnsAndStillRuns()
    {
        using var hat = new HatSandbox();
        Directory.CreateDirectory(hat.PluginDirectory);
        File.WriteAllText(Path.Combine(hat.PluginDirectory, "Loose.dll"), "not an assembly");

        Assert.Equal(0, hat.Hat(["plugins"]));

        Assert.Contains(hat.Lines, line => line.StartsWith("Warning") && line.Contains("Loose.dll") && line.Contains("own folder"));
        Assert.Contains("No plugins loaded", hat.Lines);
    }

    [Fact]
    public void GivenAPluginFolderWithoutItsAssemblyThenHatWarnsAndStillRuns()
    {
        using var hat = new HatSandbox();
        var folder = Directory.CreateDirectory(Path.Combine(hat.PluginDirectory, "Missing")).FullName;
        File.WriteAllText(Path.Combine(folder, "Other.dll"), "not an assembly");

        Assert.Equal(0, hat.Hat(["plugins"]));

        Assert.Contains(hat.Lines, line => line.StartsWith("Warning") && line.Contains("Missing.dll"));
        Assert.Contains("No plugins loaded", hat.Lines);
    }

    [Theory]
    [InlineData("ThrowsInLoad", "broken on purpose")]
    [InlineData("ThrowsInConstructor", "broken on purpose")]
    [InlineData("NotAnAssembly", "")]
    [InlineData("NoModule", "no plugin module")]
    public void GivenABrokenPluginThenHatWarnsAndLoadsTheOtherPlugins(string plugin, string reason)
    {
        using var hat = HatSandbox.WithPluginFixtures(Broken);

        Assert.Equal(0, hat.Hat(["plugins"]));

        Assert.Contains(hat.Lines, line => line.StartsWith($"Warning: plugin '{plugin}' not loaded") && line.Contains(reason));
        Assert.Contains(hat.Lines, line => line.StartsWith("File Type Plugin v"));
        Assert.DoesNotContain(hat.Lines, line => line.StartsWith("Failing Test Plugin"));
    }

    [Fact]
    public void GivenAPluginThatFailsWhileRegisteringThenNothingOfItIsRegistered()
    {
        using var hat = HatSandbox.WithPluginFixtures(Broken);

        Assert.NotEqual(0, hat.Hat(["half-loaded"]));
    }

    [Fact]
    public void GivenNoPluginsFolderThenHatRunsWithoutWarnings()
    {
        using var hat = new HatSandbox();

        Assert.Equal(0, hat.Hat(["plugins"]));

        Assert.Equal(["No plugins loaded"], hat.Lines.Where(line => line.Length > 0));
    }
}
