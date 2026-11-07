using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Autofac;
using SortingHat.API.DI;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Hashing through the real composition root: every <see cref="API.Models.File"/> starts hashing in the background as soon as it knows its path.
/// </summary>
public sealed class HashServiceTest : IDisposable
{
    private const int FileCount = 32;
    private const int FileSize = 1 << 20;

    private readonly HatSandbox _hat = new();

    [Fact]
    public async Task GivenManyFilesHashedAtTheSameTimeThenEveryHashIsCorrect()
    {
        var paths = Enumerable.Range(0, FileCount)
            .Select(index => _hat.WriteFile($"{index}.bin", new string((char)('A' + index), FileSize)))
            .ToList();

        var hashes = await _hat.InScope(scope => Task.WhenAll(paths.Select(scope.Resolve<IHashService>().GetHash)));

        Assert.Equal(paths.Select(ExpectedHash), hashes);
    }

    public void Dispose()
        => _hat.Dispose();

    private static string ExpectedHash(string path)
        => $"sha256:{Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)))}";
}
