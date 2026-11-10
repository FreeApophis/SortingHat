using System;
using Autofac;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.Plugin;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// The settings of a plugin, through the real composition root with real SQLite databases:
/// separated from hat's settings and from other plugins, globally or in the active project.
/// </summary>
public sealed class PluginSettingsTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAGlobalSettingThenTheNextRunOfHatStillHasIt()
    {
        InScope(settings => settings.Global.Set("Colour", "green"));

        Assert.Some("green", Get(settings => settings.Global.Get("Colour")));
    }

    [Fact]
    public void GivenAGlobalSettingThenEveryProjectHasIt()
    {
        InScope(settings => settings.Global.Set("Colour", "green"));
        _hat.Hat(["new-project", "other"]);
        _hat.Hat(["switch-project", "other"]);

        Assert.Some("green", Get(settings => settings.Global.Get("Colour")));
    }

    [Fact]
    public void GivenAProjectSettingThenOnlyThatProjectHasIt()
    {
        InScope(settings => settings.Project.Set("Colour", "green"));
        _hat.Hat(["new-project", "other"]);
        _hat.Hat(["switch-project", "other"]);

        Assert.None(Get(settings => settings.Project.Get("Colour")));

        _hat.Hat(["switch-project", "Default"]);
        Assert.Some("green", Get(settings => settings.Project.Get("Colour")));
    }

    [Fact]
    public void GivenAGlobalSettingThenTheProjectScopeDoesNotHaveIt()
    {
        InScope(settings => settings.Global.Set("Colour", "green"));

        Assert.None(Get(settings => settings.Project.Get("Colour")));
    }

    [Fact]
    public void GivenAPluginSettingNamedLikeOneOfHatThenHatKeepsItsOwn()
    {
        InScope(settings => settings.Global.Set("ProjectDatabaseName", "elsewhere"));

        _hat.Hat(["list-projects"]);
        Assert.Equal(["* Default"], _hat.Lines);
    }

    [Fact]
    public void GivenASettingOfHatThenAPluginCannotReadIt()
    {
        _hat.Hat(["new-project", "other"]);
        _hat.Hat(["switch-project", "other"]);

        Assert.None(Get(settings => settings.Global.Get("ProjectDatabaseName")));
    }

    public void Dispose()
        => _hat.Dispose();

    private void InScope(Action<IPluginSettings<SettingsTestPlugin>> change)
        => _hat.InScope(scope =>
        {
            change(scope.Resolve<IPluginSettings<SettingsTestPlugin>>());

            return true;
        });

    private Option<string> Get(Func<IPluginSettings<SettingsTestPlugin>, Option<string>> read)
        => _hat.InScope(scope => read(scope.Resolve<IPluginSettings<SettingsTestPlugin>>()));
}
