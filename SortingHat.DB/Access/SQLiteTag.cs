using System;
using System.Collections.Generic;
using System.Diagnostics;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.DB.Access;

[UsedImplicitly]
public class SQLiteTag(SQLiteProjectDatabase db) : ITag
{
    private const string AllTags = @"WITH RECURSIVE
  tree(id, parentid, name,level) AS (
    SELECT Tags.Id, Tags.ParentId, Tags.name, 0
    FROM Tags
    WHERE ParentId IS NULL
    UNION ALL
    SELECT Tags.Id, Tags.ParentId, Tags.name, tree.level+1
      FROM Tags JOIN tree ON Tags.ParentId=tree.Id
     ORDER BY 4 DESC, 3 ASC
  )
SELECT id, parentId, name, level FROM tree;";

    public bool Store(Tag tag)
    {
        FindOrCreate(tag);
        return true;
    }

    public bool Destroy(Tag tag)
        => Find(tag)
            .Match(
                none: false,
                some: DestroyTag);

    public bool Rename(Tag tag, string newName)
        => Find(tag)
            .Match(
                none: false,
                some: RenameTag(newName));

    public bool Move(Tag tag, Tag destinationTag)
        => (from tagId in Find(tag)
            from destinationTagId in Find(destinationTag)
            select MoveTag(tagId, destinationTagId))
            .GetOrElse(false);

    public long FileCount(Tag tag)
        => Find(tag).Match(none: 0L, some: CountFiles);

    public IEnumerable<Tag> GetTags()
        => db.ExecuteReader(AllTags, ReadTagTree);

    internal Tag Load(long tagId)
        => db.ExecuteReader("SELECT ParentId, Name FROM Tags WHERE Id = @tagId", ReadTag, new SqliteParameter("@tagId", tagId));

    /// <summary>Finds the tag by walking its path from the root, so <c>:2018</c> never matches <c>:tax:2018</c>.</summary>
    internal Option<long> Find(Tag tag)
        => Option.FromNullable(tag.Parent)
            .Match(none: () => FindChild(Option<long>.None, tag.Name), some: parent => Find(parent).SelectMany(parentId => FindChild(parentId, tag.Name)));

    internal long FindOrCreate(Tag tag)
    {
        var parentId = Option.FromNullable(tag.Parent).Select(FindOrCreate);

        return FindChild(parentId, tag.Name).GetOrElse(() => Create(parentId, tag.Name));
    }

    private static SqliteParameter ParentIdParameter(Option<long> parentId)
        => new("@parentId", parentId.Match(none: DBNull.Value, some: id => (object)id));

    private Option<long> FindChild(Option<long> parentId, string name)
        => Option.FromNullable(db.ExecuteScalar("SELECT Id FROM Tags WHERE ParentId IS @parentId AND Name = @name", ParentIdParameter(parentId), new SqliteParameter("@name", name)) as long?);

    private long Create(Option<long> parentId, string name)
        => db.ExecuteScalar("INSERT INTO Tags (ParentId, Name) VALUES(@parentId, @name); SELECT last_insert_rowid();", ParentIdParameter(parentId), new SqliteParameter("@name", name)) is long id
            ? id
            : throw new InvalidOperationException($"Tag '{name}' could not be created.");

    /// <summary>The foreign key on <c>Tags.ParentId</c> rejects a tag that still has children, and the transaction keeps its files tagged.</summary>
    private bool DestroyTag(long tagId)
        => UnlessRejected(() => db.InTransaction(() =>
        {
            var tagIdParameter = new SqliteParameter("@tagId", tagId);
            db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId", tagIdParameter);
            db.ExecuteNonQuery("DELETE FROM Tags WHERE Id = @tagId", tagIdParameter);

            return true;
        }));

    /// <summary>The unique tag name indexes reject a new name that one of its siblings already has.</summary>
    private Func<long, bool> RenameTag(string newName)
        => tagId => UnlessRejected(() => Execute("UPDATE Tags SET Name = @tagName WHERE Id = @tagId", new SqliteParameter("@tagId", tagId), new SqliteParameter("@tagName", newName)));

    /// <summary>The unique tag name indexes reject a destination that already has a child with the same name.</summary>
    private bool MoveTag(long tagId, long destinationTagId)
        => UnlessRejected(() => Execute("UPDATE Tags SET ParentId = @destinationTagId WHERE Id = @tagId", new SqliteParameter("@tagId", tagId), new SqliteParameter("@destinationTagId", destinationTagId)));

    private bool Execute(string commandText, params SqliteParameter[] parameters)
    {
        db.ExecuteNonQuery(commandText, parameters);

        return true;
    }

    /// <summary>A change the database rejects with a constraint (unique index or foreign key) is an expected failure, not an error.</summary>
    private static bool UnlessRejected(Func<bool> change)
    {
        try
        {
            return change();
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
        {
            return false;
        }
    }

    private long CountFiles(long tagId)
        => db.ExecuteScalar("SELECT COUNT(*) FROM FileTags WHERE TagId = @tagId", new SqliteParameter("@tagId", tagId)) as long? ?? 0;

    private IEnumerable<Tag> ReadTagTree(SqliteDataReader reader)
    {
        if (reader.Read())
        {
            var result = new List<Tag>();
            GetTagTree(reader, ref result, null, 0);
            return result;
        }

        return [];
    }

    private Tag ReadTag(SqliteDataReader reader)
    {
        if (!reader.Read())
        {
            throw new NotSupportedException();
        }

        return reader.IsDBNull(0)
            ? new Tag(this, reader.GetString(1))
            : new Tag(this, reader.GetString(1), Load(reader.GetInt64(0)));
    }

    private bool GetTagTree(SqliteDataReader reader, ref List<Tag> result, Tag? parent, int level)
    {
        // We must be on the right level on entry into this method
        Debug.Assert(reader.GetInt32(3) == level, "The reader must be positioned on a tag of the given level.");

        // we iterate on the siblings on the same level
        while (reader.GetInt32(3) == level)
        {
            var tag = new Tag(this, reader.GetString(2), parent);
            result.Add(tag);
            if (reader.Read())
            {
                var nextLevel = reader.GetInt32(3);

                // A child has level + 1
                if (nextLevel == level + 1)
                {
                    if (!GetTagTree(reader, ref result, tag, level + 1))
                    {
                        return false;
                    }
                }
                else if (nextLevel < level)
                {
                    return true;
                }
            }
            else
            {
                // We are at the end of the list ...
                return false;
            }
        }

        return true;
    }
}
