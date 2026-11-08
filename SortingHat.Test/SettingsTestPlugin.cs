using Autofac;
using SortingHat.API.Plugin;

namespace SortingHat.Test;

/// <summary>A plugin module only as the owner of settings, for <see cref="IPluginSettings{TPlugin}"/>; it is never loaded.</summary>
internal sealed class SettingsTestPlugin : Module, IPlugin
{
    public string Name => "Settings test";

    public string Description => "Owns settings in tests.";
}
