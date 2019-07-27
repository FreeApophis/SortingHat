using SortingHat.API.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace SortingHat.API.AutoTag
{
    [UsedImplicitly]
    public class AutoTagHandler : IAutoTagHandler
    {
        public IEnumerable<IAutoTag> AutoTags { get; }

        private readonly ITagParser _tagParser;
        private readonly Regex _variableRegex = new Regex("{.*?}");

        public AutoTagHandler(IEnumerable<IAutoTag> autoTags, ITagParser tagParser)
        {
            AutoTags = autoTags;
            _tagParser = tagParser;
        }

        public Tag? TagFromMask(string tagMask, FileInfo file)
        {
            return _tagParser.Parse(ReplaceMask(tagMask, file));
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

        private static string RemoveFirstAndLastCharacter(string bracedString)
        {
            return bracedString.Substring(1, bracedString.Length - 2);
        }

        private string? GetMatchingAutoTag(string autoTagVariable, FileInfo file)
        {
            return AutoTags
                .Where(autoTag => autoTag.FindMatch(autoTagVariable) != null)
                .Select(autoTag => HandleMatchingTag(autoTagVariable, file, autoTag))
                .FirstOrDefault();
        }

        private static string? HandleMatchingTag(string autoTagVariable, FileInfo file, IAutoTag autoTag)
        {
            return autoTag.HandleTag(file, autoTag.FindMatch(autoTagVariable));
        }
    }
}
