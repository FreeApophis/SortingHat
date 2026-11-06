using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB.Access;

public class SQLiteProjects(SQLiteMainDatabase db) : IProjects
{
    public IEnumerable<string> GetProjects()
    {
        var projects = new List<string>();
        using var reader = db.ExecuteReader("SELECT Name FROM Projects");

        while (reader.Read())
        {
            projects.Add(reader.GetString(0));
        }

        return projects;
    }

    public bool AddProject(string project)
    {
        db.ExecuteNonQuery("INSERT INTO [Projects] ([Name]) VALUES (@project)", new SqliteParameter("@project", project));

        return true;
    }

    public bool RemoveProject(string project)
    {
        db.ExecuteNonQuery("DELETE FROM [Projects] WHERE [NAME] = @project", new SqliteParameter("@project", project));

        return true;
    }
}