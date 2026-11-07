using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.DB.Access;

[UsedImplicitly]
public class SQLiteTag(SQLiteProjectDatabase db) : ITag
{
    /// <summary>Every tag reachable from a root, each parent before its children.</summary>
    private const string AllTags = """
        WITH RECURSIVE Tree(Id, ParentId, Name) AS (
            SELECT Id, ParentId, Name FROM Tags WHERE ParentId IS NULL
            UNION ALL
            SELECT Tags.Id, Tags.ParentId, Tags.Name FROM Tags JOIN Tree ON Tags.ParentId = Tree.Id)
        SELECT Id, ParentId, Name FROM Tree
        """;

    public void Store(Tag tag)
        => FindOrCreate(tag);

    public bool Destroy(Tag tag)
        => Find(tag)
            .Match(
                none: false,
                some: DestroyTag);

    public bool Rename(Tag tag, TagName newName)
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

    /// <summary>Ordered like a tree (<see cref="Tag.Order"/>): every parent comes right before its children.</summary>
    public IReadOnlyList<Tag> GetTags()
    {
        var tags = db.Query(AllTags, ReadRow)
            .Aggregate(ImmutableDictionary<long, Tag>.Empty, (known, row) => known.Add(row.Id, row.ParentId.Match(none: () => Tag.Root(row.Name), some: parentId => Known(known, parentId).Child(row.Name))));

        return [.. tags.Values.Order(Tag.Order)];
    }

    internal Tag Load(long tagId)
        => db.Query("SELECT Id, ParentId, Name FROM Tags WHERE Id = @tagId", ReadRow, new SqliteParameter("@tagId", tagId))
            .FirstOrNone()
            .Match(
                none: () => throw new InvalidDataException($"The database is damaged: the tag {tagId} does not exist."),
                some: row => row.ParentId.Match(none: () => Tag.Root(row.Name), some: parentId => Load(parentId).Child(row.Name)));

    /// <summary>Finds the tag by walking its path from the root, so <c>:2018</c> never matches <c>:tax:2018</c>.</summary>
    internal Option<long> Find(Tag tag)
        => tag.Parent
            .Match(none: () => FindChild(Option<long>.None, tag.Name), some: parent => Find(parent).SelectMany(parentId => FindChild(parentId, tag.Name)));

    internal long FindOrCreate(Tag tag)
    {
        var parentId = tag.Parent.Select(FindOrCreate);

        return FindChild(parentId, tag.Name).GetOrElse(() => Create(parentId, tag.Name));
    }

    private static (long Id, Option<long> ParentId, TagName Name) ReadRow(SqliteDataReader reader)
        => (reader.GetInt64(0), reader.IsDBNull(1) ? Option<long>.None : Option.Some(reader.GetInt64(1)), ReadName(reader.GetString(2)));

    /// <summary>Every name in the database was written from a <see cref="TagName"/>, so one that does not parse means the database is damaged.</summary>
    private static TagName ReadName(string name)
        => TagName.Parse(name).GetOrElse(problem => throw new InvalidDataException($"The database is damaged: {problem}"));

    /// <summary>The tree query returns every parent before its children.</summary>
    private static Tag Known(ImmutableDictionary<long, Tag> known, long tagId)
        => known.GetValueOrNone(tagId).GetOrElse(() => throw new InvalidDataException($"The database is damaged: the tag {tagId} comes before its parent."));

    private static SqliteParameter ParentIdParameter(Option<long> parentId)
        => new("@parentId", parentId.Match(none: DBNull.Value, some: id => (object)id));

    private Option<long> FindChild(Option<long> parentId, TagName name)
        => Option.FromNullable(db.ExecuteScalar("SELECT Id FROM Tags WHERE ParentId IS @parentId AND NameKey = @key", ParentIdParameter(parentId), new SqliteParameter("@key", name.Key)) as long?);

    private long Create(Option<long> parentId, TagName name)
        => db.ExecuteScalar("INSERT INTO Tags (ParentId, Name, NameKey) VALUES(@parentId, @name, @key); SELECT last_insert_rowid();", ParentIdParameter(parentId), new SqliteParameter("@name", name.Value), new SqliteParameter("@key", name.Key)) is long id
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
    private Func<long, bool> RenameTag(TagName newName)
        => tagId => UnlessRejected(() => Execute("UPDATE Tags SET Name = @tagName, NameKey = @key WHERE Id = @tagId", new SqliteParameter("@tagId", tagId), new SqliteParameter("@tagName", newName.Value), new SqliteParameter("@key", newName.Key)));

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
}
