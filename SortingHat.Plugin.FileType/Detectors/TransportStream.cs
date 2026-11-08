using System.Collections.Generic;
using System.Linq;

namespace SortingHat.Plugin.FileType.Detectors;

/// <summary>An MPEG transport stream: packets of 188 bytes, each starting with the sync byte 0x47.</summary>
internal sealed record TransportStream(FileType FileType) : Signature(FileType)
{
    private const int PacketLength = 188;
    private const byte SyncByte = 0x47;
    private const int PacketsChecked = 8;

    /// <summary>One sync byte is just a file starting with 'G'.</summary>
    private const int MinimumPackets = 2;

    public override int HeadLength
        => PacketLength * PacketsChecked;

    public override bool Matches(IReadOnlyList<byte> head)
        => head.Chunk(PacketLength).Take(PacketsChecked).ToList() is var packets
            && packets.Count >= MinimumPackets
            && packets.All(packet => packet is [SyncByte, ..]);
}
