using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Apophis.CLI.Writer;

namespace SortingHat.API.DI;

public class HashService(HashAlgorithm hashAlgorithm, string hashPrefix, IConsoleWriter consoleWriter) : IHashService
{
    private readonly string _hashPrefix = hashPrefix.ToLower();

    public Task<string> GetHash(string path)
    {
        return Task.Run(() =>
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            consoleWriter.WriteLine($"Hashing: '{path}'");
            var hash = hashAlgorithm.ComputeHash(fileStream);
            consoleWriter.WriteLine($"End Hashing: '{path}'");
            return $"{_hashPrefix}:{ToHex(hash)}";
        });
    }

    private static string ToHex(IEnumerable<byte> hash)
    {
        return string.Concat(hash.Select(x => x.ToString("x2")));
    }
}
