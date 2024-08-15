using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Export;

internal static class XCFramework
{
    public static bool GenerateBlocking(List<string> outputPaths, string xcFrameworkPath)
    {
        return EditorProgress.Invoke("generate_xcframework", "Generating XCFramework...", 1, progress =>
        {
            progress.Step("Running xcodebuild -create-xcframework", 0);
            return GenerateCore(outputPaths, xcFrameworkPath);
        });
    }

    private static bool GenerateCore(List<string> outputPaths, string xcFrameworkPath)
    {
        try
        {
            if (Directory.Exists(xcFrameworkPath))
            {
                Directory.Delete(xcFrameworkPath, true);
            }

            var startInfo = new ProcessStartInfo("xcrun");

            string baseDylib = $"{EditorPath.ProjectAssemblyName}.dylib";
            string baseSym = $"{EditorPath.ProjectAssemblyName}.framework.dSYM";

            startInfo.ArgumentList.Add("xcodebuild");
            startInfo.ArgumentList.Add("-create-xcframework");

            foreach (var outputPath in outputPaths)
            {
                startInfo.ArgumentList.Add("-library");
                startInfo.ArgumentList.Add(Path.Join(outputPath, baseDylib));
                startInfo.ArgumentList.Add("-debug-symbols");
                startInfo.ArgumentList.Add(Path.Join(outputPath, baseSym));
            }

            startInfo.ArgumentList.Add("-output");
            startInfo.ArgumentList.Add(xcFrameworkPath);

            using var process = BuildManager.StartProcess(startInfo);
            process.WaitForExit();
            int exitCode = process.ExitCode;

            if (exitCode != 0 && OS.Singleton.IsStdOutVerbose())
            {
                GD.Print($"xcodebuild create-xcframework exited with code: {exitCode}.");
            }

            return exitCode == 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return false;
        }
    }
}
