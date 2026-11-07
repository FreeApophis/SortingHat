using System;
using Funcky.Monads;

namespace SortingHat.API.AutoTag;

/// <summary>A helper for auto-tags that give a part of a date: <c>{Name.Year}</c>, <c>{Name.Month}</c>, ... The plugin only finds the date.</summary>
public abstract class DateAutoTag : IAutoTag
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public AutoTagParameter Parameter
        => DateParts.Parameter;

    public Option<string> Value(AutoTagFile file, AutoTagArgument argument)
        => from key in argument.Text
           from part in DateParts.Find(key)
           from date in Date(file)
           select part.Select(date);

    /// <summary>The date of the <paramref name="file"/>, none when it has none.</summary>
    protected abstract Option<DateTime> Date(AutoTagFile file);
}
