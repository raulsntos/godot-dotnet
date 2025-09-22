using System.CommandLine;
using Godot.BindingsGenerator;

var rootCommand = new GenerateCommand();
return await new CommandLineConfiguration(rootCommand).InvokeAsync(args);
