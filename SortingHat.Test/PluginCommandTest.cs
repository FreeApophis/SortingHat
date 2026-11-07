using System;
using System.IO;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// The commands of the real plugins, loaded from 'plugins' next to hat, through the real command line application.
/// They declare their arguments, so hat checks the command line and writes their help.
/// </summary>
public sealed class PluginCommandTest : IDisposable
{
    private readonly HatSandbox _hat = HatSandbox.WithPluginFixtures("Working");

    [Theory]
    [InlineData("exif", "<file>")]
    [InlineData("identify", "<file>")]
    [InlineData("scan", "<folder>")]
    public void GivenNoArgumentThenThePluginCommandAsksForIt(string command, string argument)
    {
        Assert.NotEqual(0, _hat.Hat([command]));
        Assert.Contains(_hat.Lines, line => line.Contains(argument));
    }

    [Theory]
    [InlineData("exif")]
    [InlineData("identify")]
    [InlineData("scan")]
    public void GivenTheHelpOfAPluginCommandThenItDescribesItsArgument(string command)
    {
        Assert.Equal(0, _hat.Hat([command, "--help"]));
        Assert.Contains(_hat.Lines, line => line.Contains("Files or file patterns") || line.Contains("Folders"));
    }

    [Theory]
    [InlineData("identify")]
    [InlineData("id")]
    public void GivenAnImageThenIdentifyShowsItsRealType(string command)
    {
        var image = Image("photo.dat");

        Assert.Equal(0, _hat.Hat([command, image]));

        Assert.Contains("Category : image", _hat.Lines);
        Assert.Contains("Name     : Portable Network Graphics", _hat.Lines);
    }

    [Fact]
    public void GivenAnImageThenExifShowsItsMetadata()
    {
        Assert.Equal(0, _hat.Hat(["exif", Image("test.png")]));

        Assert.Contains(_hat.Lines, line => line.StartsWith("PNG-IHDR - Image Width"));
    }

    [Fact]
    public void GivenAFolderWithTextThenScanListsItsWords()
    {
        var folder = Path.GetDirectoryName(_hat.WriteFile("notes.txt", "invoice invoice tax")) ?? string.Empty;

        Assert.Equal(0, _hat.Hat(["scan", folder]));

        Assert.Contains(_hat.Lines, line => line.StartsWith("Words: "));
        Assert.Contains("invoice", _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();

    private string Image(string name)
    {
        var image = _hat.WriteFile(name, string.Empty);
        using var resource = typeof(PluginCommandTest).Assembly.GetManifestResourceStream("SortingHat.Test.Resources.test.png") ?? throw new InvalidOperationException("test.png is missing");
        using var file = File.Create(image);
        resource.CopyTo(file);

        return image;
    }
}
