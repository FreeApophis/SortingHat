using Funcky.Monads;
using SortingHat.API.DI;

namespace SortingHat.DB.Access;

/// <summary>The keys of <paramref name="settings"/> that start with <paramref name="prefix"/>, without the prefix: nothing outside of it can be read or written.</summary>
internal sealed class PrefixedSettings(string prefix, ISettings settings) : ISettings
{
    public Option<string> Get(string key)
        => settings.Get(prefix + key);

    public void Set(string key, string value)
        => settings.Set(prefix + key, value);
}
