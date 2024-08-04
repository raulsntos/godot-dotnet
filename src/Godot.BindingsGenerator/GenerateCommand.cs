using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot.BindingsGenerator.Logging;
using Godot.BindingsGenerator.ApiDump;

namespace Godot.BindingsGenerator;

internal sealed class GenerateCommand : CliRootCommand
{
    private static readonly CliOption<FileInfo> _extensionApiPathOption = new CliOption<FileInfo>(
        name: "--extension-api"
    )
    {
        Description = "Path to the extension API dump JSON file.",
        Arity = ArgumentArity.ExactlyOne,
    }.AcceptExistingOnly();

    private static readonly CliOption<FileInfo> _extensionInterfacePathOption = new CliOption<FileInfo>(
        name: "--extension-interface"
    )
    {
        Description = "Path to the extension interface header file.",
        Arity = ArgumentArity.ExactlyOne,
    }.AcceptExistingOnly();

    private static readonly CliOption<DirectoryInfo?> _outputPathOption = new(
        name: "--output", aliases: ["-o"])
    {
        Description = "Path to the directory where the C# bindings will be generated.",
        Arity = ArgumentArity.ZeroOrOne,
    };

    private static readonly CliOption<DirectoryInfo?> _testOutputPathOption = new(
        name: "--test-output")
    {
        Description = "Path to the directory where the C# tests will be generated.",
        Arity = ArgumentArity.ZeroOrOne,
    };

    public GenerateCommand()
    {
        Options.Add(_extensionApiPathOption);
        Options.Add(_extensionInterfacePathOption);
        Options.Add(_outputPathOption);
        Options.Add(_testOutputPathOption);

        SetAction(HandleCommand);
    }

    private async Task<int> HandleCommand(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;

        var extensionApiPath = parseResult.GetValue(_extensionApiPathOption)
            ?? throw new InvalidOperationException("Godot extension API dump file path needs to be specified.");

        var extensionInterfacePath = parseResult.GetValue(_extensionInterfacePathOption)
            ?? throw new InvalidOperationException("Godot extension interface header file path needs to be specified.");

        var outputPath = parseResult.GetValue(_outputPathOption)
            ?? new DirectoryInfo(Path.Join(Environment.CurrentDirectory, "Generated"));

        var testOutputPath = parseResult.GetValue(_testOutputPathOption);

        var logger = ConsoleLogger.Instance;

        ClangGenerator.Generate(extensionInterfacePath.FullName, outputPath.FullName, testOutputPath?.FullName, logger);

        using var stream = extensionApiPath.OpenRead();
        var api = await GodotApi.DeserializeAsync(stream, cancellationToken);

        if (api is null || string.IsNullOrWhiteSpace(api.Header.VersionFullName))
        {
            logger.LogError("Error parsing the Godot extension API dump.");
            return -1;
        }

        logger.LogInformation($"Generating C# bindings for '{api.Header.VersionFullName}'.");

        BindingsGenerator.Generate(api, outputPath.FullName, logger: logger);

        string timeElapsed = FormatTimeSpan(DateTime.Now - startTime);
        logger.LogInformation($"\nTime Elapsed {timeElapsed}");

        return 0;

        static string FormatTimeSpan(TimeSpan timeSpan)
        {
            string timeSpanStr = timeSpan.ToString();
            int prettyLength = int.Min(11, timeSpanStr.Length);
            return timeSpanStr[..prettyLength];
        }
    }
}
