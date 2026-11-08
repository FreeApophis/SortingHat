using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;
using Xunit;

namespace SortingHat.Test;

/// <summary>
/// The FileType plugin identifies files by their content, through the real command line application with the real plugin.
/// </summary>
public sealed class FileTypeTest : IDisposable
{
    private const byte Filler = 0xaa;

    private readonly HatSandbox _hat = HatSandbox.WithPluginFixtures("Working");

    /// <summary>Every basic signature in FileSignatures.csv, as line number, offset, magic bytes and the name it must find.</summary>
    public static IEnumerable<object[]> Signatures
        => File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Resources", "FileSignatures.csv"))
            .Select((line, index) => (Number: index + 1, Columns: line.Split(',').Select(column => column.Trim()).ToList()))
            .WhereSelect(row => row.Columns is ["basic", _, var offset, var signature, var name, ..]
                ? long.ParseOrNone(offset).Select(value => (object[])[row.Number, value, signature, name])
                : Option<object[]>.None);

    [Theory]
    [MemberData(nameof(Signatures))]
    public void GivenAFileWithTheMagicBytesOfASignatureThenIdentifyFindsItsType(int line, long offset, string signature, string name)
    {
        var file = _hat.WriteFile($"line{line}.dat", [.. Enumerable.Repeat(Filler, (int)offset), .. Bytes(signature), .. Enumerable.Repeat(Filler, 16)]);

        Assert.Equal(0, _hat.Hat(["identify", file]));

        Assert.Contains($"Name     : {name}", _hat.Lines);
    }

    [Theory]
    [InlineData("Bob wrote this letter.")]
    [InlineData("<p>Some markup</p>")]
    [InlineData("Good morning, this is plain text.")]
    public void GivenPlainTextThenIdentifyDoesNotKnowItsType(string text)
    {
        var file = _hat.WriteFile("text.dat", string.Concat(Enumerable.Repeat(text, 100)));

        Assert.Equal(0, _hat.Hat(["identify", file]));

        Assert.Contains("Unknown filetype", _hat.Lines);
    }

    [Fact]
    public void GivenPacketsOfATransportStreamThenIdentifyFindsIt()
    {
        var packet = (byte[])[0x47, .. Enumerable.Repeat(Filler, 187)];
        var file = _hat.WriteFile("video.dat", [.. Enumerable.Repeat(packet, 20).Flatten()]);

        Assert.Equal(0, _hat.Hat(["identify", file]));

        Assert.Contains("Name     : MPEG transport stream", _hat.Lines);
    }

    public void Dispose()
        => _hat.Dispose();

    /// <summary>The bytes a signature describes, a '?' nibble is 'e', so it does not happen to be the magic bytes of another signature.</summary>
    private static IEnumerable<byte> Bytes(string signature)
        => signature
            .Chunk(2)
            .Select(pair => Convert.ToByte(new string([.. pair.Select(nibble => nibble == '?' ? 'e' : nibble)]), 16));
}
