using System.Collections.Generic;
using SortingHat.API.DI;

namespace SortingHat.API.Models;

public class Tag
{
    private readonly ITag _tag;

    public Tag(ITag tag, string name, Tag? parent = null)
    {
        _tag = tag;
        Name = name;
        Parent = parent;

        parent?.Children?.Add(this);
    }

    public string Name { get; set; }
    public Tag? Parent { get; }
    public List<Tag> Children { get; } = [];
    public long FileCount => _tag.FileCount(this);

    public string FullName => $"{(Parent == null ? string.Empty : Parent.FullName)}:{Name}";

    public static bool operator ==(Tag? lhs, Tag? rhs)
    {
        if (lhs is null)
        {
            return rhs is null;
        }

        return lhs.Equals(rhs);
    }

    public static bool operator !=(Tag? lhs, Tag? rhs)
    {
        return !(lhs == rhs);
    }

    public static IEnumerable<Tag> List(ITag tag)
    {
        return tag.GetTags();
    }

    public bool Store()
    {
        return _tag.Store(this);
    }

    public bool Destroy()
    {
        return _tag.Destroy(this);
    }

    public bool Rename(string newName)
    {
        return _tag.Rename(this, newName);
    }

    public bool Move(Tag destinationTag)
    {
        return _tag.Move(this, destinationTag);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is Tag tag)
        {
            return FullName == tag.FullName;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }
}
