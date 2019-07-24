using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IMainDatabase : IDatabase
    {
        IProjectDatabase ProjectDatabase { get; }

        IReadOnlyCollection<string> ProjectDatabases { get; }

        ISettings Settings { get; }
    }
}