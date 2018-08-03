using Microsoft.Data.Sqlite;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.IO;

namespace SortingHat.DB
{
    public class SQLiteDB : API.DI.IDatabase
    {
        private string _path;

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

        public void StoreTag(Tag tag)
        {
            if (tag.Parent != null)
            {
                StoreTag(tag.Parent);
            }

            using (var connection = Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = $"INSERT INTO Tags (ParentID, Name) VALUES(NULL, '{tag.Name}'); ";
                initializeCommand.ExecuteNonQuery();

                connection.Close();
            }
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
    }
}
