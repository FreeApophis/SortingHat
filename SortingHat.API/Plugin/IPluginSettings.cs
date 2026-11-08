using SortingHat.API.DI;

namespace SortingHat.API.Plugin;

/// <summary>
/// The settings of the plugin <typeparamref name="TPlugin"/>, kept apart from hat's settings and from every other plugin:
/// hat stores each key under the name of the plugin's assembly. Values are strings; store structured values as JSON.
/// </summary>
/// <typeparam name="TPlugin">The plugin module, such as <c>MoveOnTagModule</c>.</typeparam>
public interface IPluginSettings<TPlugin>
    where TPlugin : IPlugin
{
    /// <summary>Settings that apply in every project.</summary>
    ISettings Global { get; }

    /// <summary>Settings of the active project, such as rules that name its tags.</summary>
    ISettings Project { get; }
}
