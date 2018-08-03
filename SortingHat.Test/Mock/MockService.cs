using SortingHat.API.DI;
using System.Security.Cryptography;

namespace SortingHat.Test
{
    class MockService : IServices
    {
        public ILogger Logger { get; } = new MockLogger();
        public IDatabase DB { get; } = new MockDB();
        public HashService HashService { get; } = new HashService(SHA256.Create(), nameof(SHA256));
    }
}
