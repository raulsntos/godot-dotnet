using System.Threading;
using System.Threading.Tasks;
using Godot.UpgradeAssistant.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Godot.UpgradeAssistant.Cli.Commands;

internal sealed class AnalyzeCommand : AssistantCommandBase
{
    public AnalyzeCommand() : base("analyze", SR.AnalyzeCommand_Description) { }

    protected override async Task HandleInvocationAsync(IHost host, AssistantCommandOptions options, CancellationToken cancellationToken = default)
    {
        var upgrader = host.Services.GetRequiredService<UpgradeService>();
        await upgrader.RunAsync(new()
        {
            GodotProjectFilePath = options.GodotProject.FullName,
            DotNetSolutionFilePath = options.DotNetSolution.FullName,
            DotNetProjectFilePath = options.DotNetProject.FullName,
            TargetGodotVersion = options.TargetGodotVersion,
            EnableGodotDotNetPreview = options.EnableGodotDotNetPreview,
            ExportFilePath = options.ExportFilePath?.FullName,
            IsDryRun = true,
        }, cancellationToken);
    }
}
