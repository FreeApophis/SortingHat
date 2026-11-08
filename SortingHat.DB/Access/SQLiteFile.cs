using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;
using SortingHat.API.Models;
using SortingHat.API.Parser;
using SortingHat.API.Parser.Nodes;
using static Funcky.Functional;
using File = SortingHat.API.Models.File;

namespace SortingHat.DB.Access;

[UsedImplicitly]
internal sealed class SQLiteFile(SQLiteProjectDatabase db, SQLiteTag sqLiteTag, Parser parser) : IFile
{
    private const string AllFileTags = @"SELECT Tags.Id
FROM Tags
JOIN FileTags ON FileTags.TagId = Tags.Id
JOIN Files ON FileTags.FileId = Files.Id
WHERE Files.Hash = @hash";

    private const string DuplicateQuery = @"SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path, COUNT(FilePaths.Id) AS DuplicateCount
FROM FilePaths
JOIN Files ON FilePaths.FileId = Files.Id
GROUP BY FileId
HAVING DuplicateCount > 1";

    private const string FindByPath = @"SELECT Files.CreatedAt, Files.Hash, Files.Size, FilePaths.Path, FilePaths.Size, FilePaths.ModifiedAt, FilePaths.VerifiedAt
FROM Files
JOIN FilePaths ON FilePaths.FileId = Files.Id
WHERE FilePaths.Path = @filePath";

    public Option<FileAt> Find(string path)
        => db.Query(FindByPath, reader => new FileAt(ReadFile(reader), reader.GetString(3), ReadFingerprint(reader)), new SqliteParameter("@filePath", path))
            .FirstOrNone();

    public void Tag(FileAt file, Tag tag)
        => db.InTransaction(() => InsertFileTag(FindOrCreate(file), sqLiteTag.FindOrCreate(tag)));

    public void Untag(File file, Tag tag)
        => (from fileId in FindId(file.Hash)
            from tagId in sqLiteTag.Find(tag)
            select (FileId: fileId, TagId: tagId))
            .AndThen(ids => db.ExecuteNonQuery("DELETE FROM FileTags WHERE TagId = @tagId AND FileId = @fileId;", new SqliteParameter("@fileId", ids.FileId), new SqliteParameter("@tagId", ids.TagId)));

    public void MovePath(FileAt file, string destinationPath)
        => db.ExecuteNonQuery(
            "UPDATE FilePaths SET Path = @destinationPath WHERE Path = @sourcePath AND FileId = (SELECT Id FROM Files WHERE Hash = @hash)",
            new SqliteParameter("@destinationPath", destinationPath),
            new SqliteParameter("@sourcePath", file.Path),
            HashParameter(file.File.Hash));

    public void RemovePaths(IEnumerable<string> paths)
        => db.InTransaction(() =>
        {
            paths.ForEach(path => db.ExecuteNonQuery("DELETE FROM FilePaths WHERE Path = @path", new SqliteParameter("@path", path)));

            return true;
        });

    public void Store(FileAt file)
        => db.InTransaction(() => FindOrCreate(file));

    public Either<string, SearchResult> Search(string query)
        => parser.Parse(query).Match(
            left: error => Either<string, SearchResult>.Left(error.Message),
            right: expression => Either<string, SearchResult>.Right(Search(expression)));

    public IReadOnlyList<File> GetDuplicates()
        => db.Query(DuplicateQuery, ReadFile);

    public IReadOnlyList<Tag> GetTags(File file)
        => db.Query(AllFileTags, reader => sqLiteTag.Load(reader.GetInt64(0)), HashParameter(file.Hash));

    public IReadOnlyList<string> GetPaths()
        => db.Query("SELECT FilePaths.Path FROM FilePaths", reader => reader.GetString(0));

    public IReadOnlyList<string> GetPaths(File file)
        => db.Query("SELECT FilePaths.Path FROM FilePaths JOIN Files ON FilePaths.FileId = Files.Id WHERE Files.Hash = @hash", reader => reader.GetString(0), HashParameter(file.Hash));

    public IReadOnlyList<string> GetNames(File file)
        => db.Query("SELECT FileNames.Name FROM FileNames JOIN Files ON FileNames.FileId = Files.Id WHERE Files.Hash = @hash", reader => reader.GetString(0), HashParameter(file.Hash));

    /// <summary>Reads the columns CreatedAt, Hash and Size. CreatedAt is written as UTC, SQLite keeps no kind, so it is read back as UTC.</summary>
    private static File ReadFile(SqliteDataReader reader)
        => new(ReadHash(reader.GetString(1)), reader.GetInt64(2), DateTime.SpecifyKind(reader.GetDateTime(0), DateTimeKind.Utc));

    /// <summary>Every hash in the database was written from a <see cref="Hash"/>, so one that does not parse means the database is damaged.</summary>
    private static Hash ReadHash(string text)
        => Hash.Parse(text).GetOrElse(problem => throw new InvalidDataException($"The database is damaged: {problem}"));

