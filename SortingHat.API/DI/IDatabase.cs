using System.Collections.Generic;

namespace SortingHat.API.DI
{
    public interface IDatabase
    {
        /// <summary>
        /// Creates the necessary Database and tables...
        /// </summary>
        void Setup();

        /// <summary>
        /// Destroys or deletes the Database and tables...
        /// </summary>
        void TearDown();

        Dictionary<string, long> GetStatistics();

        IFile File { get; }
        ITag Tag { get; }
    }
}
