using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Plugin;

namespace SortingHat.DB.Access;

/// <summary>
/// The settings of a plugin in the Settings tables of the main and the project database, under the name of its assembly,
/// such as <c>SortingHat.Plugin.MoveOnTag/Rules</c>. hat's own keys have no '/', so they never meet.
/// </summary>
[UsedImplicitly]
internal sealed class SQLitePluginSettings<TPlugin>(SQLiteMainDatabase main, SQLiteProjectDatabase project) : IPluginSettings<TPlugin>
    where TPlugin : IPlugin
{
    private static readonly string Prefix = $"{Option.FromNullable(typeof(TPlugin).Assembly.GetName().Name).GetOrElse(typeof(TPlugin).Name)}/";

    public ISettings Global { get; } = new PrefixedSettings(Prefix, new SQLiteSettings(main));

    public ISettings Project { get; } = new PrefixedSettings(Prefix, new SQLiteSettings(project));
}
