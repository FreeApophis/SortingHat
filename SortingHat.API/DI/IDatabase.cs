using System;
using System.Collections.Generic;
using System.Text;
using SortingHat.API.Models;

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

        IFile File { get; }
        ITag Tag { get; }
    }
}
