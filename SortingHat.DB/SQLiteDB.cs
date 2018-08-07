using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using System;
using System.IO;

namespace SortingHat.DB
{
    public class SQLiteDB : IDatabase
    {
        private readonly string _path;
        private readonly string _dbName;

        public IFile File { get; }
        public ITag Tag { get; }

        public SQLiteDB(string path, string dbName)
        {
            _path = path;
            _dbName = dbName;

            File = new SQLiteFile(this);
            Tag = new SQLiteTag(this);
        }

        public void Setup()
        {
            var migrator = new RevisionMigrator(this);

            migrator.Initialize();
            migrator.Migrate();
        }

        public void TearDown()
        {
            throw new NotImplementedException();
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
            return Path.Combine(DBPath(), $"{_dbName}.db");
        }

        internal SqliteConnection Connection()
        {
            return new SqliteConnection($"Filename={DBFile()}");
        }
    }
}
