using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
            if (tagString.Any(char.IsWhiteSpace)) return null;

            return TagFromList(tagString.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private Tag TagFromList(IEnumerable<string> tagParts)
        {
            Tag parent = null;

            while (true)
            {
                if (tagParts.Any() == false)
                {
                    return null;
                }

                var tag = _newTag(tagParts.First(), parent);

                if (tagParts.Count() == 1)
                {
                    return tag;
                }

                tagParts = tagParts.Skip(1);
                parent = tag;
            }
        }
    }
}
