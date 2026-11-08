using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Funcky;
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

    /// <summary>Everything is checked before the first tag moves, then all of them move in one transaction.</summary>
    public Either<string, Unit> Move(IReadOnlyList<Tag> tags, Option<Tag> destination)
        => TagMoves.Problem(tags, destination).Match(
            none: () => from destinationId in DestinationId(destination)
                        from tagIds in TagIds(tags)
                        from free in NamesFree(tags.Zip(tagIds, (tag, tagId) => (Tag: tag, Id: tagId)), destination, destinationId)
                        select MoveAll(tagIds, destinationId),
            some: Either<string, Unit>.Left);

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

    private static string Where(Option<Tag> destination)
        => destination.Match(none: "the top level", some: tag => $"'{tag}'");

    private Either<string, Option<long>> DestinationId(Option<Tag> destination)
        => destination.Match(
            none: () => Either<string, Option<long>>.Right(Option<long>.None),
            some: tag => Find(tag).Match(
                none: () => Either<string, Option<long>>.Left($"The tag '{tag}' does not exist."),
                some: tagId => Either<string, Option<long>>.Right(Option.Some(tagId))));

    private Either<string, ImmutableList<long>> TagIds(IReadOnlyList<Tag> tags)
        => tags.Aggregate(
            Either<string, ImmutableList<long>>.Right([]),
            (found, tag) => from tagIds in found
                            from tagId in Find(tag).Match(none: () => Either<string, long>.Left($"The tag '{tag}' does not exist."), some: Either<string, long>.Right)
                            select tagIds.Add(tagId));

    /// <summary>A tag already under the destination has its own name there, that is no conflict.</summary>
    private Either<string, Unit> NamesFree(IEnumerable<(Tag Tag, long Id)> moves, Option<Tag> destination, Option<long> destinationId)
        => moves
            .FirstOrNone(move => FindChild(destinationId, move.Tag.Name).Match(none: false, some: childId => childId != move.Id))
            .Match(
                none: () => Either<string, Unit>.Right(Unit.Value),
                some: move => Either<string, Unit>.Left($"'{move.Tag}' cannot move to {Where(destination)}, a tag named '{move.Tag.Name}' is already there."));

    private Unit MoveAll(IReadOnlyList<long> tagIds, Option<long> destinationId)
        => db.InTransaction(() =>
        {
            tagIds.ForEach(tagId => db.ExecuteNonQuery("UPDATE Tags SET ParentId = @parentId WHERE Id = @tagId", ParentIdParameter(destinationId), new SqliteParameter("@tagId", tagId)));

            return Unit.Value;
        });
}
