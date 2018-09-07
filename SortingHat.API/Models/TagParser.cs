using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.API.Models
{
    [UsedImplicitly]
    public class TagParser : ITagParser
    {
        private readonly Func<string, Tag, Tag> _newTag;

        public TagParser(Func<string, Tag, Tag> newTag)
        {
            _newTag = newTag;
        }

        /// <summary>
        /// A tag always begins with a colon (:) and can have multiple parts, each part beginning with a colon (:)
        /// A tagpart can have no whitespace and has at least one character.
        /// </summary>
        /// <param name="tagString"></param>
        /// <returns>The list is empty in case of an illegal tag string</returns>
        public Tag Parse(string tagString)
        {
            if (tagString == null) return null;
            if (tagString.StartsWith(":") == false) return null;

            return ToTag(tagString.Split(new[] { ':' }).Select(s => s.Trim()).Where(s => s.Length > 0));
        }

        private Tag ToTag(IEnumerable<string> tagParts)
        {
            return tagParts.Aggregate<string, Tag>(null, (current, tagPart) => _newTag(tagPart, current));
        }
    }
}
