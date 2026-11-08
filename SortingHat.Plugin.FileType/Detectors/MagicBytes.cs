using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SortingHat.Plugin.FileType.Detectors;

/// <summary>A file type recognised by fixed bytes at a fixed <see cref="Offset"/>, such as '%PDF' at 0.</summary>
internal sealed record MagicBytes(FileType FileType, int Offset, ImmutableArray<BytePattern> Pattern) : Signature(FileType)
{
    public override int HeadLength
        => Offset + Pattern.Length;

    public override bool Matches(IReadOnlyList<byte> head)
        => head.Count >= HeadLength
            && head.Skip(Offset).Zip(Pattern).All(pair => pair.Second.Matches(pair.First));
}
