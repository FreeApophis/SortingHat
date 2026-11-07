using Funcky.Monads;

namespace SortingHat.API.AutoTag;

/// <summary>
/// A variable for tag masks such as <c>:Photos:{Camera.Make}</c>: hat fills it per file when it tags files.
/// A variable is written <c>{Name}</c>, or <c>{Name.Argument}</c> when it has a <see cref="Parameter"/>, such as <c>{CreatedAt.Year}</c>.
/// </summary>
public interface IAutoTag
{
    /// <summary>The name in the mask, such as <c>Camera.Make</c>. Use the plugin's name as a prefix, two auto-tags must not share a name.</summary>
    string Name { get; }

    /// <summary>The argument the variable takes after its name; hat checks it before it touches any file.</summary>
    AutoTagParameter Parameter { get; }

    /// <summary>What the value is, for the help.</summary>
    string Description { get; }

    /// <summary>
    /// The value for the <paramref name="file"/>, which becomes (part of) a tag level; none when the file has no such value, such as a PDF and a camera.
    /// Read what several variables need once with <see cref="AutoTagFile.Remember{T}"/>. An exception skips the tag for this file and is reported.
    /// </summary>
    Option<string> Value(AutoTagFile file, AutoTagArgument argument);
}
