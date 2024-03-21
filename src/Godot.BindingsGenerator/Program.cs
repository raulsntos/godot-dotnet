using System.CommandLine;
using Godot.BindingsGenerator;

var rootCommand = new GenerateCommand();
return await new CliConfiguration(rootCommand).InvokeAsync(args);
