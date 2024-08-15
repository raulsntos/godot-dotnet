using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Godot.EditorIntegration.Internals;
using JetBrains.Rider.PathLocator;

namespace Godot.EditorIntegration.CodeEditors;

internal sealed class RiderManager : CodeEditorManager
{
    private readonly RiderPathLocator _riderPathLocator;
    private readonly RiderFileOpener _riderFileOpener;

    private static string? _riderPath;

    public RiderManager()
    {
        var riderLocatorEnvironment = new RiderLocatorEnvironment();
        _riderPathLocator = new RiderPathLocator(riderLocatorEnvironment);
        _riderFileOpener = new RiderFileOpener(riderLocatorEnvironment);
    }

    protected override Error LaunchCore(string filePath, int line, int column)
    {
        if (!File.Exists(_riderPath))
        {
            // Try to search it again if it wasn't found last time or if it was removed from its location.
            if (!TryGetRiderPath(_riderPathLocator, out _riderPath))
            {
                GD.PushError(SR.FormatCodeEditorErrorNotFound("JetBrains Rider"));
                return Error.FileNotFound;
            }
        }

        _riderFileOpener.OpenFile(_riderPath, EditorPath.ProjectSlnPath, filePath, line, column);
        return Error.Ok;
    }

    private static bool TryGetRiderPath(RiderPathLocator locator, [NotNullWhen(true)] out string? path)
    {
        var allInfos = locator.GetAllRiderPaths();
        if (allInfos.Length == 0)
        {
            path = null;
            return false;
        }

        var riderInfos = allInfos
            .Where(info => IsRider(info.Path))
            .ToArray();

        path = riderInfos.Length > 0
            ? riderInfos[^1].Path
            : allInfos[^1].Path;
        return true;
    }

    private static bool IsRider([NotNullWhen(true)] string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (path.AsSpan().ContainsAny(Path.GetInvalidPathChars()))
        {
            return false;
        }

        return Path.GetFileName(path).StartsWith("rider", StringComparison.OrdinalIgnoreCase);
    }
}
