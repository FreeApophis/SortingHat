using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace SortingHat.API.Models;

/// <summary>
/// The hash of a file's content, which identifies the file: the name of its algorithm and its digest as lowercase hex, written <c>sha256:9f86…</c>.
/// Any algorithm can be named; for a known one, the digest must have the length that algorithm produces.
/// </summary>
public sealed record Hash
{
    private const char Separator = ':';

    private const int ShortLength = 8;

    /// <summary>The length of the hex digest of every algorithm hat knows.</summary>
    private static readonly ImmutableDictionary<string, int> DigestLengths = ImmutableDictionary.CreateRange([KeyValuePair.Create("sha256", 64)]);

    private Hash(string algorithm, string digest)
        => (Algorithm, Digest) = (algorithm, digest);

    /// <summary>The name of the algorithm, lowercase letters and digits such as <c>sha256</c>.</summary>
    public string Algorithm { get; }

    /// <summary>The digest as lowercase hex.</summary>
    public string Digest { get; }

    /// <summary>The first eight digits of the <see cref="Digest"/>, enough to tell files apart on screen.</summary>
    public string Short
        => Digest[..int.Min(ShortLength, Digest.Length)];

    /// <summary>Returns the hash written in <paramref name="text"/> as <c>algorithm:digest</c>, or why it is none.</summary>
    public static Either<string, Hash> Parse(string text)
        => text.Split(Separator) switch
        {
            [var algorithm, var digest] => Problem(algorithm, digest).Match(
                none: () => Either<string, Hash>.Right(new Hash(algorithm, digest)),
                some: problem => Either<string, Hash>.Left($"'{text}' is no hash: {problem}.")),
            _ => Either<string, Hash>.Left($"'{text}' is no hash: it must be written as algorithm{Separator}digest."),
        };

    public override string ToString()
        => $"{Algorithm}{Separator}{Digest}";

    private static Option<string> Problem(string algorithm, string digest)
        => (algorithm, digest) switch
        {
            ("", _) => "the algorithm is missing",
            _ when !algorithm.All(IsLowercaseLetterOrDigit) => "the algorithm can only contain lowercase letters and digits",
            (_, "") => "the digest is missing",
            _ when !digest.All(char.IsAsciiHexDigitLower) => "the digest can only contain lowercase hex digits",
            _ when digest.Length % 2 != 0 => "the digest must have whole bytes, an even number of hex digits",
            _ => DigestLengths.GetValueOrNone(algorithm)
                .Where(length => length != digest.Length)
                .Select(length => $"a {algorithm} digest has {length} hex digits, not {digest.Length}"),
        };

    private static bool IsLowercaseLetterOrDigit(char character)
        => char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character);
}
