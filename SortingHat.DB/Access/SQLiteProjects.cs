using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky;
using Funcky.Monads;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;
using File = System.IO.File;

namespace SortingHat.DB.Access;

[UsedImplicitly]
internal sealed class SQLiteProjects(SQLiteMainDatabase db, DatabaseSettings databaseSettings, IRevisionMigrator migrator) : IProjects
{
    public IReadOnlyList<string> GetProjects()
        => db.Query("SELECT Name FROM Projects", reader => reader.GetString(0));

    public void AddProject(ProjectName project)
        => db.ExecuteNonQuery("INSERT INTO [Projects] ([Name]) VALUES (@project)", new SqliteParameter("@project", project.Value));

    /// <summary>
    /// Deletes the project's database file first: when that fails, the project is still listed instead of being half removed.
    /// </summary>
    public void RemoveProject(ProjectName project)
    {
        File.Delete(DatabaseFile(project));
        db.ExecuteNonQuery("DELETE FROM [Projects] WHERE [NAME] = @project", new SqliteParameter("@project", project.Value));
    }

    public Either<string, Unit> Export(ProjectName project, string destination)
        => File.Exists(DatabaseFile(project))
            ? DatabaseCopy.Copy(DatabaseFile(project), destination)
            : Either<string, Unit>.Left($"The project '{project}' has no database yet, there is nothing to export.");

    /// <summary>The copy is registered last: when registering fails, the copy is deleted again.</summary>
    public Either<string, Unit> Import(string projectFile, ProjectName project)
        => from known in KnownProject(projectFile)
           from free in NoDatabaseFile(project)
           from copied in DatabaseCopy.Copy(projectFile, DatabaseFile(project))
           from added in Register(project)
           select added;

    private static Either<string, IReadOnlyList<string>> Revisions(string projectFile)
    {
        try
        {
            using var connection = DatabaseCopy.OpenReadOnly(projectFile);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT [Name] FROM [Revisions]";
            using var reader = command.ExecuteReader();

            var revisions = new List<string>();
            while (reader.Read())
            {
                revisions.Add(reader.GetString(0));
            }

            return revisions;
        }
        catch (SqliteException)
        {
            return Either<string, IReadOnlyList<string>>.Left($"'{projectFile}' is no SortingHat project.");
        }
    }

    /// <summary>A project file records the revisions of a project database; this hat opens it when it knows all of them, older ones are migrated.</summary>
    private Either<string, Unit> KnownProject(string projectFile)
        => File.Exists(projectFile)
            ? Revisions(projectFile).SelectMany(revisions => Known(projectFile, revisions))
            : Either<string, Unit>.Left($"There is no file '{projectFile}'.");

    private Either<string, Unit> Known(string projectFile, IReadOnlyList<string> revisions)
    {
        var known = migrator.Revisions(MigrationType.Project);

        return revisions.Any(known.Contains) switch
        {
            false => Either<string, Unit>.Left($"'{projectFile}' is no SortingHat project."),
            true when revisions.Any(revision => !known.Contains(revision)) => Either<string, Unit>.Left($"'{projectFile}' is a project of a newer hat, update hat to import it."),
            true => Unit.Value,
        };
    }

    private Either<string, Unit> NoDatabaseFile(ProjectName project)
        => File.Exists(DatabaseFile(project))
            ? Either<string, Unit>.Left($"There is already a database file for '{project}': {DatabaseFile(project)}. Remove it, or import under another name.")
            : Unit.Value;

    private Either<string, Unit> Register(ProjectName project)
    {
        try
        {
            AddProject(project);

            return Unit.Value;
        }
        catch (SqliteException exception)
        {
            File.Delete(DatabaseFile(project));

            return Either<string, Unit>.Left($"The project '{project}' could not be registered: {exception.Message}");
        }
    }

    private string DatabaseFile(ProjectName project)
        => Path.Combine(databaseSettings.DbPath, $"{project.Value}.db");
}
