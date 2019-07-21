using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SortingHat.ConsoleWriter;

namespace SortingHat.API.DI
{
    public class HashService : IHashService
    {
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly IConsoleWriter _consoleWriter;
        private readonly string _hashPrefix;

        public HashService(HashAlgorithm hashAlgorithm, string hashPrefix, IConsoleWriter consoleWriter)
        {
            _hashAlgorithm = hashAlgorithm;
            _consoleWriter = consoleWriter;
            _hashPrefix = hashPrefix.ToLower();
        }

        public Task<string> GetHash(string path)
        {
            return Task.Run(() =>
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _consoleWriter.WriteLine($"Hashing: '{path}'");
                    var hash = _hashAlgorithm.ComputeHash(fileStream);
                    _consoleWriter.WriteLine($"End Hashing: '{path}'");
                    return $"{_hashPrefix}:{ToHex(hash)}";
                }
            });
        }

        private static string ToHex(IEnumerable<byte> hash)
        {
            return string.Concat(hash.Select(x => x.ToString("x2")));
        }
    }
}
