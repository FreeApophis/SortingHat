using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using static Funcky.Functional;

namespace SortingHat.DB.Access;

[UsedImplicitly]
public class SQLiteFile(Func<File> newFile, SQLiteProjectDatabase db, SQLiteTag sqLiteTag, Parser parser, ITagParser tagParser) : IFile
{
    private const string AllFileTags = @"SELECT Tags.Id
FROM Tags
JOIN FileTags ON FileTags.TagId = Tags.Id
JOIN Files ON FileTags.FileId = Files.Id
WHERE Files.Hash = @fileHash";

    private const string DuplicateQuery = @"SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path, COUNT(FilePaths.Id) AS DuplicateCount
FROM FilePaths
JOIN Files ON FilePaths.FileId = Files.Id
GROUP BY FileId
HAVING DuplicateCount > 1";

    public async Task Tag(File file, Tag tag)
    {
        var hash = await file.Hash;

        db.InTransaction(() => InsertFileTag(FindOrCreate(file, hash), sqLiteTag.FindOrCreate(tag)));
    }

    public async Task Untag(File file, Tag tag)
    {
        var fileId = Find(await file.Hash);

        (from f in fileId
         from t in sqLiteTag.Find(tag)
         select (FileId: f, TagId: t))
            .AndThen(ids => db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId AND FileId = @fileId;", new SqliteParameter("@fileId", ids.FileId), new SqliteParameter("@tagId", ids.TagId)));
    }

    public bool Store(File file)
    {
        FindOrCreate(file).Wait();

        return true;
    }

    public SearchResult Search(string query)
        => parser.Parse(query) is { } expression
            ? Search(expression)
            : new SearchResult([], []);

    public IEnumerable<File> GetDuplicates()
    {
        return MetaSearch(DuplicateQuery);
    }

    public bool LoadByPath(File file)
    {
        return db.ExecuteReader(
            "SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path FROM Files JOIN FilePaths ON FilePaths.FileId = Files.Id WHERE FilePaths.Path = @filePath",
            reader => LoadFileFromReader(reader, file),
            new SqliteParameter("@filePath", file.Path));
    }

    public async Task<IEnumerable<Tag>> GetTags(File file)
        => db.Query(AllFileTags, reader => sqLiteTag.Load(reader.GetInt64(0)), new SqliteParameter("@fileHash", await file.Hash));

    public IEnumerable<string> GetPaths()
        => db.Query("SELECT FilePaths.Path FROM FilePaths", reader => reader.GetString(0));

    public async Task<IEnumerable<string>> GetPaths(File file)
        => db.Query("SELECT FilePaths.Path FROM FilePaths JOIN Files ON FilePaths.FileId = Files.Id WHERE Files.Hash = @fileHash", reader => reader.GetString(0), new SqliteParameter("@fileHash", await file.Hash));

    public async Task<IEnumerable<string>> GetNames(File file)
        => db.Query("SELECT FileNames.Name FROM FileNames JOIN Files ON FileNames.FileId = Files.Id WHERE Files.Hash = @fileHash", reader => reader.GetString(0), new SqliteParameter("@fileHash", await file.Hash));

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

    private Option<long> Find(string hash)
        => Option.FromNullable(db.ExecuteScalar("SELECT Id FROM Files WHERE Hash = @hash", new SqliteParameter("@hash", hash)) as long?);

    private long Create(File file, string hash)
    {
        var fileId = InsertFile(file, hash);

        InsertFileName(file, fileId);
        InsertFilePath(file, fileId);

        return fileId;
    }

    private long InsertFile(File file, string hash)
        => db.ExecuteScalar(
            "INSERT INTO Files (Hash, Size, CreatedAt) VALUES(@hash,@size, @createdAt); SELECT last_insert_rowid();",
            new SqliteParameter("@hash", hash),
            new SqliteParameter("@size", file.Size),
            new SqliteParameter("@createdAt", file.CreatedAt)) as long?
            ?? throw new InvalidOperationException("Inserting the file did not return its id.");

    /// <summary>Tagging a file again keeps its existing row, the table's ON CONFLICT REPLACE would delete and re-insert it.</summary>
    private bool InsertFileTag(long fileId, long tagId)
    {
        db.ExecuteNonQuery("INSERT OR IGNORE INTO FileTags (TagId, FileId) VALUES(@tagId, @fileId);", new SqliteParameter("@tagId", tagId), new SqliteParameter("@fileId", fileId));

        return true;
    }

    private void InsertFileName(File file, long fileId)
    {
        var fileName = System.IO.Path.GetFileName(file.Path);
        db.ExecuteNonQuery("INSERT OR IGNORE INTO FileNames (FileId, Name) VALUES(@fileId, @fileName);", new SqliteParameter("@fileId", fileId), new SqliteParameter("@fileName", fileName));
    }

    private void InsertFilePath(File file, long fileId)
    {
        db.ExecuteNonQuery("INSERT OR IGNORE INTO FilePaths (FileId, Path) VALUES(@fileId, @filePath);", new SqliteParameter("@fileId", fileId), new SqliteParameter("@filePath", file.Path));
    }

    private async Task<long> FindOrCreate(File file)
    {
        var hash = await file.Hash;

        return db.InTransaction(() => FindOrCreate(file, hash));
    }

    private long FindOrCreate(File file, string hash)
        => Find(hash).Match(none: () => Create(file, hash), some: fileId => UpdateFileAttributes(file, fileId));

    private long UpdateFileAttributes(File file, long fileId)
    {
        InsertFileName(file, fileId);
        InsertFilePath(file, fileId);

        return fileId;
    }

    private SearchResult Search(IParseNode expression)
    {
        var tags = SearchQuery.TagNames(expression)
            .Distinct()
            .Select(name => (Name: name, Id: FindTag(name)))
            .ToList();

        var knownTags = tags
            .WhereSelect(tag => tag.Id.Select(id => (tag.Name, Id: id)))
            .Select((tag, index) => (tag.Name, Parameter: new SqliteParameter($"@tag{index}", tag.Id)))
            .ToList();

        var unknownTags = tags
            .Where(tag => tag.Id.Match(none: true, some: False))
            .Select(tag => tag.Name)
            .ToList();

        var sql = SearchQuery.Sql(expression, knownTags.ToDictionary(tag => tag.Name, tag => tag.Parameter.ParameterName));

        return new SearchResult(db.ExecuteReader(sql, ReadFiles, [.. knownTags.Select(tag => tag.Parameter)]), unknownTags);
    }

    private Option<long> FindTag(string name)
        => from tag in Option.FromNullable(tagParser.Parse(name))
           from id in sqLiteTag.Find(tag)
           select id;

    private IEnumerable<File> MetaSearch(string search)
        => db.ExecuteReader(search, ReadFiles);

    private List<File> ReadFiles(SqliteDataReader reader)
    {
        var files = new List<File>();

        while (AddFileEntry(reader, files))
        {
        }

        return files;
    }

    private bool AddFileEntry(SqliteDataReader reader, List<File> files)
    {
        var file = newFile();
        var hasEntry = LoadFileFromReader(reader, file);

        if (hasEntry)
        {
            files.Add(file);
        }

        return hasEntry;
    }
}
