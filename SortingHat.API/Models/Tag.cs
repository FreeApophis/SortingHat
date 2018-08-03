using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SortingHat.API.Models
{
    public class Tag
    {
        public string Name;
        public Tag Parent;

        public string FullName => $"{(Parent == null ? string.Empty : Parent.FullName)}:{Name}";

        public Tag(string name)
        {
            Name = name;
        }

        public bool Store(IServices services)
        {
            services.DB.StoreTag(this);

            return true;
        }

        public Tag(string name, Tag parent)
        {
            Name = name;
            Parent = parent;
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
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null);
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

        public static IEnumerable<Tag> List()
        {
            List<Tag> tags = new List<Tag>();

            var taxYear = new Tag("Steuerjahr");
            tags.Add(taxYear);
            tags.Add(new Tag("2012", taxYear));
            tags.Add(new Tag("2013", taxYear));
            tags.Add(new Tag("2014", taxYear));
            tags.Add(new Tag("2015", taxYear));
            tags.Add(new Tag("2016", taxYear));
            tags.Add(new Tag("2017", taxYear));
            tags.Add(new Tag("2018", taxYear));

            var gender = new Tag("Geschlecht");
            tags.Add(gender);
            tags.Add(new Tag("male", gender));
            tags.Add(new Tag("female", gender));

            var created = new Tag("created");
            var year = new Tag("2018", created);
            var month = new Tag("02", year);
            var day = new Tag("17", month);

            tags.Add(created);
            tags.Add(year);
            tags.Add(month);
            tags.Add(day);

            return tags;
        }
    }

}
