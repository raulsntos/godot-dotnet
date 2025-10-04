using System.CommandLine;

namespace Godot.UpgradeAssistant.Cli.Commands;

internal sealed class CliRootCommand : RootCommand
{
    public CliRootCommand()
    {
        Add(new AnalyzeCommand());
        Add(new UpgradeCommand());
    }
}
