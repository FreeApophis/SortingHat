using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.DB.Access;

public class SQLiteProjects(SQLiteMainDatabase db, DatabaseSettings databaseSettings) : IProjects
{
    public IEnumerable<string> GetProjects()
        => db.Query("SELECT Name FROM Projects", reader => reader.GetString(0));

    public bool AddProject(ProjectName project)
    {
        db.ExecuteNonQuery("INSERT INTO [Projects] ([Name]) VALUES (@project)", new SqliteParameter("@project", project.Value));

        return true;
    }

    /// <summary>
    /// Deletes the project's database file first: when that fails, the project is still listed instead of being half removed.
    /// </summary>
    public bool RemoveProject(string project)
    {
        System.IO.File.Delete(System.IO.Path.Combine(databaseSettings.DbPath, $"{project}.db"));
        db.ExecuteNonQuery("DELETE FROM [Projects] WHERE [NAME] = @project", new SqliteParameter("@project", project));

        return true;
    }
}