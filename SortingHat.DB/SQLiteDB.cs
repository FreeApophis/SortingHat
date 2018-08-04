using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SortingHat.DB
{
    public class SQLiteDB : IDatabase
    {
        private readonly string _path;

        public SQLiteDB(string path)
        {
            _path = path;
        }

        public void Setup()
        {
            var migrator = new RevisionMigrator(this);

            migrator.Initialize();
            migrator.Migrate();
        }

        private string DBPath()
        {
            string result = Path.Combine(_path, ".hat");

            if (Directory.Exists(result) == false)
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }

        private string DBFile()
        {
            return Path.Combine(DBPath(), "hat.db");
        }

        internal SqliteConnection Connection()
        {
            return new SqliteConnection($"Filename={DBFile()}");
        }

        string ToComparison(long? value)
        {
            if (value.HasValue)
            {
                return $"= {value}";
            }

            return "IS NULL";
        }

        string ToSQL(long? value)
        {
            if (value.HasValue)
            {
                return value.ToString();
            }

            return "NULL";
        }

        #region Files
        public void TagFile(API.Models.File file, Tag tag)
        {
            long fileID = FindOrCreateFile(file);
            long tagID = FindOrCreateTag(tag);

            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand tagCommand = connection.CreateCommand();
                tagCommand.CommandText = $"INSERT INTO FileTags (TagID, FileID) VALUES({tagID},{fileID});";
                tagCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        private long? FindFile(API.Models.File file)
        {
            long? resultID;
            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand findCommand = connection.CreateCommand();
                findCommand.CommandText = $"SELECT ID FROM Files WHERE Hash = '{file.Hash}'";
                resultID = findCommand.ExecuteScalar() as long?;

                connection.Close();
            }

            return resultID;
        }
        private long CreateFile(API.Models.File file)
        {
            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = $"INSERT INTO Files (Hash, Size, CreatedAt) VALUES('{file.Hash}','{file.Size}','{file.CreatedAt}'); SELECT last_insert_rowid();";
                var resultID = (long)initializeCommand.ExecuteScalar();
                connection.Close();

                return resultID;
            }
        }

        private long FindOrCreateFile(API.Models.File file)
        {
            long? fileID = FindFile(file);

            if (fileID.HasValue)
            {
                return fileID.Value;
            }

            return CreateFile(file);
        }

        public void UntagFile(API.Models.File file, Tag tag)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tags
        public void StoreTag(Tag tag)
        {
            FindOrCreateTag(tag);
        }

        private long? FindTag(Tag tag)
        {
            long? parentID = null;
            if (tag.Parent != null)
            {
                parentID = FindTag(tag.Parent);
            }

            long? resultID = null;
            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand findCommand = connection.CreateCommand();
                findCommand.CommandText = $"SELECT ID FROM Tags WHERE ParentID {ToComparison(parentID)} AND Name = '{tag.Name}'";
                resultID = findCommand.ExecuteScalar() as long?;

                connection.Close();
            }

            return resultID;
        }

        private long FindOrCreateTag(Tag tag)
        {
            long? parentID = null;
            if (tag.Parent != null)
            {
                parentID = FindOrCreateTag(tag.Parent);
            }

            long? resultID = null;
            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand findCommand = connection.CreateCommand();
                findCommand.CommandText = $"SELECT ID FROM Tags WHERE ParentID {ToComparison(parentID)} AND Name = '{tag.Name}'";
                resultID = findCommand.ExecuteScalar() as long?;


                if (resultID.HasValue == false)
                {
                    SqliteCommand initializeCommand = connection.CreateCommand();
                    initializeCommand.CommandText = $"INSERT INTO Tags (ParentID, Name) VALUES({ToSQL(parentID)}, '{tag.Name}'); SELECT last_insert_rowid();";
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

        public IEnumerable<Tag> GetAllTags()
        {
            var result = new List<Tag>();
            using (var connection = Connection())
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

        #endregion
    }
}
