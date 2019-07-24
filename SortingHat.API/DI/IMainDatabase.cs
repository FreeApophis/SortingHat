using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IMainDatabase : IDatabase
    {
        IProjectDatabase ProjectDatabase { get; }

        IEnumerable<string> ProjectDatabases { get; }

        ISettings Settings { get; }
    }
}