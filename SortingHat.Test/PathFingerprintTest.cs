using System;
using SortingHat.API.Models;
using Xunit;

namespace SortingHat.Test;

public class PathFingerprintTest
{
    private static readonly DateTime Written = new(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);
    private static readonly PathFingerprint Fingerprint = new(Size: 100, ModifiedAtUtc: Written, TakenAtUtc: Written.AddMinutes(5));

    [Fact]
    public void GivenTheSameSizeAndTimestampThenTheFingerprintStillMatches()
        => Assert.True(Fingerprint.StillMatches(100, Written));

    [Fact]
    public void GivenAnotherSizeThenTheFingerprintDoesNotMatch()
        => Assert.False(Fingerprint.StillMatches(101, Written));

    [Fact]
    public void GivenAnotherTimestampThenTheFingerprintDoesNotMatch()
        => Assert.False(Fingerprint.StillMatches(100, Written.AddTicks(1)));

    [Fact]
    public void GivenAFingerprintTakenRightAfterTheLastWriteThenItIsNotTrusted()
        => Assert.False((Fingerprint with { TakenAtUtc = Written.AddSeconds(1) }).StillMatches(100, Written));

    [Fact]
    public void GivenAFingerprintTakenTwoSecondsAfterTheLastWriteThenItIsTrusted()
        => Assert.True((Fingerprint with { TakenAtUtc = Written.AddSeconds(2) }).StillMatches(100, Written));
}
