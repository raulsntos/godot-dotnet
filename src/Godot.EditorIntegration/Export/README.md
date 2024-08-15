# Godot.EditorIntegration (export)

This directory contains the .NET export plugin implementation which is registered by the editor integration to handle exporting Godot .NET projects.

The `DotNetExportPlugin` class implements the export plugin, it delegates the actual export logic to the first registered `PlatformExporter` implementation it finds that can support the target export platform.

The [PlatformExporters](./PlatformExporters) subdirectory contains all the `PlatformExporter` implementations for the platforms that we intend to support officially by default:

- [Desktop](./PlatformExporters/Desktop) (Windows, macOS, and Linux)
- [Android](./PlatformExporters/Android)
- [iOS](./PlatformExporters/IOS)
- [Browser](./PlatformExporters/Browser) (a.k.a. Web)

## Implementation

Implementing a `PlatformExporter` for a specific platform is only one side of what's required for Godot to support a given platform. The exporter implementation must ensure the output matches what the [`dotnet`](https://github.com/godotengine/godot/tree/master/modules/dotnet) module expects, so it can initialize the .NET runtime and load the .NET assemblies that were built when exporting the project.

The basic steps to implement a `PlatformExporter` are:

1. Implement `DetermineBuildOptions` to specify how the `dotnet publish` command should be executed to build the .NET project.
2. Implement `ExportAfterBuild` to specify how the output of `dotnet publish` should be included in the export output.

### DetermineBuildOptions

When implementing `DetermineBuildOptions`, the exporter can use the `PlatformExporterContext.AddBuild` API to specify the build options that will be used when building the .NET project. The exporter should add as many build options as required, each instance represents one execution of `dotnet publish`.

Most platforms just need to execute `dotnet publish` once for the target runtime identifier, but some platforms support multi-architecture exports which means we can build the .NET project for each of the architectures we want to support in the exported project and then we can add them all together and the resulting binary will be compatible with multiple architectures.

```csharp
public override void DetermineBuildOptions(PlatformExporterContext context)
{
	// Get the path to the .csproj file and the assembly name for the .NET project
	// we are going to build.
	string csprojPath = EditorPath.ProjectCSProjPath;
	string assemblyName = EditorPath.ProjectAssemblyName;

	context.AddBuild(new ExportBuildOptions()
	{
		// Specify the `dotnet publish` options to build the .NET project
		// for a 64-bit Linux machine.
		PublishOptions = new PublishOptions()
		{
			SlnOrProject = csprojPath,
			OutputPath = context.CreateTemporaryDirectory(),
			Configuration = "Release",
			RuntimeIdentifier = "linux-x64",
			SelfContained = true,
		},
		AssemblyName = EditorPath.ProjectAssemblyName,
		GodotPlatform = "Linux",
		GodotArchitecture = "x86_64",
	});
}
```

The example above adds one instance of build options hardcoded to build the .NET project for a 64-bit Linux machine. Usually, you'd want to retrieve more information from `context` to determine the target platform, architecture, and other configuration that may have been specified by the user through the export options.

### ExportAfterBuild

There are other virtual methods that can be implemented that will be called at different times of the export process. For example, `ExportAfterAllBuilds` is called after every execution of `dotnet publish`, which means if you are building a multi-architecture binary this is the right time to merge all of the `dotnet publish` outputs.

But `ExportAfterBuild` must always be implemented, the implementation should usually take the output of the last `dotnet publish` execution and add the files to the export using the `ExportPlugin` available in the `PlatformExporterContext`.

```csharp
public override void ExportAfterBuild(PlatformExporterContext context, ExportBuildOptions options)
{
	// Get the output path of the `dotnet publish` command.
	string outputPath = options.PublishOptions.OutputPath;

	// Get the name of the directory that will be used by the .NET module
	// to lookup the .NET assemblies, this is where the output of `dotnet publish`
	// so be stored so the .NET module can find it.
	string projectDataDirName = options.GetExportedDataDirectoryName();

	// Iterate all the files and directories contained in the output path.
	foreach (string path in Directory.EnumerateFiles(outputPath, "*", SearchOptions.AllDirectories))
	{
		// Determine the target path that the file will be copied to.
		string relativePath = Path.GetRelativePath(outputPath, path);
		string target = Path.Join(projectDataDirName, Path.GetDirectoryName(relativePath));

		// Use the Godot ExportPlugin API to add the file as a shared object.
		context.ExportPlugin.AddSharedObject(path, tags: null, target);
	}
}
```
