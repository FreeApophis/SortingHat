using System.IO;
using Funcky;
using Funcky.Monads;
using Microsoft.Data.Sqlite;

namespace SortingHat.DB;

/// <summary>Copies SQLite database files through SQLite, never as plain files.</summary>
internal static class DatabaseCopy
{
    /// <summary>
    /// Writes a consistent, compact copy of <paramref name="source"/> to <paramref name="destination"/> with <c>VACUUM INTO</c>,
    /// also while the source is in use or has changes still in its write-ahead log. An existing destination is never overwritten,
    /// and a destination left behind by a failed copy is deleted.
    /// </summary>
    public static Either<string, Unit> Copy(string source, string destination)
        => File.Exists(destination) || Directory.Exists(destination)
            ? Either<string, Unit>.Left($"'{destination}' already exists, choose a new file.")
            : CopyInto(source, destination);

    /// <summary>A connection that cannot change <paramref name="file"/>.</summary>
    public static SqliteConnection OpenReadOnly(string file)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = file, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        connection.Open();

        return connection;
    }

    private static Either<string, Unit> CopyInto(string source, string destination)
    {
        try
        {
            using var connection = OpenReadOnly(source);
            using var command = connection.CreateCommand();
            command.CommandText = "VACUUM INTO @destination";
            command.Parameters.AddWithValue("@destination", destination);
            command.ExecuteNonQuery();

            return Unit.Value;
        }
        catch (SqliteException exception)
        {
            File.Delete(destination);

            return Either<string, Unit>.Left($"'{source}' could not be copied to '{destination}': {exception.Message}");
        }
    }
}
