using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using System.Collections.Generic;
using System.Linq;

namespace SortingHat.DB
{
    class SQLiteFile : IFile
    {
        SQLiteDB _db;

        public SQLiteFile(SQLiteDB db)
        {
            _db = db;
        }

        public void Tag(File file, Tag tag)
        {
            long fileID = FindOrCreate(file);
            long tagID = ((SQLiteTag)_db.Tag).FindOrCreate(tag);

            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand tagCommand = connection.CreateCommand();
                tagCommand.CommandText = $"INSERT INTO FileTags (TagID, FileID) VALUES({tagID},{fileID});";
                tagCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        private long? Find(File file)
        {
            long? resultID;
            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand findCommand = connection.CreateCommand();
                findCommand.CommandText = $"SELECT ID FROM Files WHERE Hash = '{file.Hash}'";
                resultID = findCommand.ExecuteScalar() as long?;

                connection.Close();
            }

            return resultID;
        }
        private long Create(File file)
        {
            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = $"INSERT INTO Files (Hash, Size, CreatedAt) VALUES('{file.Hash}','{file.Size}','{file.CreatedAt}'); SELECT last_insert_rowid();";
                var resultID = (long)initializeCommand.ExecuteScalar();
                connection.Close();

                return resultID;
            }
        }

        private long FindOrCreate(API.Models.File file)
        {
            long? fileID = Find(file);

            if (fileID.HasValue)
            {
                return fileID.Value;
            }

            return Create(file);
        }

        public void Untag(File file, Tag tag)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<File> Search(string query)
        {
            using (var connection = _db.Connection())
            {
                connection.Open();

                SqliteCommand initializeCommand = connection.CreateCommand();
                initializeCommand.CommandText = $"TODO";
                var resultID = (long)initializeCommand.ExecuteScalar();
                connection.Close();
            }


            return Enumerable.Empty<File>();
        }
    }
}
