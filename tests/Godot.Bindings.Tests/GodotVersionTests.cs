using Godot.Bridge;

namespace Godot.Bindings.Tests;

public class GodotVersionTests
{
    [Theory]
    // Stable version uses no prerelease label.
    [InlineData(4, 0, 0, "stable", "4.0.0")]
    // Unnumbered versions are development builds that aren't published.
    // Godot .NET always uses 'dev' for these.
    [InlineData(4, 1, 0, "alpha", "4.1.0-dev")]
    [InlineData(4, 2, 0, "beta", "4.2.0-dev")]
    [InlineData(4, 3, 0, "dev", "4.3.0-dev")]
    [InlineData(4, 4, 0, "rc", "4.4.0-dev")]
    // Numbered prerelease versions use the appropriate label.
    // Godot .NET uses 'alpha' for 'dev' builds. The rest should match.
    [InlineData(4, 5, 0, "alpha1", "4.5.0-alpha.1")]
    [InlineData(4, 6, 0, "beta2", "4.6.0-beta.2")]
    [InlineData(4, 7, 0, "dev3", "4.7.0-alpha.3")]
    [InlineData(4, 8, 0, "rc4", "4.8.0-rc.4")]
    public void GetGodotDotNetVersionReturnsExpectedString(int major, int minor, int patch, string status, string expected)
    {
        var godotVersion = new GodotVersion()
        {
            Major = major,
            Minor = minor,
            Patch = patch,
            Status = status,
            Build = "official",
            Hash = "abcd1234efgh5678ijkl9012mnop3456qrst7890",
            TimeStamp = 1700000000,
            DisplayString = $"Godot v{major}.{minor}.{patch}.{status}.official",
        };

        string actual = godotVersion.GetGodotDotNetVersion();
        Assert.Equal(expected, actual);
    }
}
