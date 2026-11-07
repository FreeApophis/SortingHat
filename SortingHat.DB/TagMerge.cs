using System;
using System.Collections.Generic;
using Funcky.Extensions;
using Microsoft.Data.Sqlite;

namespace SortingHat.DB;

/// <summary>Merges two tags that are the same tag, for migrations: the oldest one stays, on every level.</summary>
internal static class TagMerge
{
    /// <summary>Merges the siblings <paramref name="tagId"/> and <paramref name="siblingId"/>.</summary>
    /// <returns>The tag that stays, the older one.</returns>
    public static long Siblings(SQLiteDatabase db, long tagId, long siblingId, StringComparer names)
    {
        var (keepId, dropId) = tagId < siblingId ? (tagId, siblingId) : (siblingId, tagId);

        Into(db, dropId, keepId, names);

        return keepId;
    }

    /// <summary>
    /// The files and children of <paramref name="tagId"/> move to <paramref name="intoId"/>, then <paramref name="tagId"/> is deleted.
    /// Children with the same name under both are merged as well, the older one stays.
    /// </summary>
    private static void Into(SQLiteDatabase db, long tagId, long intoId, StringComparer names)
    {
        db.ExecuteNonQuery("INSERT OR IGNORE INTO FileTags (TagId, FileId) SELECT @intoId, FileId FROM FileTags WHERE TagId = @tagId", new SqliteParameter("@intoId", intoId), new SqliteParameter("@tagId", tagId));
        db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId", new SqliteParameter("@tagId", tagId));

        Children(db, tagId).ForEach(child => Children(db, intoId)
            .FirstOrNone(existing => names.Equals(existing.Name, child.Name))
            .Switch(
                none: () => Reparent(db, child.Id, intoId),
                some: existing => Reparent(db, Siblings(db, child.Id, existing.Id, names), intoId)));

        db.ExecuteNonQuery("DELETE FROM Tags WHERE Id = @tagId", new SqliteParameter("@tagId", tagId));
    }

    private static void Reparent(SQLiteDatabase db, long tagId, long parentId)
        => db.ExecuteNonQuery("UPDATE Tags SET ParentId = @parentId WHERE Id = @tagId", new SqliteParameter("@parentId", parentId), new SqliteParameter("@tagId", tagId));

    private static IReadOnlyList<(long Id, string Name)> Children(SQLiteDatabase db, long tagId)
        => db.Query("SELECT Id, Name FROM Tags WHERE ParentId = @tagId", reader => (Id: reader.GetInt64(0), Name: reader.GetString(1)), new SqliteParameter("@tagId", tagId));
}
