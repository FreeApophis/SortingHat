using Microsoft.Data.Sqlite;
using SortingHat.API.Models;
using System.IO;
using System;

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
    }
}
