using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SortingHat.API.Models;

namespace SortingHat.API.AutoTag;

[UsedImplicitly]
public class AutoTagHandler(IEnumerable<IAutoTag> autoTags, ITagParser tagParser) : IAutoTagHandler
{
    private readonly Regex _variableRegex = new Regex("{.*?}");

    public IEnumerable<IAutoTag> AutoTags { get; } = autoTags;

    public Tag? TagFromMask(string tagMask, FileInfo file)
    {
        return tagParser.Parse(ReplaceMask(tagMask, file));
    }

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
        return tagMask.Replace(match.Value, GetMatchingAutoTag(RemoveFirstAndLastCharacter(match.Value), file));
    }

    private string? GetMatchingAutoTag(string autoTagVariable, FileInfo file)
    {
        return AutoTags
            .Where(autoTag => autoTag.FindMatch(autoTagVariable) != null)
            .Select(autoTag => HandleMatchingTag(autoTagVariable, file, autoTag))
            .FirstOrDefault();
    }
}
