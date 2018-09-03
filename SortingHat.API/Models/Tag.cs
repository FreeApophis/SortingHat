using SortingHat.API.DI;
using System.Collections.Generic;

namespace SortingHat.API.Models
{
    public class Tag
    {
        private readonly IDatabase _db;

        public string Name { get; set; }
        public Tag Parent { get; }
        public List<Tag> Children { get; } = new List<Tag>();
        public long FileCount => _db.Tag.FileCount(this);

        public string FullName => $"{(Parent == null ? string.Empty : Parent.FullName)}:{Name}";

        public Tag(IDatabase db, string name, Tag parent = null)
        {
            _db = db;
            Name = name;
            Parent = parent;

            parent?.Children?.Add(this);
        }

        public bool Store()
        {
            return _db.Tag.Store(this);
        }

        public bool Destroy()
        {
            return _db.Tag.Destroy(this);
        }

        public bool Rename(string newName)
        {
            return _db.Tag.Rename(this, newName);
        }

        public bool Move(Tag destinationTag)
        {
            return _db.Tag.Move(this, destinationTag);
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

