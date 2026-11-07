using Funcky.Monads;

namespace SortingHat.API.DI;

/// <summary>Settings that hat keeps between runs, such as the active project.</summary>
public interface ISettings
{
    /// <summary>The value of the <paramref name="key"/>, none when it was never set.</summary>
    Option<string> Get(string key);

    /// <summary>Sets the <paramref name="key"/> to <paramref name="value"/>, replacing its value.</summary>
    void Set(string key, string value);
}
