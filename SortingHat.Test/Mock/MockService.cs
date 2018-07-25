using SortingHat.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SortingHat.Test
{
    class MockService : IServices
    {
        public ILogger Logger { get; } = new MockLogger();
    }
}
