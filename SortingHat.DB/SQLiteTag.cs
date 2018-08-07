using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SortingHat.DB
{
    class SQLiteTag : ITag
    {
        private SQLiteDB _db;
        public SQLiteTag(SQLiteDB db)
        {
            _db = db;
        }

        public void Destroy(Tag tag)
        {
            throw new NotImplementedException();
        }

        public void Store(Tag tag)
        {
            FindOrCreate(tag);
        }

        private List<Tag> Ancestors(Tag tag)
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
            var tags = Ancestors(tag).Select((t, ID) => new { ID, t.Name });
            var query = new StringBuilder();

            query.AppendLine($"SELECT {string.Join(", ", tags.Select(t => $"T{t.ID}.ID"))}");
            query.AppendLine("FROM Tags T0");

            foreach (var t in tags.Skip(1))
            {
                query.AppendLine($"JOIN Tags T{t.ID} ON T{t.ID}.ParentID == T{t.ID - 1}.ID");
            }

            query.AppendLine($"WHERE {string.Join(" AND ", tags.Select(t => $"T{t.ID}.Name = '{t.Name}'"))}");

            return query.ToString();
        }

        internal List<long> TagIDs(Tag tag)
        {
            var result = new List<long>();

            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand findCommand = connection.CreateCommand();
                findCommand.CommandText = TagIDsQuery(tag);
                var reader = findCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        result.Add(reader.GetInt64(i));
                    }
                }

                connection.Close();
            }
            return result;
        }

        internal long? Find(Tag tag)
        {
            var tagIDs = TagIDs(tag);

            return (tagIDs.Count == 0) ? (long?)null : tagIDs.First();
        }

        internal long FindOrCreate(Tag tag)
        {
            long? parentID = null;
            if (tag.Parent != null)
            {
                parentID = FindOrCreate(tag.Parent);
            }

            long? resultID = null;
            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand findCommand = connection.CreateCommand();
                findCommand.CommandText = $"SELECT ID FROM Tags WHERE ParentID {DBString.ToComparison(parentID)} AND Name = '{tag.Name}'";
                resultID = findCommand.ExecuteScalar() as long?;


                if (resultID.HasValue == false)
                {
                    SqliteCommand initializeCommand = connection.CreateCommand();
                    initializeCommand.CommandText = $"INSERT INTO Tags (ParentID, Name) VALUES({DBString.ToSQL(parentID)}, '{tag.Name}'); SELECT last_insert_rowid();";
                    resultID = (long)initializeCommand.ExecuteScalar();
                }

                connection.Close();
            }

            return resultID.Value;
        }

        const string allTags = @"WITH RECURSIVE
  tree(id, parentid, name,level) AS (
    SELECT Tags.ID, Tags.ParentID, Tags.name, 0
    FROM Tags
    WHERE ParentID IS NULL
    UNION ALL
    SELECT Tags.ID, Tags.ParentID, Tags.name, tree.level+1
      FROM Tags JOIN tree ON Tags.ParentID=tree.ID
     ORDER BY 4 DESC
  )
SELECT id, parentid, name, level FROM tree;";

        public IEnumerable<Tag> GetTags()
        {
            var result = new List<Tag>();
            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = allTags;
                var reader = initializeCommand.ExecuteReader();

                if (reader.Read())
                {
                    GetTagTree(reader, ref result, null, 0);
                }

                connection.Close();
            }

            return result.OrderBy(t => t.FullName);
        }

        private bool GetTagTree(SqliteDataReader reader, ref List<Tag> result, Tag parent, int level)
        {
            // We must be on the right level on entry into this method
            Debug.Assert(reader.GetInt32(3) == level);

            // we iterate on the siblings on the same level
            while (reader.GetInt32(3) == level)
            {
                var tag = new Tag(reader.GetString(2), parent);
                result.Add(tag);
                if (reader.Read())
                {
                    var test = reader.IsDBNull(3);
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
    }
}
