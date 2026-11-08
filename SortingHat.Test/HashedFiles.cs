using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Test;

/// <summary>
/// The real hashing of hat, recording the files it reads to hash them.
/// Hashing is an edge like the console, so a test sees whether hat trusted a stored hash or read the file again.
/// </summary>
internal sealed class HashedFiles
{
    private readonly ConcurrentQueue<string> _paths = new();

    /// <summary>The files hashed since the last <see cref="Clear"/>, in the order hashing started.</summary>
    public IReadOnlyList<string> Paths
        => [.. _paths];

    public void Clear()
        => _paths.Clear();

    /// <summary>Wraps the hashing of hat, for <c>new HatSandbox(hashedFiles.Register)</c>.</summary>
    public void Register(ContainerBuilder builder)
        => builder.RegisterDecorator<IHashService>((_, _, hashService) => new Recording(hashService, _paths));

    private sealed class Recording(IHashService hashService, ConcurrentQueue<string> paths) : IHashService
    {
        public Task<Hash> GetHash(string path)
        {
            paths.Enqueue(path);

            return hashService.GetHash(path);
        }
    }
}
