using SortingHat.API.DI;
using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.Test
{
    class MockService : IServices
    {
        public ILogger Logger { get; } = new MockLogger();
        public IDatabase DB { get; } = new MockDB();
        public IHashService HashService { get; } = null;
    }
}
