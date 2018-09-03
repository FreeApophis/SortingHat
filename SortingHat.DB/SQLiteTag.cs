using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace SortingHat.DB
{
    [UsedImplicitly]
    public class SQLiteTag : ITag
    {
        private readonly SQLiteDB _db;
        public SQLiteTag(SQLiteDB db)
        {
            _db = db;
        }

        public bool Store(Tag tag)
        {
            FindOrCreate(tag);
            return true;
        }

        public bool Destroy(Tag tag)
        {
            var tagID = Find(tag);
            if (tagID.HasValue)
            {
                var tagIDParameter = new SqliteParameter("@tagID", tagID);
                _db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagID = @tagID", tagIDParameter);
                _db.ExecuteNonQuery("DELETE FROM Tags WHERE ID = @tagID", tagIDParameter);
            }

            return tagID.HasValue;
        }

        public bool Rename(Tag tag, string newName)
        {
            var tagID = Find(tag);
            if (tagID.HasValue)
            {
                var tagIDParameter = new SqliteParameter("@tagID", tagID);
                var tagNameParameter = new SqliteParameter("@tagName", newName);

                _db.ExecuteNonQuery("UPDATE Tags SET Name = @tagName WHERE ID = @tagID", tagIDParameter, tagNameParameter);
            }

            return tagID.HasValue;
        }

        public bool Move(Tag tag, Tag destinationTag)
        {
            var tagID = Find(tag);

            if (tagID.HasValue)
            {
                var tagIDParameter = new SqliteParameter("@tagID", tagID);
                if (destinationTag == null)
                {
                    return ExecuteMoveQuery(tagIDParameter, new SqliteParameter("@destinationTagID", DBNull.Value));
                }

                var destinationTagID = Find(destinationTag);
                if (destinationTagID.HasValue)
                {
                    return ExecuteMoveQuery(tagIDParameter, new SqliteParameter("@destinationTagID", destinationTagID));
                }
            }

            return false;
        }

        private bool ExecuteMoveQuery(SqliteParameter tagIDParameter, SqliteParameter destinationTagIDParameter)
        {
            _db.ExecuteNonQuery("UPDATE Tags SET ParentID = @destinationTagID WHERE ID = @tagID", tagIDParameter, destinationTagIDParameter);

            return true;
        }

        public long FileCount(Tag tag)
        {
            var tagID = Find(tag);
            var count = _db.ExecuteScalar("SELECT COUNT(*) FROM Tags JOIN FileTags ON FileTags.TagID = Tags.ID WHERE Tags.ID = @tagID", new SqliteParameter("@tagID", tagID)) as long?;
            return count ?? 0;
        }

        private static List<Tag> Ancestors(Tag tag)
        {
            if (tag.Parent == null)
            {
                return new List<Tag> { tag };
            }

            var result = Ancestors(tag.Parent);
            result.Add(tag);
            return result;
        }

        private string TagIDsQuery(Tag tag)
        {
            var tags = Ancestors(tag).Select((t, id) => new { ID = id, t.Name });
            var query = new StringBuilder();

            query.AppendLine($"SELECT {string.Join(", ", tags.Select(t => $"T{t.ID}.ID ID{t.ID}"))}");
            query.AppendLine("FROM Tags T0");

            foreach (var t in tags.Skip(1))
            {
                query.AppendLine($"JOIN Tags T{t.ID} ON T{t.ID}.ParentID == T{t.ID - 1}.ID");
            }

            query.AppendLine($"WHERE {string.Join(" AND ", tags.Select(t => $"T{t.ID}.Name = '{t.Name}'"))}");

            return query.ToString();
        }

        private List<long> TagIDs(Tag tag)
        {
            var reader = _db.ExecuteReader(TagIDsQuery(tag));
            var result = new List<long>();

            if (reader.HasRows)
            {
                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    result.Add(reader.GetInt64(i));
                }
            }

            return result;
        }

        internal long? Find(Tag tag)
        {
            var tagIDs = TagIDs(tag);

            return (tagIDs.Count == 0) ? (long?)null : tagIDs.Last();
        }

        internal long FindOrCreate(Tag tag)
        {
            long? parentID = null;
            if (tag.Parent != null)
            {
                parentID = FindOrCreate(tag.Parent);
            }

            long? resultID = _db.ExecuteScalar($"SELECT ID FROM Tags WHERE ParentID {DBString.ToComparison(parentID)} AND Name = '{tag.Name}'") as long?;
            if (resultID.HasValue == false)
            {
                resultID = (long)_db.ExecuteScalar($"INSERT INTO Tags (ParentID, Name) VALUES({DBString.ToSQL(parentID)}, '{tag.Name}'); SELECT last_insert_rowid();");
            }

            return resultID.Value;
        }

        private const string AllTags = @"WITH RECURSIVE
  tree(id, parentid, name,level) AS (
    SELECT Tags.ID, Tags.ParentID, Tags.name, 0
    FROM Tags
    WHERE ParentID IS NULL
    UNION ALL
    SELECT Tags.ID, Tags.ParentID, Tags.name, tree.level+1
      FROM Tags JOIN tree ON Tags.ParentID=tree.ID
     ORDER BY 4 DESC, 3 ASC
  )
SELECT id, parentid, name, level FROM tree;";

        public IEnumerable<Tag> GetTags()
        {
            var reader = _db.ExecuteReader(AllTags);

            if (reader.Read())
            {
                var result = new List<Tag>();
                GetTagTree(reader, ref result, null, 0);
                return result;
            }


            return Enumerable.Empty<Tag>();
        }

        private bool GetTagTree(SqliteDataReader reader, ref List<Tag> result, Tag parent, int level)
        {
            // We must be on the right level on entry into this method
            Debug.Assert(reader.GetInt32(3) == level);

            // we iterate on the siblings on the same level
            while (reader.GetInt32(3) == level)
            {
                var tag = new Tag(_db, reader.GetString(2), parent);
                result.Add(tag);
                if (reader.Read())
                {
                    var nextLevel = reader.GetInt32(3);
                    // A child has level + 1
                    if (nextLevel == level + 1)
                    {
                        if (GetTagTree(reader, ref result, tag, level + 1) == false)
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

        internal Tag Load(long tagID)
        {
            var reader = _db.ExecuteReader("SELECT ParentID, Name FROM Tags WHERE ID = @tagID", new SqliteParameter("@tagID", tagID));

            if (!reader.Read()) throw new NotSupportedException();

            return reader.IsDBNull(0)
                ? new Tag(_db, reader.GetString(1))
                : new Tag(_db, reader.GetString(1), Load(reader.GetInt64(0)));

        }
    }
}
