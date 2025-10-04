using System.CommandLine;
using Godot.UpgradeAssistant.Cli.Commands;
using Microsoft.Build.Locator;

// Ensure MSBuildLocator is initialized before executing any code using MSBuild.
MSBuildLocator.RegisterDefaults();

var rootCommand = new CliRootCommand();
return await new CommandLineConfiguration(rootCommand).InvokeAsync(args);
