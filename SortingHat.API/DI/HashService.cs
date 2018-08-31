using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SortingHat.API.DI
{
    public class HashService : IHashService
    {
        readonly HashAlgorithm _hashAlgorithm;
        readonly string _hashPrefix;

        public HashService(HashAlgorithm hashAlgorithm, string hashPrefix)
        {
            _hashAlgorithm = hashAlgorithm;
            _hashPrefix = hashPrefix.ToLower();
        }

        public Task<string> GetHash(string path)
        {
            return Task.Run(() =>
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    System.Console.WriteLine($"Hashing: '{path}'");
                    var hash = _hashAlgorithm.ComputeHash(fileStream);
                    System.Console.WriteLine($"End Hashing: '{path}'");
                    return $"{_hashPrefix}:{ToHex(hash)}";
                }
            });
        }

        private static string ToHex(byte[] hash)
        {
            return string.Concat(hash.Select(x => x.ToString("x2")));
        }
    }
}
