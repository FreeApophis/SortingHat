using Apophis.CLI;
using Xunit;

namespace SortingHat.Test;

public class NumberFormatterTest
{
    [Theory]
    [InlineData(0, "0 B ")]
    [InlineData(999, "999 B ")]
    [InlineData(1000, "1 KB")]
    [InlineData(1999, "1 KB")]
    [InlineData(999_999, "999 KB")]
    [InlineData(1_000_000, "1 MB")]
    [InlineData(1_500_000_000, "1 GB")]
    [InlineData(long.MaxValue, "9 EB")]
    public void HumanSizeUsesDecimalUnitsAndTruncatesToWholeNumbers(long size, string humanSize)
    {
        Assert.Equal(humanSize, size.HumanSize());
    }
}
