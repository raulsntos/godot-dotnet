# Godot.EditorIntegration (build)

This directory contains code related with building .NET projects, a wrapper around the `dotnet` CLI, and the UI controls used by the editor integration to display the build output to the user.

## dotnet CLI wrapper

The [Cli](./Cli) directory contains the `DotNetCli` type that provides a convenient way to execute the `dotnet` CLI from C#.

All the available `dotnet` CLI options are defined in `DotNetCliOptions`. These options should exactly match the options from the `dotnet` CLI. We only define the options we use, so some options may be missing.

`DotNetCli` is a low-level API and shouldn't be used directly, instead `BuildManager` should always be used to build a .NET project because it adds Godot-specific behavior.

## BuildManager

`BuildManager` provides a convenient wrapper over `DotNetCli` that includes Godot-specific behavior like logging MSBuild diagnostics, showing progress dialogs, etc.

The APIs available in `BuildManager` use the convenient Options types that determine which CLI arguments will be used when executing the `dotnet` CLI. We try to match the `dotnet` CLI options closely but sometimes we may differ a bit for convenience.

Since `BuildManager` options require using .NET names, we also implement `GodotPlatform`, `RuntimeIdentifierOS` and `RuntimeIdentifierArchitecture` to help determine the current/target platform and convert between the Godot and .NET names.

```csharp
// Assume that the method below returns a struct with the target OS and architecture
// for an export, and it uses Godot names everywhere.
var exportInfo = GetGodotExportInformation();

if (GodotPlatform.IsWindows(exportInfo.OS))
{
	RuntimeIdentifierOS ridOS = RuntimeIdentifierOS.Win;
	RuntimeIdentifierArchitecture ridArch = RuntimeIdentifierArchitecture.X64;

	if (ridArch.GodotName != exportInfo.Architecture)
	{
		throw new NotSupportedException($"Architecture '{exportInfo.Architecture}' not supported.");
	}

	// This should result in the runtime identifier 'win-x64'
	// which can now be used with BuildManager.
	string rid = $"{ridOS}-{ridArch}";
}
```

## UI

The [UI](./UI) directory contains Godot controls like the MSBuild panel that are used by the editor integration to display the build output to users.
