using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.DB.Access
{
    [UsedImplicitly]
    public class SQLiteTag : ITag
    {
        private readonly SQLiteProjectDatabase _db;
        public SQLiteTag(SQLiteProjectDatabase db)
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
            var tagId = Find(tag);
            if (tagId.HasValue)
            {
                var tagIdParameter = new SqliteParameter("@tagId", tagId);
                _db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId", tagIdParameter);
                _db.ExecuteNonQuery("DELETE FROM Tags WHERE Id = @tagId", tagIdParameter);
            }

            return tagId.HasValue;
        }

        public bool Rename(Tag tag, string newName)
        {
            var tagId = Find(tag);
            if (tagId.HasValue)
            {
                var tagIdParameter = new SqliteParameter("@tagId", tagId);
                var tagNameParameter = new SqliteParameter("@tagName", newName);

                _db.ExecuteNonQuery("UPDATE Tags SET Name = @tagName WHERE Id = @tagId", tagIdParameter, tagNameParameter);
            }

            return tagId.HasValue;
        }

        public bool Move(Tag tag, Tag destinationTag)
        {
            var tagId = Find(tag);

            if (tagId.HasValue)
            {
                var tagIdParameter = new SqliteParameter("@tagId", tagId);
                if (destinationTag is null)
                {
                    return ExecuteMoveQuery(tagIdParameter, new SqliteParameter("@destinationTagId", DBNull.Value));
                }

                var destinationTagId = Find(destinationTag);
                if (destinationTagId.HasValue)
                {
                    return ExecuteMoveQuery(tagIdParameter, new SqliteParameter("@destinationTagId", destinationTagId));
                }
            }

            return false;
        }

        private bool ExecuteMoveQuery(SqliteParameter tagIdParameter, SqliteParameter destinationTagIdParameter)
        {
            _db.ExecuteNonQuery("UPDATE Tags SET ParentId = @destinationTagId WHERE Id = @tagId", tagIdParameter, destinationTagIdParameter);

            return true;
        }

        public long FileCount(Tag tag)
        {
            var tagId = Find(tag);
            var count = _db.ExecuteScalar("SELECT COUNT(*) FROM Tags JOIN FileTags ON FileTags.TagId = Tags.Id WHERE Tags.Id = @tagId", new SqliteParameter("@tagId", tagId)) as long?;
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

        private string TagIdsQuery(Tag tag)
        {
            var tags = Ancestors(tag).Select((t, id) => new { Id = id, t.Name }).ToList();
            var query = new StringBuilder();

            query.AppendLine($"SELECT {string.Join(", ", tags.Select(t => $"T{t.Id}.Id Id{t.Id}"))}");
            query.AppendLine("FROM Tags T0");

            foreach (var t in tags.Skip(1))
            {
                query.AppendLine($"JOIN Tags T{t.Id} ON T{t.Id}.ParentId == T{t.Id - 1}.Id");
            }

            query.AppendLine($"WHERE {string.Join(" AND ", tags.Select(t => $"T{t.Id}.Name = '{t.Name}'"))}");

            return query.ToString();
        }

        private List<long> TagIds(Tag tag)
        {
            var reader = _db.ExecuteReader(TagIdsQuery(tag));
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
            var x = TagIds(tag);

            return x.Count == 0 ? null as long? : x.Last();
        }

        internal long FindOrCreate(Tag tag)
        {
            long? parentId = null;
            if (tag.Parent != null)
            {
                parentId = FindOrCreate(tag.Parent);
            }

            long? resultId = _db.ExecuteScalar($"SELECT Id FROM Tags WHERE ParentId {DBString.ToComparison(parentId)} AND Name = '{tag.Name}'") as long?;
            if (resultId.HasValue == false)
            {
                resultId = _db.ExecuteScalar($"INSERT INTO Tags (ParentId, Name) VALUES({DBString.ToSQL(parentId)}, '{tag.Name}'); SELECT last_insert_rowid();") as long?;
            }

            if (resultId == null)
            {
                throw new NotImplementedException("resultId cannot be null.");
            }
                return resultId.Value;
        }

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

        private bool GetTagTree(SqliteDataReader reader, ref List<Tag> result, Tag? parent, int level)
        {
            // We must be on the right level on entry into this method
            Debug.Assert(reader.GetInt32(3) == level);

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
                        if (GetTagTree(reader, ref result, tag, level + 1) == false)
                        {
                            return false;
                        }
                    } else if (nextLevel < level)
                    {
                        return true;
                    }
                } else
                {
                    // We are at the end of the list ... 
                    return false;
                }
            }
            return true;
        }

        internal Tag Load(long tagId)
        {
            var reader = _db.ExecuteReader("SELECT ParentId, Name FROM Tags WHERE Id = @tagId", new SqliteParameter("@tagId", tagId));

            if (!reader.Read()) throw new NotSupportedException();

            return reader.IsDBNull(0)
                ? new Tag(this, reader.GetString(1))
                : new Tag(this, reader.GetString(1), Load(reader.GetInt64(0)));

        }
    }
}
