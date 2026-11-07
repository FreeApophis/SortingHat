using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.Models;
using static Funcky.Functional;

namespace SortingHat.DB;

/// <summary>
/// Older hat versions accepted almost any tag name, such as 'NIKON CORPORATION', which a search can never contain.
/// Every such name becomes a valid one like an auto-tag value ('NIKON-CORPORATION'); when a sibling already has that name, both tags
/// are merged with their files and children. A name with nothing usable becomes 'unnamed'.
/// </summary>
[UsedImplicitly]
internal sealed class TagNameGrammarMigration : ICodeMigration
{
    private const string Unnamed = "unnamed";

    public string Name => "20261108-1200-TagNameGrammar";

    public MigrationType MigrationType => MigrationType.Project;

    public void Run(SQLiteDatabase db)
        => db.Query("SELECT Id, Name FROM Tags", reader => (Id: reader.GetInt64(0), Name: reader.GetString(1)))
            .Where(tag => TagName.Parse(tag.Name).Match(left: True, right: False))
            .ForEach(tag => Convert(db, tag.Id));

    /// <summary>A tag can be gone already, merged into a sibling together with its parent.</summary>
    private static void Convert(SQLiteDatabase db, long tagId)
        => Row(db, tagId).AndThen(row =>
        {
            var name = TagName.FromValue(row.Name).Match(none: Unnamed, some: tagName => tagName.Value);

            FindChild(db, row.ParentId, name)
                .Where(siblingId => siblingId != tagId)
                .Switch(
                    none: () => db.ExecuteNonQuery("UPDATE Tags SET Name = @name WHERE Id = @tagId", new SqliteParameter("@name", name), new SqliteParameter("@tagId", tagId)),
                    some: siblingId => Merge(db, tagId, siblingId));
        });

    /// <summary>The files and children of <paramref name="tagId"/> move to <paramref name="intoId"/>, children with the same name are merged as well.</summary>
    private static void Merge(SQLiteDatabase db, long tagId, long intoId)
    {
        db.ExecuteNonQuery("INSERT OR IGNORE INTO FileTags (TagId, FileId) SELECT @intoId, FileId FROM FileTags WHERE TagId = @tagId", new SqliteParameter("@intoId", intoId), new SqliteParameter("@tagId", tagId));
        db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId", new SqliteParameter("@tagId", tagId));

        db.Query("SELECT Id, Name FROM Tags WHERE ParentId = @tagId", reader => (Id: reader.GetInt64(0), Name: reader.GetString(1)), new SqliteParameter("@tagId", tagId))
            .ForEach(child => FindChild(db, Option.Some(intoId), child.Name).Switch(
                none: () => db.ExecuteNonQuery("UPDATE Tags SET ParentId = @intoId WHERE Id = @childId", new SqliteParameter("@intoId", intoId), new SqliteParameter("@childId", child.Id)),
                some: existingId => Merge(db, child.Id, existingId)));

        db.ExecuteNonQuery("DELETE FROM Tags WHERE Id = @tagId", new SqliteParameter("@tagId", tagId));
    }

    private static Option<(Option<long> ParentId, string Name)> Row(SQLiteDatabase db, long tagId)
        => db.Query(
                "SELECT ParentId, Name FROM Tags WHERE Id = @tagId",
                reader => (ParentId: reader.IsDBNull(0) ? Option<long>.None : Option.Some(reader.GetInt64(0)), Name: reader.GetString(1)),
                new SqliteParameter("@tagId", tagId))
            .FirstOrNone();

    private static Option<long> FindChild(SQLiteDatabase db, Option<long> parentId, string name)
        => Option.FromNullable(db.ExecuteScalar(
            "SELECT Id FROM Tags WHERE ParentId IS @parentId AND Name = @name",
            new SqliteParameter("@parentId", parentId.Match(none: () => (object)System.DBNull.Value, some: id => id)),
            new SqliteParameter("@name", name)) as long?);
}
