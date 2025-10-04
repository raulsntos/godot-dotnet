using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Godot.UpgradeAssistant.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Godot.UpgradeAssistant.Cli.Commands;

internal abstract class AssistantCommandBase : Command
{
    private static readonly Argument<FileInfo> _godotProjectArgument = new Argument<FileInfo>(
        name: "project")
    {
        Description = SR.AssistantCommandBase_ArgumentGodotProjectDescription,
        Arity = ArgumentArity.ExactlyOne,
    }.AcceptExistingOnly();

    private static readonly Option<FileInfo> _dotnetSolutionOption = new Option<FileInfo>(
        name: "--solution")
    {
        Description = SR.AssistantCommandBase_OptionDotNetSolutionDescription,
        Required = true,
    }.AcceptExistingOnly();

    private static readonly Option<FileInfo> _dotnetProjectOption = new Option<FileInfo>(
        name: "--project")
    {
        Description = SR.AssistantCommandBase_OptionDotNetProjectDescription,
        Required = true,
    }.AcceptExistingOnly();

    private static readonly Option<SemVer> _targetGodotVersionOption = new(
        name: "--target-godot-version", aliases: ["-t"])
    {
        Description = SR.AssistantCommandBase_OptionTargetGodotVersionDescription,
        DefaultValueFactory = _ => Constants.LatestGodotVersion,
        CustomParser = (ArgumentResult result) =>
        {
            if (result.Tokens.Count == 0)
            {
                // Default to the latest known Godot version.
                return Constants.LatestGodotVersion;
            }

            if (result.Tokens.Count != 1)
            {
                result.AddError(SR.AssistantCommandBase_OptionTargetGodotVersionError_MoreThanOneVersionSpecified);
                return default;
            }

            if (!SemVer.TryParse(result.Tokens[0].Value, out var version))
            {
                result.AddError(SR.FormatAssistantCommandBase_OptionTargetGodotVersionError_InvalidFormat(result.Tokens[0].Value));
                return default;
            }

            if (version > Constants.LatestGodotVersion)
            {
                result.AddError(SR.FormatAssistantCommandBase_OptionTargetGodotVersionError_UnsupportedVersion(version, Constants.LatestGodotVersion));
                return default;
            }

            return version;
        },
    };

    private static readonly Option<bool> _enableGodotDotNetPreviewOption = new(
        name: "--enable-preview")
    {
        Description = SR.AssistantCommandBase_OptionEnableGodotDotNetDescription,
    };

    private static readonly Option<bool> _verboseOption = new(
        name: "--verbose", aliases: ["-v"])
    {
        Description = SR.AssistantCommandBase_OptionVerboseDescription,
    };

    private static readonly Option<FileInfo?> _exportFilePathOption = new(
        name: "--output", aliases: ["-o"])
    {
        Description = SR.AssistantCommandBase_OptionOutputDescription,
        DefaultValueFactory = _ => new FileInfo("godot-upgrade-assistant.summary.{TimeStamp}.html"),
    };

    private static readonly Option<bool> _noSummaryOption = new(
        name: "--no-summary")
    {
        Description = SR.AssistantCommandBase_OptionNoSummaryDescription,
    };

    private bool IsVerbose { get; set; }

    protected AssistantCommandBase(string name, string? description = null) : base(name, description)
    {
        Arguments.Add(_godotProjectArgument);
        Options.Add(_dotnetSolutionOption);
        Options.Add(_dotnetProjectOption);
        Options.Add(_targetGodotVersionOption);
        Options.Add(_enableGodotDotNetPreviewOption);
        Options.Add(_verboseOption);
        Options.Add(_exportFilePathOption);
        Options.Add(_noSummaryOption);

        SetAction(HandleCommand);
    }

    private AssistantCommandOptions HandleParseResult(ParseResult parseResult)
    {
        var godotProject = parseResult.GetValue(_godotProjectArgument)!;
        var dotnetSolution = parseResult.GetValue(_dotnetSolutionOption)!;
        var dotnetProject = parseResult.GetValue(_dotnetProjectOption)!;
        var targetGodotVersion = parseResult.GetValue(_targetGodotVersionOption);
        bool enableGodotDotNetPreview = parseResult.GetValue(_enableGodotDotNetPreviewOption);
        var exportFilePath = parseResult.GetValue(_exportFilePathOption);

        IsVerbose = parseResult.GetValue(_verboseOption);

        if (parseResult.GetValue(_noSummaryOption))
        {
            exportFilePath = null;
        }

        if (targetGodotVersion > Constants.LastSupportedGodotSharpVersion)
        {
            // If the target Godot version already supports the Godot .NET bindings without preview.
            // Disable the preview option which is now redundant and will throw exceptions otherwise.
            enableGodotDotNetPreview = false;
        }

        return new AssistantCommandOptions()
        {
            GodotProject = godotProject,
            DotNetSolution = dotnetSolution,
            DotNetProject = dotnetProject,
            TargetGodotVersion = targetGodotVersion,
            EnableGodotDotNetPreview = enableGodotDotNetPreview,
            ExportFilePath = exportFilePath,
        };
    }

    protected abstract Task HandleInvocationAsync(IHost host, AssistantCommandOptions options, CancellationToken cancellationToken = default);

    protected virtual void Configure(IHostBuilder builder)
    {
        const string LogFilePath = "godot-dotnet-upgrade-assistant.log.clef";

        builder
            .ConfigureServices(services => services
                .AddSingleton(new ProviderService())
                .AddSingleton<UpgradeService>())
            .UseSerilog((context, _, loggerConfiguration) => loggerConfiguration
                .Enrich.FromLogContext()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Console(
                    levelSwitch: new LoggingLevelSwitch(IsVerbose ? LogEventLevel.Verbose : LogEventLevel.Information),
                    formatProvider: CultureInfo.CurrentCulture)
                .WriteTo.File(new CompactJsonFormatter(), LogFilePath, levelSwitch: new LoggingLevelSwitch(LogEventLevel.Debug)));
    }

    private async Task<int> HandleCommand(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var options = HandleParseResult(parseResult);

        var builder = Host.CreateDefaultBuilder();
        Configure(builder);

        var host = builder.Build();

        await HandleInvocationAsync(host, options, cancellationToken);

        return 0;
    }
}
