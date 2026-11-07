using System;
using System.IO;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// Tags files with auto-tag masks through the real command line application, with real files in a temporary folder.
/// </summary>
public sealed class AutoTagTest : IDisposable
{
    private readonly HatSandbox _hat = new();

    [Fact]
    public void GivenAMaskWithTheUpdateYearThenTheFileIsTaggedWithTheYearOfItsLastWrite()
    {
        var path = _hat.WriteFile("a.txt", "a");
        File.SetLastWriteTime(path, new DateTime(2003, 4, 5, 6, 7, 8, DateTimeKind.Local));

        Assert.Equal(0, _hat.Hat(["tag-files", ":updated:{UpdatedAt.Year}", path]));

        Assert.Equal(["a.txt"], _hat.FindFiles(":updated:2003"));
    }

    [Theory]
    [InlineData(0, "invoices")]
    [InlineData(1, "archive")]
    public void GivenAMaskWithAFolderCountedFromTheFileThenTheFileIsTaggedWithThatFolder(int index, string folder)
    {
        var path = FileIn(Path.Combine("archive", "invoices"), "a.txt");

        Assert.Equal(0, _hat.Hat(["tag-files", $":folder:{{Folder.FromFile.{index}}}", path]));

        Assert.Equal(["a.txt"], _hat.FindFiles($":folder:{folder}"));
    }

    public void Dispose()
        => _hat.Dispose();

    private string FileIn(string folder, string name)
    {
        var root = Path.GetDirectoryName(_hat.WriteFile("marker.txt", "marker")) ?? string.Empty;
        Directory.CreateDirectory(Path.Combine(root, folder));

        return _hat.WriteFile(Path.Combine(folder, name), name);
    }
}
