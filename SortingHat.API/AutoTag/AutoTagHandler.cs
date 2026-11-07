using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.Models;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
internal class AutoTagHandler(IEnumerable<IAutoTag> autoTags) : IAutoTagHandler
{
    private readonly Regex _variableRegex = new("{.*?}");

    public IEnumerable<IAutoTag> AutoTags { get; } = autoTags;

    public Option<Tag> TagFromMask(string tagMask, FileInfo file)
        => Tag.Parse(ReplaceMask(tagMask, file)).RightOrNone();

    private static string RemoveFirstAndLastCharacter(string bracedString)
    {
        return bracedString.Substring(1, bracedString.Length - 2);
    }

    private static string? HandleMatchingTag(string autoTagVariable, FileInfo file, IAutoTag autoTag)
    {
        return autoTag.HandleTag(file, autoTag.FindMatch(autoTagVariable));
    }

    private string ReplaceMask(string tagMask, FileInfo file)
    {
        return _variableRegex
            .Matches(tagMask)
            .Cast<Match>()
            .Aggregate(tagMask, (t, match) => ReplaceMatch(t, file, match));
    }

    private string ReplaceMatch(string tagMask, FileInfo file, Capture match)
    {
        // values read from files are often padded (EXIF strings), a level cannot start or end with whitespace
        return tagMask.Replace(match.Value, GetMatchingAutoTag(RemoveFirstAndLastCharacter(match.Value), file)?.Trim());
    }

    private string? GetMatchingAutoTag(string autoTagVariable, FileInfo file)
    {
        return AutoTags
            .Where(autoTag => autoTag.FindMatch(autoTagVariable) != null)
            .Select(autoTag => HandleMatchingTag(autoTagVariable, file, autoTag))
            .FirstOrDefault();
    }
}
