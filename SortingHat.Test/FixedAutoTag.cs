using System;
using System.IO;
using Funcky.Monads;
using SortingHat.API.AutoTag;

namespace SortingHat.Test;

/// <summary>An auto-tag whose value the test decides, like a plugin would contribute one.</summary>
internal sealed class FixedAutoTag(string name, AutoTagParameter parameter, Func<AutoTagFile, AutoTagArgument, Option<string>> value) : IAutoTag
{
    public FixedAutoTag(string name, string value)
        : this(name, AutoTagParameter.None, (_, _) => value)
    {
    }

    public string Name => name;

    public AutoTagParameter Parameter => parameter;

    public string Description => $"The test variable {name}.";

    /// <summary>A file the auto-tags can read from, without touching the disk.</summary>
    public static AutoTagFile AnyFile
        => new(new FileInfo(Path.Combine(Path.GetTempPath(), "any.txt")));

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => value(file, argument);
}
