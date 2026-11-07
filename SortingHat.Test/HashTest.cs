using Funcky.Monads;
using SortingHat.API.Models;
using Xunit;
using static Funcky.Functional;

namespace SortingHat.Test;

/// <summary>A hash names its algorithm and holds the digest as lowercase hex, exactly as the database stores it.</summary>
public sealed class HashTest
{
    private const string Sha256Digest = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08";

    [Fact]
    public void GivenASha256HashThenItKeepsItsAlgorithmAndDigest()
    {
        var hash = Valid($"sha256:{Sha256Digest}");

        Assert.Equal(("sha256", Sha256Digest), (hash.Algorithm, hash.Digest));
    }

    [Fact]
    public void GivenAHashThenItIsWrittenAsItWasParsed()
        => Assert.Equal($"sha256:{Sha256Digest}", Valid($"sha256:{Sha256Digest}").ToString());

    [Fact]
    public void GivenAnotherAlgorithmThenItIsAHashAsWell()
        => Assert.Equal("blake3", Valid("blake3:00ff").Algorithm);

    [Fact]
    public void GivenTheSameTextThenTheHashesAreEqual()
        => Assert.Equal(Valid($"sha256:{Sha256Digest}"), Valid($"sha256:{Sha256Digest}"));

    [Theory]
    [InlineData("sha256:5891b5b522d5df086d0ff0b110fbd9d21bb4fc7163af34d08286a2e846f6be03", "5891b5b5")]
    [InlineData("md5:01234567", "01234567")]
    [InlineData("crc:00ff", "00ff")]
    public void GivenAHashThenItsShortFormIsTheFirstEightDigitsOfItsDigest(string text, string shortForm)
        => Assert.Equal(shortForm, Valid(text).Short);

    [Theory]
    [InlineData("")]
    [InlineData("BADHASH")]
    [InlineData("sha256")]
    [InlineData("sha256:")]
    [InlineData(":00ff")]
    [InlineData("SHA256:00ff")]
    [InlineData("sha-256:00ff")]
    [InlineData("sha256:00FF")]
    [InlineData("sha256:0ff")]
    [InlineData("sha256:00fg")]
    [InlineData("sha256:00:ff")]
    public void GivenTextThatIsNoHashThenItIsInvalid(string text)
        => IsInvalid(Hash.Parse(text));

    [Theory]
    [InlineData("sha256:00ff")]
    [InlineData("sha256:9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a0800")]
    public void GivenAKnownAlgorithmWithADigestOfTheWrongLengthThenItIsInvalid(string text)
        => IsInvalid(Hash.Parse(text));

    private static Hash Valid(string text)
        => Hash.Parse(text).GetOrElse(problem => throw new Xunit.Sdk.XunitException(problem));

    private static void IsInvalid(Either<string, Hash> parsed)
        => Assert.True(parsed.Match(left: True, right: False), $"Expected an error, but the text was accepted: {parsed}");
}
