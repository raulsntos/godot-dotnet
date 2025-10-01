# Godot .NET upgrade assistant (Providers)

Implements analysis and upgrade providers for the [Godot .NET upgrade assistant](../Godot.UpgradeAssistant.Cli). The analysis providers implement the logic to find out if changes need to be applied to the C# project, and the upgrade providers implement the logic to apply the needed changes.

A special pair of analysis and upgrade providers, `DotNetAnalyzersAnalyzeProvider` and `DotNetCodeFixersUpgradeProvider` use Roslyn analyzers and code fixers to implement the providers. This allows taking advantage of the powerful Roslyn APIs to implement the providers.

## Features

- **API mappings** \
	Simple upgrade mechanism for APIs that are renamed across different versions of Godot. If the API has been removed or can't be handled by the upgrade assistant, a comment will be added instead to let the user know that the old API is no longer available and they'll need to take manual steps to finish the upgrade. The comment can include information about why the API was removed, direct the user towards an available API that could serve as a replacement, or include a link to a documentation page explaining what steps the user could take to resolve the issue.

- **Godot .NET SDK** \
	The MSBuild SDK used by the `.csproj` will be upgraded to match the version required by the target Godot version.

- **.NET TargetFramework** \
	Newer versions of Godot may require the project to target a newer version of .NET. The `.csproj` will be upgraded to target the minimum required version if the current target version is incompatible.

- **Package compatibility** \
	Checks that the projects and packages referenced by the Godot project are still compatible after upgrading the project. Upgrading the .NET version targeted by the project may result in incompatibility if those packages target an unsupported .NET version.

- **Solution build configurations** \
	Ensure the build configurations specified in the `.sln` match the build configurations used by the target Godot version (`Debug` and `Release` -or- `Debug`, `ExportDebug`, and `ExportRelease`).

- **Enable Godot .NET preview** \
	Enable the MSBuild property `<EnableGodotDotNetPreview>` so the Godot .NET SDK uses the Godot .NET packages instead of GodotSharp. This is a temporary MSBuild property while both bindings are supported and will eventually be dropped when GodotSharp is replaced by Godot .NET.
