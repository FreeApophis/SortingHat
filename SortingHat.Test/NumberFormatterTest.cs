using Apophis.CLI;
using Funcky.Monads;
using Xunit;

namespace SortingHat.Test;

public class NumberFormatterTest
{
    [Theory]
    [InlineData("sha256:5891b5b522d5df086d0ff0b110fbd9d21bb4fc7163af34d08286a2e846f6be03", "5891b5b5")]
    [InlineData("md5:01234567", "01234567")]
    public void GivenAnAlgorithmAndADigestThenShortHashIsTheFirstEightDigits(string hash, string shortHash)
    {
        Assert.Equal<Option<string>>(Option.Some(shortHash), hash.ShortHash());
    }

    [Theory]
    [InlineData("")]
    [InlineData("5891b5b522d5df08")]
    [InlineData("sha256:")]
    [InlineData("sha256:1234567")]
    [InlineData(":5891b5b522d5df08")]
    [InlineData("sha256:5891b5b522d5df08:5891b5b522d5df08")]
    public void GivenAMalformedHashThenThereIsNoShortHash(string hash)
    {
        Assert.Equal<Option<string>>(Option<string>.None, hash.ShortHash());
    }
}
