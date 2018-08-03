using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SortingHat.API.DI
{
    public class HashService
    {
        readonly HashAlgorithm _hashAlgorithm;
        readonly string _hashPrefix;

        public HashService(HashAlgorithm hashAlgorithm, string hashPrefix)
        {
            _hashAlgorithm = hashAlgorithm;
            _hashPrefix = hashPrefix.ToLower();
        }

        public string GetHash(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var hash = _hashAlgorithm.ComputeHash(fileStream);

                return $"{_hashPrefix}:{ToHex(hash)}";
            }
        }

        private static string ToHex(byte[] hash)
        {
            return string.Concat(hash.Select(x => x.ToString("x2")));
        }
    }
}
