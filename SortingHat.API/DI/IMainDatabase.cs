using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IMainDatabase : IDatabase
    {
        IProjectDatabase ProjectDatabase { get; }

        IProjects Projects { get; }

        ISettings Settings { get; }
    }
}