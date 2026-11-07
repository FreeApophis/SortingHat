using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SortingHat.API.Models;

[UsedImplicitly]
internal class TagParser(Func<string, Tag?, Tag> newTag) : ITagParser
{
    /// <summary>
    /// A tag always begins with a colon (:) and can have multiple parts, each part beginning with a colon (:)
    /// A tagpart can have no whitespace and has at least one character.
    /// </summary>
    /// <param name="tagString">The tag in its textual form, e.g. :tax:2018.</param>
    /// <returns>The parsed tag, or null in case of an illegal tag string.</returns>
    public Tag? Parse(string tagString)
    {
        if (tagString == null)
        {
            return null;
        }

        if (!tagString.StartsWith(":"))
        {
            return null;
        }

        return ToTag(tagString.Split([':']).Select(s => s.Trim()).Where(s => s.Length > 0));
    }

    private Tag? ToTag(IEnumerable<string> tagParts)
    {
        Tag? result = null;
        foreach (var part in tagParts)
        {
            result = newTag(part, result);
        }

        return result;
    }
}
