using System;

namespace Godot.UpgradeAssistant.Tests;

public class SemVerTests
{
    [Theory]
    [InlineData("1.0.0", 1, 0, 0, "", "")]
    [InlineData("1.2.3", 1, 2, 3, "", "")]
    [InlineData("1.2.3-alpha", 1, 2, 3, "alpha", "")]
    [InlineData("1.2.3-alpha.1", 1, 2, 3, "alpha.1", "")]
    [InlineData("1.2.3+build.123", 1, 2, 3, "", "build.123")]
    [InlineData("1.2.3-alpha+build.123", 1, 2, 3, "alpha", "build.123")]
    [InlineData("1.2.3-alpha.1+build.123", 1, 2, 3, "alpha.1", "build.123")]
    [InlineData("0.0.0", 0, 0, 0, "", "")]
    [InlineData("10.20.30", 10, 20, 30, "", "")]
    [InlineData("1.0.0-0A.is-rc+build-001", 1, 0, 0, "0A.is-rc", "build-001")]
    public void TryParseValidVersions(string input, int expectedMajor, int expectedMinor, int expectedPatch, string expectedPrerelease, string expectedBuildMetadata)
    {
        bool success = SemVer.TryParse(input, out var result);
        Assert.True(success);
        Assert.Equal(expectedMajor, result.Major);
        Assert.Equal(expectedMinor, result.Minor);
        Assert.Equal(expectedPatch, result.Patch);
        Assert.Equal(expectedPrerelease, result.Prerelease);
        Assert.Equal(expectedBuildMetadata, result.BuildMetadata);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1.0")]
    [InlineData("a.b.c")]
    [InlineData("1.0.0-")]
    [InlineData("1.0.0+")]
    [InlineData("1.0.0-01")]
    [InlineData("1.0.0-rc..1")]
    [InlineData("1.0.0+build..1")]
    [InlineData("1.0.0-rc#1")]
    [InlineData("1.0.0+build#1")]
    [InlineData("01.2.3")]
    [InlineData("1.02.3")]
    [InlineData("1.2.03")]
    public void TryParseInvalidVersions(string input)
    {
        bool success = SemVer.TryParse(input, out _);
        Assert.False(success);
    }

    [Theory]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("1.2.3-alpha", "1.2.3-alpha")]
    [InlineData("1.2.3+build.123", "1.2.3+build.123")]
    [InlineData("1.2.3-alpha+build.123", "1.2.3-alpha+build.123")]
    public void SemVerToString(string input, string expected)
    {
        var version = SemVer.Parse(input);
        Assert.Equal(expected, version.ToString());
    }

    [Theory]
    [InlineData("1.2.3", "1.2.3", true)]
    [InlineData("1.2.3", "1.2.3-alpha", false)]
    [InlineData("1.2.3", "1.2.3-alpha.1", false)]
    [InlineData("1.2.3-alpha.1", "1.2.3-alpha.1", true)]
    [InlineData("1.2.3-alpha.1", "1.2.3-alpha.2", false)]
    [InlineData("1.2.3-dev.1", "1.2.3-alpha.1", true)]
    public void SemVerEquals(string inputA, string inputB, bool expectedEqual)
    {
        var a = SemVer.Parse(inputA);
        var b = SemVer.Parse(inputB);

        Assert.Equal(expectedEqual, a.Equals(b));
    }

    [Theory]
    [InlineData("1.2.3", "1.2.4", -1)]
    [InlineData("1.2.4", "1.2.3", 1)]
    [InlineData("1.2.3", "1.2.3", 0)]
    [InlineData("1.2.3-alpha", "1.2.3", -1)]
    [InlineData("1.2.3", "1.2.3-alpha", 1)]
    [InlineData("1.2.3-alpha", "1.2.3-alpha", 0)]
    [InlineData("1.2.3-alpha.1", "1.2.3-alpha.2", -1)]
    [InlineData("1.2.3-alpha.2", "1.2.3-alpha.1", 1)]
    [InlineData("1.2.3-alpha.1", "1.2.3-alpha.1", 0)]
    [InlineData("1.2.3+build.1", "1.2.3+build.2", 0)]
    [InlineData("1.2.3+build.2", "1.2.3+build.1", 0)]
    public void SemVerComparison(string inputA, string inputB, int expectedComparison)
    {
        var a = SemVer.Parse(inputA);
        var b = SemVer.Parse(inputB);

        Assert.Equal(a.CompareTo(b), expectedComparison);
    }

    [Fact]
    public void SemVerSorting()
    {
        SemVer[] versionsExpected =
        [
            SemVer.Parse("1.0.0-alpha"),
            SemVer.Parse("1.0.0-alpha.1"),
            SemVer.Parse("1.0.0-dev.2"),
            SemVer.Parse("1.0.0-alpha.3"),
            SemVer.Parse("1.0.0-dev.4"),
            SemVer.Parse("1.0.0-beta"),
            SemVer.Parse("1.0.0-beta.2"),
            SemVer.Parse("1.0.0-beta.11"),
            SemVer.Parse("1.0.0-rc"),
            SemVer.Parse("1.0.0-rc.1"),
            SemVer.Parse("1.0.0-rc.3"),
            SemVer.Parse("1.0.0-rc.33"),
            SemVer.Parse("1.0.0"),
            SemVer.Parse("1.0.1"),
            SemVer.Parse("1.1.0"),
            SemVer.Parse("2.0.0"),
        ];

        SemVer[] versionsActual =
        [
            SemVer.Parse("1.0.0-alpha"),
            SemVer.Parse("1.1.0"),
            SemVer.Parse("1.0.0-alpha.3"),
            SemVer.Parse("1.0.0-dev.4"),
            SemVer.Parse("1.0.0-dev.2"),
            SemVer.Parse("1.0.0-alpha.1"),
            SemVer.Parse("1.0.0-rc"),
            SemVer.Parse("1.0.0-beta.2"),
            SemVer.Parse("1.0.0-beta.11"),
            SemVer.Parse("1.0.0-rc.33"),
            SemVer.Parse("1.0.0-beta"),
            SemVer.Parse("2.0.0"),
            SemVer.Parse("1.0.0-rc.3"),
            SemVer.Parse("1.0.0-rc.1"),
            SemVer.Parse("1.0.0"),
            SemVer.Parse("1.0.1"),
        ];
        Array.Sort(versionsActual);

        Assert.Equal(versionsExpected, versionsActual);
    }
}
