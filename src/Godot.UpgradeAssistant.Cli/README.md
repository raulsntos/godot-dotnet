# Godot .NET upgrade assistant

Tool to upgrade Godot .NET projects heavily inspired by the [.NET upgrade assistant](https://github.com/dotnet/upgrade-assistant). This tool only upgrades C# code. Scenes and resources must be upgraded by opening the project with the Godot editor.

Projects should be targetting the latest Godot 3.x version before upgrading (other 3.x versions might also work but it hasn't been tested). For better results make sure the project compiles before upgrading and that there are no deprecation warnings.

The upgrade process works a lot better if your project targets Godot 4.x, since changes in minor 4.x versions try to avoid breaking compatibility as much as possible.

This tool is intended to be used to upgrade to the latest version of Godot, upgrading to any other version may be possible but it's not guaranteed to work.

This tool doesn't need to be used directly by users, it will be used by the Godot editor when needed.

See the [Godot.UpgradeAssistant.Providers](../Godot.UpgradeAssistant.Providers) project for more information about the features provided by this tool.

## Installation

Since this is a [.NET tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools), it can be installed like any other .NET tool:

```bash
dotnet tool install --global Godot.UpgradeAssistant.Cli
# or update
dotnet tool update --global Godot.UpgradeAssistant.Clii
```

Check installed version:

```bash
godot-upgrade-assistant --version
```

## Usage

Once installed, the tool can be executed using verbs `analyze` or `upgrade`:

- **analyze** executes a _dry-run_. It only analyzes the Godot project and reports the problems it finds, but it doesn't attempt to fix any of them.
- **upgrade** analyzes the Godot project to find problems and then attempts to fix as many of them as it can, reporting the results.

Basic example:

```bash
godot-upgrade-assistant upgrade /path/to/project.godot \
	--solution /path/to/solution.sln \
	--project /path/to/project.csproj
```

Optional arguments:

- **--target-godot-version**: SemVer 2 format (e.g.: `4.5.0-dev`). When not provided, it defaults to the latest version known by the tool.
- **--verbose**: Enables verbose logging to the console.
- **--output <path>**: Specify a different path and filename for the report file. When not provided, it will write the report file to the current working directory with a timestamped name.
- **--no-summary**: Disable generating a report file.

See more information with `godot-upgrade-assistant upgrade --help` or `godot-upgrade-assistant analyze --help`.
