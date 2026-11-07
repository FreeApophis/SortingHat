using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Apophis.CLI.Writer;

namespace SortingHat.API.DI;

/// <summary>Hashes files with a new hash algorithm from <paramref name="newHashAlgorithm"/> per file, because one instance must not be used concurrently.</summary>
internal class HashService(Func<HashAlgorithm> newHashAlgorithm, string hashPrefix, IConsoleWriter consoleWriter) : IHashService
{
    private readonly string _hashPrefix = hashPrefix.ToLower();

    public Task<string> GetHash(string path)
    {
        return Task.Run(() =>
        {
            using var hashAlgorithm = newHashAlgorithm();
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
