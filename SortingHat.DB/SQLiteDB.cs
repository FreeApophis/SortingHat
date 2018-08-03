using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System;
using System.Collections.Generic;
using System.IO;

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

        private long FindOrCreateFile(API.Models.File file)
        {
            return 0;
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

        public IEnumerable<Tag> GetAllTags()
        {
            var result = new List<Tag>();
            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = $"SELECT Name FROM Tags ORDER BY Name;";
                var reader = initializeCommand.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(new Tag(reader.GetString(0)));
                }

                connection.Close();
            }

            return result;
        }

        #endregion
    }
}