    private static SqliteParameter HashParameter(Hash hash)
        => new("@hash", hash.ToString());

    /// <summary>Paths stored before fingerprints existed have none, so their files are hashed again once.</summary>
    private static Option<PathFingerprint> ReadFingerprint(SqliteDataReader reader)
        => reader.IsDBNull(4) || reader.IsDBNull(5) || reader.IsDBNull(6)
            ? Option<PathFingerprint>.None
            : Option.Some(new PathFingerprint(reader.GetInt64(4), new DateTime(reader.GetInt64(5), DateTimeKind.Utc), new DateTime(reader.GetInt64(6), DateTimeKind.Utc)));

    private static SqliteParameter[] FingerprintParameters(Option<PathFingerprint> fingerprint)
        =>
        [
            new("@size", fingerprint.Match(none: DBNull.Value, some: f => (object)f.Size)),
            new("@modifiedAt", fingerprint.Match(none: DBNull.Value, some: f => (object)f.ModifiedAtUtc.Ticks)),
            new("@verifiedAt", fingerprint.Match(none: DBNull.Value, some: f => (object)f.TakenAtUtc.Ticks)),
        ];

    private Option<long> FindId(Hash hash)
        => Option.FromNullable(db.ExecuteScalar("SELECT Id FROM Files WHERE Hash = @hash", HashParameter(hash)) as long?);

    private long Create(FileAt file)
    {
        var fileId = InsertFile(file.File);

        InsertFileName(file, fileId);
        InsertFilePath(file, fileId);

        return fileId;
    }

    private long InsertFile(File file)
        => db.ExecuteScalar(
            "INSERT INTO Files (Hash, Size, CreatedAt) VALUES(@hash,@size, @createdAt); SELECT last_insert_rowid();",
            HashParameter(file.Hash),
            new SqliteParameter("@size", file.Size),
            new SqliteParameter("@createdAt", file.CreatedAt)) as long?
            ?? throw new InvalidOperationException("Inserting the file did not return its id.");

    /// <summary>Tagging a file again keeps its existing row, the table's ON CONFLICT REPLACE would delete and re-insert it.</summary>
    private bool InsertFileTag(long fileId, long tagId)
    {
        db.ExecuteNonQuery("INSERT OR IGNORE INTO FileTags (TagId, FileId) VALUES(@tagId, @fileId);", new SqliteParameter("@tagId", tagId), new SqliteParameter("@fileId", fileId));

        return true;
    }

    private void InsertFileName(FileAt file, long fileId)
        => db.ExecuteNonQuery(
            "INSERT OR IGNORE INTO FileNames (FileId, Name) VALUES(@fileId, @fileName);",
            new SqliteParameter("@fileId", fileId),
            new SqliteParameter("@fileName", Path.GetFileName(file.Path)));

    /// <summary>
    /// A path holds one content at a time: when an edited file is stored, its path leaves the old content, and the tags follow the path
    /// (the new content gets a copy, the old content keeps its own). The fingerprint is stored with the path, or updated when the path is already known.
    /// </summary>
    private void InsertFilePath(FileAt file, long fileId)
    {
        db.ExecuteNonQuery(
            "INSERT OR IGNORE INTO FileTags (TagId, FileId) SELECT TagId, @fileId FROM FileTags WHERE FileId IN (SELECT FileId FROM FilePaths WHERE Path = @filePath AND FileId <> @fileId);",
            new SqliteParameter("@fileId", fileId),
            new SqliteParameter("@filePath", file.Path));
        db.ExecuteNonQuery("DELETE FROM FilePaths WHERE Path = @filePath AND FileId <> @fileId;", new SqliteParameter("@fileId", fileId), new SqliteParameter("@filePath", file.Path));
        db.ExecuteNonQuery(
            """
            INSERT INTO FilePaths (FileId, Path, Size, ModifiedAt, VerifiedAt) VALUES(@fileId, @filePath, @size, @modifiedAt, @verifiedAt)
            ON CONFLICT (FileId, Path) DO UPDATE SET Size = excluded.Size, ModifiedAt = excluded.ModifiedAt, VerifiedAt = excluded.VerifiedAt;
            """,
            [new SqliteParameter("@fileId", fileId), new SqliteParameter("@filePath", file.Path), .. FingerprintParameters(file.Fingerprint)]);
    }

    private long FindOrCreate(FileAt file)
        => FindId(file.File.Hash).Match(none: () => Create(file), some: fileId => UpdateFileAttributes(file, fileId));

    private long UpdateFileAttributes(FileAt file, long fileId)
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

        return new SearchResult(db.Query(sql, reader => new FileAt(ReadFile(reader), reader.GetString(3), Option<PathFingerprint>.None), [.. knownTags.Select(tag => tag.Parameter)]), unknownTags);
    }

    private Option<long> FindTag(string name)
        => from tag in API.Models.Tag.Parse(name).RightOrNone()
           from id in sqLiteTag.Find(tag)
           select id;
}
