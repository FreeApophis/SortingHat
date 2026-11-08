using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.Plugin.FileType.Detectors;

/// <summary>
/// All known signatures, in the order of FileSignatures.csv: signatures are not prefix free, the first match wins.
/// A row is 'detector,category,offset,magic bytes,name,extension,...', a line starting with '#' is a comment.
/// </summary>
internal sealed record Signatures(ImmutableArray<Signature> All)
{
    private const char Separator = ',';

    /// <summary>How many bytes from the start of a file <see cref="Identify"/> needs.</summary>
    public int HeadLength
        => All.Select(signature => signature.HeadLength).DefaultIfEmpty(0).Max();

    public Option<FileType> Identify(IReadOnlyList<byte> head)
        => All.FirstOrNone(signature => signature.Matches(head)).Select(signature => signature.FileType);

    /// <summary>The signatures in the <paramref name="text"/> of FileSignatures.csv, or the first row that is none, with its line number.</summary>
    public static Either<string, Signatures> Parse(string text)
        => text.Split('\n')
            .Select((line, index) => (Number: index + 1, Line: line.Trim()))
            .Where(row => row.Line is not "" && !row.Line.StartsWith('#'))
            .Aggregate(
                Either<string, ImmutableArray<Signature>>.Right([]),
                (signatures, row) => from parsed in signatures
                                     from signature in Row(row.Line).SelectLeft(reason => $"line {row.Number}: {reason}")
                                     select parsed.Add(signature))
            .Select(signatures => new Signatures(signatures));

    private static Either<string, Signature> Row(string line)
        => line.Split(Separator).Select(column => column.Trim()).ToArray() switch
        {
            [var detector, var category, var offset, var magicBytes, var name, .. var extensions]
                => from fileType in FileType(category, name, extensions)
                   from signature in Signature(detector, offset, magicBytes, fileType)
                   select signature,
            _ => Either<string, Signature>.Left("a row is 'detector,category,offset,magic bytes,name,extension,...'"),
        };

    private static Either<string, FileType> FileType(string category, string name, IReadOnlyList<string> extensions)
        => (category, name, extensions) switch
        {
            ("", _, _) => Either<string, FileType>.Left("the category is missing"),
            (_, "", _) => Either<string, FileType>.Left("the name is missing"),
            (_, _, []) => Either<string, FileType>.Left("an extension is missing"),
            _ => extensions.FirstOrNone(extension => !IsExtension(extension)).Match(
                none: () => Either<string, FileType>.Right(new FileType(category, name, extensions)),
                some: extension => Either<string, FileType>.Left($"'{extension}' is no extension, is there a '{Separator}' in the name?")),
        };

    private static bool IsExtension(string extension)
        => extension is not "" && extension.All(character => char.IsAsciiLetterOrDigit(character) || character is '_');

    private static Either<string, Signature> Signature(string detector, string offset, string magicBytes, FileType fileType)
        => detector switch
        {
            "basic" => from start in Offset(offset)
                       from pattern in Pattern(magicBytes)
                       select (Signature)new MagicBytes(fileType, start, pattern),
            "ts" => Either<string, Signature>.Right(new TransportStream(fileType)),
            _ => Either<string, Signature>.Left($"'{detector}' is no detector, use 'basic' or 'ts'"),
        };

    private static Either<string, int> Offset(string offset)
        => int.ParseOrNone(offset)
            .Where(start => start >= 0)
            .ToEither($"the offset '{offset}' is no number of bytes");

    private static Either<string, ImmutableArray<BytePattern>> Pattern(string magicBytes)
        => magicBytes.Length > 0 && magicBytes.Length % 2 == 0
            ? magicBytes
                .Chunk(2)
                .Select(pair => pair is [var high, var low] ? BytePattern.Parse(high, low) : Option<BytePattern>.None)
                .Aggregate(
                    Option.Some(ImmutableArray<BytePattern>.Empty),
                    (pattern, octet) => from parsed in pattern from next in octet select parsed.Add(next))
                .ToEither($"the magic bytes '{magicBytes}' are no pairs of hex digits or '?'")
            : Either<string, ImmutableArray<BytePattern>>.Left($"the magic bytes '{magicBytes}' are no pairs of hex digits or '?'");
}
