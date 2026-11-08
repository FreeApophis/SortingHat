using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.Plugin.FileType.Detectors;

/// <summary>One byte of a magic number, written as two hex digits where '?' matches any nibble, such as '4c' or 'b?'.</summary>
internal readonly record struct BytePattern(byte Value, byte Mask)
{
    private const string HexDigits = "0123456789abcdef";

    public bool Matches(byte octet)
        => (octet & Mask) == Value;

    public static Option<BytePattern> Parse(char high, char low)
        => from highNibble in Nibble(high)
           from lowNibble in Nibble(low)
           select new BytePattern((byte)((highNibble.Value << 4) | lowNibble.Value), (byte)((highNibble.Mask << 4) | lowNibble.Mask));

    private static Option<(int Value, int Mask)> Nibble(char digit)
        => digit is '?'
            ? (0, 0)
            : HexDigits.IndexOfOrNone(char.ToLowerInvariant(digit)).Select(value => (value, 0xf));
}
