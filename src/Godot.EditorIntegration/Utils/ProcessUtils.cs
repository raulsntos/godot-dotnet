using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

namespace Godot.EditorIntegration.Utils;

internal static class ProcessUtils
{
    public static string? PathWhich(string name)
    {
        if (OperatingSystem.IsWindows())
        {
            return PathWhichWindows(name);
        }

        return PathWhichUnix(name);
    }

    [SupportedOSPlatform("windows")]
    private static string? PathWhichWindows(string name)
    {
        string[] windowsExts = Environment.GetEnvironmentVariable("PATHEXT")?.Split(Path.PathSeparator) ?? [];
        string[] pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        char[] invalidPathChars = Path.GetInvalidPathChars();

        List<string> searchDirs = [];

        if (pathDirs is not null)
        {
            foreach (string pathDir in pathDirs)
            {
                if (pathDir.AsSpan().ContainsAny(invalidPathChars))
                {
                    continue;
                }

                searchDirs.Add(pathDir);
            }
        }

        string nameExt = Path.GetExtension(name);
        bool hasPathExt = !string.IsNullOrEmpty(nameExt)
            && windowsExts.Contains(nameExt, StringComparer.OrdinalIgnoreCase);

        // Last in the list.
        searchDirs.Add(Directory.GetCurrentDirectory());

        if (hasPathExt)
        {
            return searchDirs
                .Select(dir => Path.Join(dir, name))
                .FirstOrDefault(File.Exists);
        }

        return searchDirs
            .Select(dir => Path.Join(dir, name))
            .SelectMany(path => windowsExts.Select(ext => $"{path}{ext}"))
            .FirstOrDefault(File.Exists);
    }

    [UnsupportedOSPlatform("windows")]
    private static string? PathWhichUnix(string name)
    {
        string[] pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        char[] invalidPathChars = Path.GetInvalidPathChars();

        List<string>? searchDirs = [];

        if (pathDirs is not null)
        {
            foreach (string pathDir in pathDirs)
            {
                if (pathDir.AsSpan().ContainsAny(invalidPathChars))
                {
                    continue;
                }

                searchDirs.Add(pathDir);
            }
        }

        // Last in the list.
        searchDirs.Add(Directory.GetCurrentDirectory());

        return searchDirs
            .Select(dir => Path.Join(dir, name))
            .FirstOrDefault(path =>
            {
                return File.Exists(path) && File.GetUnixFileMode(path).HasFlag(UnixFileMode.UserExecute);
            });
    }

    public static string GetCommandLineDisplay(this ProcessStartInfo startInfo)
    {
        var sb = new StringBuilder();

        AppendProcessFileNameForDisplay(sb, startInfo.FileName);

        if (startInfo.ArgumentList.Count == 0)
        {
            sb.Append(' ');
            sb.Append(startInfo.Arguments);
        }
        else
        {
            AppendProcessArgumentsForDisplay(sb, startInfo.ArgumentList);
        }

        return sb.ToString();

        static void AppendProcessWordForDisplay(StringBuilder sb, string word)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            if (word.Contains(' '))
            {
                sb.Append('"');
                sb.Append(word);
                sb.Append('"');
            }
            else
            {
                sb.Append(word);
            }
        }

        static void AppendProcessFileNameForDisplay(StringBuilder sb, string fileName)
        {
            AppendProcessWordForDisplay(sb, fileName);
        }

        static void AppendProcessArgumentsForDisplay(StringBuilder sb, Collection<string> argumentList)
        {
            // This is intended just for reading. It doesn't need to be a valid command line.
            // E.g.: We don't handle escaping of quotes.

            foreach (string argument in argumentList)
            {
                AppendProcessWordForDisplay(sb, argument);
            }
        }
    }
}
