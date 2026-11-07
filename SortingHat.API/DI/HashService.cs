using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Apophis.CLI.Writer;
using SortingHat.API.Models;

namespace SortingHat.API.DI;

/// <summary>Hashes files with a new hash algorithm from <paramref name="newHashAlgorithm"/> per file, because one instance must not be used concurrently.</summary>
internal class HashService(Func<HashAlgorithm> newHashAlgorithm, string algorithmName, IConsoleWriter consoleWriter) : IHashService
{
    private readonly string _algorithm = algorithmName.ToLowerInvariant();

    public Task<Hash> GetHash(string path)
    {
        return Task.Run(() =>
        {
            using var hashAlgorithm = newHashAlgorithm();
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            consoleWriter.WriteLine($"Hashing: '{path}'");
            var digest = hashAlgorithm.ComputeHash(fileStream);
            consoleWriter.WriteLine($"End Hashing: '{path}'");
            return ToHash(digest);
        });
    }

    /// <summary>A hash this service computed that is no valid <see cref="Hash"/> is a bug, such as a misconfigured algorithm name.</summary>
    private Hash ToHash(byte[] digest)
        => Hash.Parse($"{_algorithm}:{Convert.ToHexStringLower(digest)}")
            .GetOrElse(problem => throw new InvalidOperationException(problem));
}
