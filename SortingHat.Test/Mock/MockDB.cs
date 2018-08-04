using System.Collections.Generic;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Test
{
    internal class MockDB : IDatabase
    {
        public IFile File => throw new System.NotImplementedException();

        public ITag Tag => throw new System.NotImplementedException();

        public void Setup()
        {
            throw new System.NotImplementedException();
        }

        public void TearDown()
        {
            throw new System.NotImplementedException();
        }
    }
}