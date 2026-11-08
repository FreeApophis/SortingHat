using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// file-info shows the value of every tag variable for a file, built-in and from the real plugins, through the real command line application:
/// what a tag mask would put into the tag, so a mask can be written by looking first.
/// </summary>
public sealed class FileInfoVariablesTest : IDisposable
{
    private readonly HatSandbox _hat = HatSandbox.WithPluginFixtures("Working");

    [Fact]
    public void GivenAFileInFoldersThenFileInfoShowsItsFolders()
    {
        var path = _hat.WriteFile(Path.Combine("archive", "invoices", "a.txt"), "a");

        Assert.Equal(0, _hat.Hat(["file-info", "--variables", path]));

        Assert.Contains("  {Folder.FromFile.0} = invoices", _hat.Lines);
        Assert.Contains("  {Folder.FromFile.1} = archive", _hat.Lines);
    }

    [Fact]
    public void GivenANumberedVariableThenFileInfoShowsItsNumbersUpToTheLastValue()
    {
        var path = _hat.WriteFile(Path.Combine("archive", "invoices", "a.txt"), "a");

        _hat.Hat(["file-info", "--variables", path]);

        var numbers = _hat.Lines.Where(line => line.StartsWith("  {Folder.FromFile.")).ToList();
        Assert.InRange(numbers.Count, 3, 31);
        Assert.Equal(Enumerable.Range(0, numbers.Count).Select(number => $"  {{Folder.FromFile.{number}}} = "), numbers.Select(line => line[..line.IndexOf('=')] + "= "));
    }

    [Fact]
    public void GivenAVariableWithChoicesThenFileInfoShowsEachChoice()
    {
        var path = _hat.WriteFile("a.txt", "a");

        _hat.Hat(["file-info", "--variables", path]);

        Assert.Contains($"  {{CreatedAt.Year}} = {File.GetCreationTime(path).Year}", _hat.Lines);
        Assert.Contains(_hat.Lines, line => line.StartsWith("  {CreatedAt.Month} = "));
    }

    [Fact]
    public void GivenAnImageThenFileInfoShowsTheVariablesOfThePlugins()
    {
        var image = Image("photo.dat");

        _hat.Hat(["file-info", "--variables", image]);

        Assert.Contains("  {FileType.Extension} = png", _hat.Lines);
    }

    [Fact]
    public void GivenAVariableWithoutAValueThenFileInfoLeavesItOut()
    {
        var path = _hat.WriteFile("a.txt", "plain text");

        _hat.Hat(["file-info", "--variables", path]);

        Assert.DoesNotContain(_hat.Lines, line => line.StartsWith("  {FileType."));
    }

    [Fact]
    public void GivenAFileThatIsNotIndexedThenFileInfoStillShowsItsVariables()
    {
        var path = _hat.WriteFile(Path.Combine("archive", "a.txt"), "never tagged");

        _hat.Hat(["file-info", "--variables", path]);

        Assert.Contains("File not in index!", _hat.Lines);
        Assert.Contains("Tag variables:", _hat.Lines);
        Assert.Contains("  {Folder.FromFile.0} = archive", _hat.Lines);
    }

    [Fact]
    public void GivenNoVariablesOptionThenFileInfoStaysShort()
    {
        var path = _hat.WriteFile("a.txt", "a");

        Assert.Equal(0, _hat.Hat(["file-info", path]));

        Assert.DoesNotContain("Tag variables:", _hat.Lines);
    }

    [Fact]
    public void GivenTheShortOptionThenFileInfoShowsTheVariables()
    {
        var path = _hat.WriteFile("a.txt", "a");

        Assert.Equal(0, _hat.Hat(["file-info", "-v", path]));

        Assert.Contains("Tag variables:", _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();

    private string Image(string name)
    {
        using var resource = typeof(FileInfoVariablesTest).Assembly.GetManifestResourceStream("SortingHat.Test.Resources.test.png") ?? throw new InvalidOperationException("test.png is missing");
        using var content = new MemoryStream();
        resource.CopyTo(content);

        return _hat.WriteFile(name, content.ToArray());
    }
}
