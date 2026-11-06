using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB.Access;

public class SQLiteProjects(SQLiteMainDatabase db) : IProjects
{
    public IEnumerable<string> GetProjects()
        => db.Query("SELECT Name FROM Projects", reader => reader.GetString(0));

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