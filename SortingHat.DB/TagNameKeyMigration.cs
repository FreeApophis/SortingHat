using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.Models;

namespace SortingHat.DB;

/// <summary>
/// Tag names ignore case: <c>NameKey</c> holds the name in upper case (<see cref="TagName.Key"/>) and is unique among siblings, <c>Name</c> keeps the spelling.
/// Older databases can have siblings that differ only in case: they are merged with their files and children, the oldest one stays.
/// Running it again is harmless, so a test can run it after changing names.
/// </summary>
[UsedImplicitly]
internal sealed class TagNameKeyMigration : ICodeMigration
{
    public string Name => "20261108-1800-TagNameKeys";

    public MigrationType MigrationType => MigrationType.Project;

    public void Run(SQLiteDatabase db)
    {
        db.ExecuteNonQuery("""
            DROP INDEX IF EXISTS TagsRootNameIndex;
            DROP INDEX IF EXISTS TagsChildNameIndex;
            DROP INDEX IF EXISTS TagsRootNameKeyIndex;
            DROP INDEX IF EXISTS TagsChildNameKeyIndex;
            """);

        if (!db.Query("SELECT name FROM pragma_table_info('Tags')", reader => reader.GetString(0)).Contains("NameKey"))
        {
            db.ExecuteNonQuery("ALTER TABLE Tags ADD COLUMN NameKey TEXT NOT NULL DEFAULT ''");
        }

        db.Query("SELECT Id FROM Tags ORDER BY Id", reader => reader.GetInt64(0)).ForEach(tagId => MergeWithOlderSibling(db, tagId));

        db.Query("SELECT Id, Name FROM Tags", reader => (Id: reader.GetInt64(0), Name: reader.GetString(1)))
            .ForEach(tag => db.ExecuteNonQuery("UPDATE Tags SET NameKey = @key WHERE Id = @tagId", new SqliteParameter("@key", TagName.KeyOf(tag.Name)), new SqliteParameter("@tagId", tag.Id)));

        db.ExecuteNonQuery("""
            CREATE UNIQUE INDEX TagsRootNameKeyIndex ON Tags (NameKey) WHERE ParentId IS NULL;
            CREATE UNIQUE INDEX TagsChildNameKeyIndex ON Tags (ParentId, NameKey) WHERE ParentId IS NOT NULL;
            """);
    }

    /// <summary>A tag can be gone already, merged into an older sibling of its parent.</summary>
    private static void MergeWithOlderSibling(SQLiteDatabase db, long tagId)
        => db.Query(
                "SELECT ParentId, Name FROM Tags WHERE Id = @tagId",
                reader => (ParentId: reader.IsDBNull(0) ? Option<long>.None : Option.Some(reader.GetInt64(0)), Name: reader.GetString(1)),
                new SqliteParameter("@tagId", tagId))
            .FirstOrNone()
            .SelectMany(tag => OlderSibling(db, tagId, tag.ParentId, tag.Name))
            .AndThen(siblingId => TagMerge.Siblings(db, tagId, siblingId, StringComparer.OrdinalIgnoreCase));

    private static Option<long> OlderSibling(SQLiteDatabase db, long tagId, Option<long> parentId, string name)
        => db.Query(
                "SELECT Id, Name FROM Tags WHERE ParentId IS @parentId AND Id < @tagId ORDER BY Id",
                reader => (Id: reader.GetInt64(0), Name: reader.GetString(1)),
                new SqliteParameter("@parentId", parentId.Match(none: () => (object)DBNull.Value, some: id => id)),
                new SqliteParameter("@tagId", tagId))
            .FirstOrNone(sibling => StringComparer.OrdinalIgnoreCase.Equals(sibling.Name, name))
            .Select(sibling => sibling.Id);
}
