using System;
using System.Linq;
using Autofac;
using Funcky.Extensions;
using SortingHat.API.DI;
using Xunit;
using File = System.IO.File;

namespace SortingHat.Test;

/// <summary>What the database gives back for a stored file, with a real SQLite database and real files in a temporary folder.</summary>
public sealed class FileStoreTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAStoredFileThenItsCreationTimeIsReadBackAsUtc()
    {
        var path = _hat.TaggedFile("a.txt", ":x");

        var found = _hat.InScope(scope => scope.Resolve<IFile>().Search(":x")).Match(left: problem => throw new InvalidOperationException(problem), right: result => result.Files.Single());

        Assert.Equal((DateTimeKind.Utc, File.GetCreationTimeUtc(path)), (found.File.CreatedAt.Kind, found.File.CreatedAt));
    }

    [Fact]
    public void GivenAStoredFileThenItIsFoundByItsPathWithItsFingerprint()
    {
        var path = _hat.TaggedFile("a.txt", ":x");

        var found = _hat.InScope(scope => scope.Resolve<IFile>().Find(path));

        Assert.True(found.Match(none: false, some: file => file.Path == path && file.Fingerprint.Match(none: false, some: fingerprint => fingerprint.Size == 5)));
    }

    [Fact]
    public void GivenAnUnknownPathThenNothingIsFound()
        => Assert.None(_hat.InScope(scope => scope.Resolve<IFile>().Find(_hat.WriteFile("a.txt", "a"))));

    public void Dispose()
        => _hat.Dispose();
}
