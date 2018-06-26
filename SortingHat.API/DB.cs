using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace SortingHat.API
{
    public class DB
    {
        private static string DBPath(string path)
        {
            string result = Path.Combine(path, ".hat");
            if (Directory.Exists(result) == false)
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        public static string DBFile(string path)
        {
            return Path.Combine(DBPath(path), "hat.db");
        }

        private static string FindExistingDBPath()
        {
            string path = Directory.GetCurrentDirectory();

            while (File.Exists(DBFile(path)) == false)
            {
                var directoryInfo = Directory.GetParent(path);
                if (directoryInfo == null)
                {
                    return null;
                }
                path = Directory.GetParent(path).FullName;
            }

            return path;
        }

        private static string ConnectionString(string file)
        {
            if (file == null)
            {
                var path = FindExistingDBPath();
                return path != null ? DBFile(path) : null;
            }
            return file;
        }

        public static SqliteConnection Connection(string file = null)
        {
            string connectionString = ConnectionString(file);

            if (connectionString == null)
            {
                Console.WriteLine("No Database found!");
                return null;
            }

            return new SqliteConnection($"Filename={connectionString}");
        }


        public static void Init()
        {
            var migrator = new RevisionMigrator();

            migrator.Initialize();
            migrator.Migrate();
        }
    }
}
