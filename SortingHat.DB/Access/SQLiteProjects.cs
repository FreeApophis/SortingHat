using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SortingHat.API.DI;

namespace SortingHat.DB.Access
{
    public class SQLiteProjects : IProjects
    {
        private readonly SQLiteMainDatabase _db;

        public SQLiteProjects(SQLiteMainDatabase db)
        {
            _db = db;
        }

        public IEnumerable<string> GetProjects()
        {
            var projects = new List<string>();
            var reader = _db.ExecuteReader("SELECT Name FROM Projects");

            while (reader.Read())
            {
                projects.Add(reader.GetString(0));
            }

            return projects;
        }

        public bool AddProject(string project)
        {
            _db.ExecuteNonQuery("INSERT INTO [Projects] ([Name]) VALUES (@project)", new SqliteParameter("@project", project));

            return true;
        }

        public bool RemoveProject(string project)
        {
            throw new NotImplementedException();
        }
    }
}