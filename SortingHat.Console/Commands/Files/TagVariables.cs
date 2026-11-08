using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using SortingHat.API.AutoTag;
using static Funcky.Functional;

namespace SortingHat.CLI.Commands.Files;

/// <summary>
/// The value of every tag variable for a file, as file-info shows it: filled through a real tag mask,
/// so it is exactly the tag level a mask would give, after normalisation.
/// </summary>
internal static class TagVariables
{
    /// <summary>A numbered variable such as Folder.FromRoot is shown up to its last value, but never further than this.</summary>
    private const int MaximumNumbers = 32;

    /// <returns>A line per variable that has a value for the <paramref name="file"/>, such as <c>  {Camera.Make} = Canon</c>.</returns>
    public static IEnumerable<string> Lines(IAutoTagHandler autoTagHandler, AutoTagFile file)
        => autoTagHandler.AutoTags
            .OrderBy(autoTag => autoTag.Name, StringComparer.Ordinal)
            .SelectMany(autoTag => Values(autoTagHandler, autoTag, file))
            .Select(variable => $"  {{{variable.Form}}} = {variable.Value}");

    /// <summary>Numbers count up from 0 and stop at the first one without a value; choices without a value are left out.</summary>
    private static IEnumerable<(string Form, string Value)> Values(IAutoTagHandler autoTagHandler, IAutoTag autoTag, AutoTagFile file)
    {
        var values = autoTag.Parameter.Forms(autoTag.Name)
            .Take(MaximumNumbers)
            .Select(form => (Form: form, Value: Value(autoTagHandler, form, file)));

        return (autoTag.Parameter == AutoTagParameter.Integer
                ? values.TakeWhile(variable => variable.Value.Match(none: false, some: True))
                : values)
            .WhereSelect(variable => variable.Value.Select(value => (variable.Form, value)));
    }

    private static Option<string> Value(IAutoTagHandler autoTagHandler, string form, AutoTagFile file)
        => autoTagHandler.ParseMask($":{{{form}}}")
            .SelectMany(mask => mask.Apply(file))
            .RightOrNone()
            .Select(tag => tag.Name.Value);
}
