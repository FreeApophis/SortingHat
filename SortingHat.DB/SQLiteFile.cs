using System;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using System.Threading.Tasks;

namespace SortingHat.DB
{
    [UsedImplicitly]
    public class SQLiteFile : IFile
    {
        private readonly Func<SearchQueryVisitor> _newSearchQueryVisitor;
        private readonly Func<File> _newFile;
        private readonly SQLiteProjectDatabase _db;
        private readonly Parser _parser;

        public SQLiteFile(Func<SearchQueryVisitor> newSearchQueryVisitor, Func<File> newFile, SQLiteProjectDatabase db, Parser parser)
        {
            _newSearchQueryVisitor = newSearchQueryVisitor;
            _newFile = newFile;
            _db = db;
            _parser = parser;
        }

        public async Task Tag(File file, Tag tag)
        {
            long? fileId = await FindOrCreate(file);
            var tagId = ((SQLiteTag)_db.Tag).FindOrCreate(tag);

            _db.ExecuteNonQuery("INSERT INTO FileTags (TagId, FileId) VALUES(@tagId, @fileId);", new SqliteParameter("@tagId", tagId), new SqliteParameter("@fileId", fileId));
        }

        private async Task<long?> Find(File file)
        {
            return _db.ExecuteScalar("SELECT Id FROM Files WHERE Hash = @hash", new SqliteParameter("@hash", await file.Hash)) as long?;
        }

        private async Task<long> Create(File file)
        {
            var fileId = await InsertFile(file);

            InsertFileName(file, fileId);
            InsertFilePath(file, fileId);

            return fileId;
        }

        private async Task<long> InsertFile(File file)
        {
            var hash = new SqliteParameter("@hash", await file.Hash);
            var size = new SqliteParameter("@size", file.Size);
            var createdAt = new SqliteParameter("@createdAt", file.CreatedAt);

            return (long)_db.ExecuteScalar("INSERT INTO Files (Hash, Size, CreatedAt) VALUES(@hash,@size, @createdAt); SELECT last_insert_rowid();", hash, size, createdAt);
        }

        private void InsertFileName(File file, long fileId)
        {
            var fileName = System.IO.Path.GetFileName(file.Path);
            _db.ExecuteNonQuery("INSERT INTO FileNames (FileId, Name) VALUES(@fileId, @fileName);", new SqliteParameter("@fileId", fileId), new SqliteParameter("@fileName", fileName));
        }

        private void InsertFilePath(File file, long fileId)
        {
            _db.ExecuteNonQuery("INSERT INTO FilePaths (FileId, Path) VALUES(@fileId, @filePath);", new SqliteParameter("@fileId", fileId), new SqliteParameter("@filePath", file.Path));
        }

        private async Task<long> FindOrCreate(File file)
        {
            long? fileId = await Find(file);

            if (fileId.HasValue)
            {
                return UpdateFileAttributes(file, fileId.Value);
            }

            return await Create(file);
        }

        private long UpdateFileAttributes(File file, long fileId)
        {
            InsertFileName(file, fileId);
            InsertFilePath(file, fileId);

            return fileId;
        }

        public async Task Untag(File file, Tag tag)
        {
            var fileId = await Find(file);
            var tagId = ((SQLiteTag)_db.Tag).Find(tag);

            if (fileId.HasValue && tagId.HasValue)
            {
                _db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId AND FileId = @fileId;", new SqliteParameter("@fileId", fileId), new SqliteParameter("@tagId", tagId));
            }
        }

        public IEnumerable<File> Search(string query)
        {
            return MetaSearch(ParseQuery(query));
        }

        public IEnumerable<File> GetDuplicates()
        {
            return MetaSearch(DuplicateQuery);
        }

        private IEnumerable<File> MetaSearch(string search)
        {
            var reader = _db.ExecuteReader(search);

            var files = new List<File>();

            while (AddFileEntry(reader, files)) { }

            return files;
        }

        private bool AddFileEntry(SqliteDataReader reader, List<File> files)
        {
            var file = _newFile();
            var hasEntry = LoadFileFromReader(reader, file);

            if (hasEntry)
            {
                files.Add(file);
            }

            return hasEntry;
        }

        private string ParseQuery(string query)
        {
            if (_parser.Parse(query) is { } ir)
            {
                var visitor = _newSearchQueryVisitor();
                ir.Accept(visitor);

                return visitor.Result;
            }

            return string.Empty;
        }

        private static bool LoadFileFromReader(DbDataReader reader, File file)
        {
            var hasMore = reader.Read();

            if (reader.HasRows && hasMore)
            {
                file.CreatedAt = reader.GetDateTime(0);
                file.Hash = Task.FromResult(reader.GetString(1));
                file.Size = reader.GetInt64(2);
                file.Path = reader.GetString(3);
            }

            return hasMore;
        }

        public bool LoadByPath(File file)
        {
            var reader = _db.ExecuteReader("SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path FROM Files JOIN FilePaths ON FilePaths.FileId = Files.Id WHERE FilePaths.Path = @filePath", new SqliteParameter("@filePath", file.Path));

            return LoadFileFromReader(reader, file);
        }

        private const string AllFileTags = @"SELECT Tags.Id
FROM Tags
JOIN FileTags ON FileTags.TagId = Tags.Id
JOIN Files ON FileTags.FileId = Files.Id
WHERE Files.Hash = @fileHash";

        public async Task<IEnumerable<Tag>> GetTags(File file)
        {
            var tags = new List<Tag>();


            var reader = _db.ExecuteReader(AllFileTags, new SqliteParameter("@fileHash", await file.Hash));

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

        public async Task<IEnumerable<string>> GetPaths(File file)
        {
            var paths = new List<string>();

            var reader = _db.ExecuteReader("SELECT FilePaths.Path FROM FilePaths JOIN Files ON FilePaths.FileId = Files.Id WHERE Files.Hash = @fileHash", new SqliteParameter("@fileHash", await file.Hash));

            while (reader.Read())
            {
                paths.Add(reader.GetString(0));
            }

            return paths;
        }

        public async Task<IEnumerable<string>> GetNames(File file)
        {
            var names = new List<string>();

            var reader = _db.ExecuteReader("SELECT FileNames.Name FROM FileNames JOIN Files ON FileNames.FileId = Files.Id WHERE Files.Hash = @fileHash", new SqliteParameter("@fileHash", await file.Hash));

            while (reader.Read())
            {
                names.Add(reader.GetString(0));
            }

            return names;
        }


        private const string DuplicateQuery = @"SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path, COUNT(FilePaths.Id) AS DuplicateCount
FROM FilePaths
JOIN Files ON FilePaths.FileId = Files.Id
GROUP BY FileId
HAVING DuplicateCount > 1";

    }
}
