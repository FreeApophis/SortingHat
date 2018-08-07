using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using System;
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

                long fileID = InsertFile(connection, file);
                InsertFileName(connection, file, fileID);
                InsertFilePath(connection, file, fileID);

                connection.Close();

                return fileID;
            }
        }

        private static long InsertFile(SqliteConnection connection, File file)
        {
            SqliteCommand createFileCommand = connection.CreateCommand();
            createFileCommand.CommandText = $"INSERT INTO Files (Hash, Size, CreatedAt) VALUES('{file.Hash}','{file.Size}','{file.CreatedAt}'); SELECT last_insert_rowid();";
            return (long)createFileCommand.ExecuteScalar();
        }

        private static void InsertFileName(SqliteConnection connection, File file, long fileID)
        {
            SqliteCommand createFileNameCommand = connection.CreateCommand();
            createFileNameCommand.CommandText = $"INSERT INTO FileNames (FileID, Name) VALUES('{fileID}','{System.IO.Path.GetFileName(file.Path)}');";
            createFileNameCommand.ExecuteNonQuery();
        }

        private static void InsertFilePath(SqliteConnection connection, File file, long fileID)
        {
            SqliteCommand createFilePathCommand = connection.CreateCommand();
            createFilePathCommand.CommandText = $"INSERT INTO FilePaths (FileID, Path) VALUES('{fileID}','{file.Path}');";
            createFilePathCommand.ExecuteNonQuery();
        }

        private long FindOrCreate(File file)
        {
            long? fileID = Find(file);

            if (fileID.HasValue)
            {
                return UpdateFileAttributes(file, fileID.Value);
            }

            return Create(file);
        }

        private long UpdateFileAttributes(File file, long fileID)
        {
            using (var connection = _db.Connection())
            {
                connection.Open();

                InsertFileName(connection, file, fileID);
                InsertFilePath(connection, file, fileID);

                connection.Close();

                return fileID;
            }
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
                initializeCommand.CommandText = ParseQuery(query);
                var resultID = (long)initializeCommand.ExecuteScalar();
                connection.Close();
            }

            return Enumerable.Empty<File>();
        }

        private string ParseQuery(string query)
        {
            var parser = new QueryParser(query);
            var ir = parser.Parse();

            var visitor = new SearchQueryVisitor();
            ir.Accept(visitor);

            return visitor.Result;
        }
    }
}
