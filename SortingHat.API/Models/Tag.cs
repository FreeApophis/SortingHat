using SortingHat.API.DI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SortingHat.API.Models
{
    public class Tag
    {
        public string Name;
        public Tag Parent;
        public List<Tag> Children { get; } = new List<Tag>();

        public string FullName => $"{(Parent == null ? string.Empty : Parent.FullName)}:{Name}";

        public Tag(string name)
        {
            Name = name;
        }

        public bool Store(IDatabase db)
        {
            return db.Tag.Store(this);
        }

        public bool Destroy(IDatabase db)
        {
            return db.Tag.Destroy(this);
        }

        public Tag(string name, Tag parent)
        {
            Name = name;
            Parent = parent;

            parent?.Children?.Add(this);
        }

        /// <summary>
        /// A tag always begins with a colon (:) and can have multiple parts, each part beginning with a colon (:)
        /// A tagpart can have no whitespace and has at least one character.
        /// </summary>
        /// <param name="tagString"></param>
        /// <returns>The list is empty in case of an illegal tag string</returns>
        public static Tag Parse(string tagString)
        {
            if (tagString == null) return null;
            if (tagString.StartsWith(":") == false) return null;
            if (tagString.Any(Char.IsWhiteSpace)) return null;

            return TagFromList(tagString.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static Tag TagFromList(IEnumerable<string> tagParts, Tag parent = null)
        {
            if (tagParts.Any() == false)
            {
                return null;
            }

            var tag = new Tag(tagParts.First(), parent);

            if (tagParts.Count() == 1)
            {
                return tag;
            }

            return TagFromList(tagParts.Skip(1), tag);
        }

        public static bool operator ==(Tag lhs, Tag rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Tag lhs, Tag rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            var tag = other as Tag;
            return FullName == tag.FullName;
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }

        public static IEnumerable<Tag> List(IDatabase db)
        {
            return db.Tag.GetTags();
        }
    }

}
