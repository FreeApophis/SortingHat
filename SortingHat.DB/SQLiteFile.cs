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

            _db.ExecuteNonQuery($"INSERT INTO FileTags (TagID, FileID) VALUES(@tagID, @fileID);", new SqliteParameter("@tagID", fileID), new SqliteParameter("@fileID", tagID));
        }

        private long? Find(File file)
        {
            return _db.ExecuteScalar("SELECT ID FROM Files WHERE Hash = @hash", new SqliteParameter("@hash", file.Hash)) as long?;
        }

        private long Create(File file)
        {

            long fileID = InsertFile(file);
            InsertFileName(file, fileID);
            InsertFilePath(file, fileID);

            return fileID;
        }

        private long InsertFile(File file)
        {
            var hash = new SqliteParameter("@hash", file.Hash);
            var size = new SqliteParameter("@size", file.Size);
            var createdAt = new SqliteParameter("@createdAt", file.CreatedAt);

            return (long)_db.ExecuteScalar("INSERT INTO Files (Hash, Size, CreatedAt) VALUES(@hash,@size, @createdAt); SELECT last_insert_rowid();", hash, size, createdAt);
        }

        private void InsertFileName(File file, long fileID)
        {
            _db.ExecuteNonQuery($"INSERT INTO FileNames (FileID, Name) VALUES('{fileID}','{System.IO.Path.GetFileName(file.Path)}');");
        }

        private void InsertFilePath(File file, long fileID)
        {
            _db.ExecuteNonQuery($"INSERT INTO FilePaths (FileID, Path) VALUES('{fileID}','{file.Path}');");
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

            InsertFileName(file, fileID);
            InsertFilePath(file, fileID);

            return fileID;
        }

        public void Untag(File file, Tag tag)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<File> Search(string query)
        {
            var reader = _db.ExecuteReader(ParseQuery(query));
            var files = new List<File>();

            while (reader.Read())
            {
                files.Add(new File(reader.GetString(0), reader.GetString(1)));
            }

            return files;
        }

        private string ParseQuery(string query)
        {
            var parser = new QueryParser(query);
            var ir = parser.Parse();

            var visitor = new SearchQueryVisitor(_db);
            ir.Accept(visitor);

            return visitor.Result;
        }
    }
}
