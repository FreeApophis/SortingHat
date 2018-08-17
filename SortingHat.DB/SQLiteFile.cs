﻿using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using System.Collections.Generic;


namespace SortingHat.DB
{
    class SQLiteFile : IFile
    {
        readonly SQLiteDB _db;

        public SQLiteFile(SQLiteDB db)
        {
            _db = db;
        }

        public void Tag(File file, Tag tag)
        {
            var fileID = FindOrCreate(file);
            var tagID = ((SQLiteTag)_db.Tag).FindOrCreate(tag);

            _db.ExecuteNonQuery("INSERT INTO FileTags (TagID, FileID) VALUES(@tagID, @fileID);", new SqliteParameter("@tagID", tagID), new SqliteParameter("@fileID", fileID));
        }

        private long? Find(File file)
        {
            return _db.ExecuteScalar("SELECT ID FROM Files WHERE Hash = @hash", new SqliteParameter("@hash", file.Hash)) as long?;
        }

        private long Create(File file)
        {

            var fileID = InsertFile(file);
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
            var fileName = System.IO.Path.GetFileName(file.Path);
            _db.ExecuteNonQuery("INSERT INTO FileNames (FileID, Name) VALUES(@fileID, @fileName);", new SqliteParameter("@fileID", fileID), new SqliteParameter("@fileName", fileName));
        }

        private void InsertFilePath(File file, long fileID)
        {
            _db.ExecuteNonQuery("INSERT INTO FilePaths (FileID, Path) VALUES(@fileID, @filePath);", new SqliteParameter("@fileID", fileID), new SqliteParameter("@filePath", file.Path));
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
            var fileID = Find(file);
            var tagID = ((SQLiteTag)_db.Tag).Find(tag);

            if (fileID.HasValue && tagID.HasValue)
            {
                _db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagID = @tagID AND FileID = @fileID;", new SqliteParameter("@fileID", fileID), new SqliteParameter("@tagID", tagID));
            }
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


        public void Load(File file)
        {
            if (LoadByPath(file) == false)
            {
                //if (System.IO.File.Exists(file.Path))
                //{
                //    file.Hash = hashService.GetHash(file.Path);
                //    LoadByHash(file);
                //}
            }
        }

        private bool LoadFileFromReader(SqliteDataReader reader, File file)
        {
            if (reader.HasRows && reader.Read())
            {
                file.CreatedAt = reader.GetDateTime(0);
                file.Hash = reader.GetString(1);
                file.Size = reader.GetInt64(2);
            }

            return false;
        }

        private bool LoadByHash(File file)
        {
            var reader = _db.ExecuteReader("SELECT Files.CreatedAt, Files.Hash, Files.Size FROM Files WHERE Files.Hash= @fileHash", new SqliteParameter("@fileHash", file.Hash));

            return LoadFileFromReader(reader, file);
        }

        private bool LoadByPath(File file)
        {
            var reader = _db.ExecuteReader("SELECT Files.CreatedAt, Files.Hash, Files.Size FROM Files JOIN FilePaths ON FilePaths.FileID = Files.ID WHERE FilePaths.Path = @filePath", new SqliteParameter("@filePath", file.Path));

            return LoadFileFromReader(reader, file);
        }

        const string AllFileTags = @"SELECT Tags.ID
FROM Tags
JOIN FileTags ON FileTags.TagID = Tags.ID
JOIN Files ON FileTags.FileID = Files.ID
WHERE Files.Hash = @fileHash";

        public IEnumerable<Tag> GetTags(File file)
        {
            var tags = new List<Tag>();


            var reader = _db.ExecuteReader(AllFileTags, new SqliteParameter("@fileHash", file.Hash));

            while (reader.Read())
            {
                Tag tag = ((SQLiteTag)_db.Tag).Load(reader.GetInt64(0));
                tags.Add(tag);
            }

            return tags;
        }

        public IEnumerable<string> GetPaths()
        {
            var paths = new List<string>();

            var reader = _db.ExecuteReader("SELECT FilePaths.Path FROM FilePaths");

            while (reader.Read())
            {
                paths.Add(reader.GetString(0));
            }

            return paths;
        }

        public IEnumerable<string> GetPaths(File file)
        {
            var paths = new List<string>();

            var reader = _db.ExecuteReader("SELECT FilePaths.Path FROM FilePaths JOIN Files ON FilePaths.FileID = Files.ID WHERE Files.Hash = @fileHash", new SqliteParameter("@fileHash", file.Hash));

            while (reader.Read())
            {
                paths.Add(reader.GetString(0));
            }

            return paths;
        }

        public IEnumerable<string> GetNames(File file)
        {
            var names = new List<string>();

            var reader = _db.ExecuteReader("SELECT FileNames.Name FROM FileNames JOIN Files ON FileNames.FileID = Files.ID WHERE Files.Hash = @fileHash", new SqliteParameter("@fileHash", file.Hash));

            while (reader.Read())
            {
                names.Add(reader.GetString(0));
            }

            return names;
        }
    }
}
