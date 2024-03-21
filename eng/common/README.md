# Common build infrastructure

Contains the common properties, targets, and build scripts used by every project in the repository. Adapted from Microsoft's [Arcade SDK](https://github.com/dotnet/arcade).

## Features

The [tools](./tools) directory contains the following features:

- **Build.proj** \
	MSBuild project that contains the logic to build the solution. Executed by the `build.sh`/`build.ps1` scripts with the information collected from the arguments. It can be considered the real entry-point of the build system. See the [_Basic usage_](#Basic-usage) section for more information.

- **Publish.proj** \
	MSBuild project that contains the logic to publish the artifacts produced by the build. It's used to copy the packages to a local NuGet feed publishing directory, usually a local source used by contributors to consume the built packages during development. See the [_Basic usage_](#Basic-usage) section for more information.

- **ExcludeFromBuild** \
	Describes the properties to exclude a project from the build. Test projects are automatically excluded from _product builds_. See the [_Basic usage_](#Basic-usage) section for more information.

- **RepoLayout** \
	Describes the layout of the repository with properties like `RepoRoot` that contains the path to the root directory of this repository, configures the path to the .NET tool, and sets the output paths to the `artifacts` directory at the root of the repository.

- **ProjectDefaults** \
	Describes common default properties for the projects in the repository. Includes authors, license, icon, readme, and other metadata that is common to every project.

- **Versions** \
	Describes common version properties for the projects in the repository. Includes a target that calculates a version suffix from the current date that can be used when building nightly packages. See the [_Versioning and build kind_](#Versioning-and-build-kind) section for more information.

- **Tests** \
	Describes whether a project is a test project and, if so, imports common dependencies. Includes the `Test` target that runs the unit tests in the project using the XUnit test runner. See the [_Testing_](#Testing) section for more information.

- **Nuspec** \
	Describes nuspec properties from MSBuild properties so they can be used when authoring nuspec files.

The [tasks](./tasks) directory contains the project that contains MSBuild tasks used by the build infrastructure. This project is built by the build scripts before building the repository.

## Common properties

- `IsPackable` \
	Determines whether the project will be packaged in a `.nupkg` file to be distributed.

- `IsShipping` \
	Determines whether the project packages will be distributed through official channels. By default, every project is considered shipping. Set this property to `false` to prevent pushing packages that shouldn't be shipped.

- `IsTestProject` \
	Determines whether a project is a unit test project. This is automatically set if the project name ends with `.Tests` following .NET conventions. Test projects automatically include the test framework packages (XUnit) and will be run by the build script when the `--test` argument is provided. See the [_Testing_](#Testing) section for more information.

- `OfficialBuildId` \
	Determines whether we are building an _official build_. See the [_Build kind_](#Build-kind) section for more information. When specified it automatically sets the `OfficialBuild` property to `true`.

- `ExcludeFromBuild` \
	Determines whether the project should be skipped when building the solution. See the [_Basic usage_](#Basic-usage) section for more information.

- `ExcludeFromProductBuild` \
	Determines whether the project should be skipped when building the solution for a _product build_. See the [_Basic usage_](#Basic-usage) section for more information.

## Basic usage

To build the solution use the `build.sh`/`build.cmd` scripts at the root of the repository. Use the `--help` argument to get usage information. Command line arguments not listed in the usage information are passed thru to MSBuild.

By default the build scripts will build every project in the solution but individual projects can be specified with the `--projects` argument (allows globbing and relative paths).

```bash
# Build all projects.
./build.sh --build

# Build 'Godot.Bindings' project.
./build.sh --build --projects ./src/Godot.Bindings/Godot.Bindings.csproj
```

To produce the NuGet packages use the `--pack` argument.

```bash
# Builds all projects and produces NuGet packages for all the packable projects.
./build.sh --build --pack
```

The `--productBuild` argument is the argument used to create a **_product build_**. A _product build_ is a build produced in the way it will be built for distribution. It implies restore, build and pack, so it can be used as a _shorthand_. It will also skip building projects that set the `ExcludeFromProductBuild` property to `true`, test projects set it to `true` by default. The `ExcludeFromBuild` property is a more general property that can be used to exclude projects from every build. Excluding projects from the build means none of the targets are executed (build, restore, pack, etc.)

The output of the build will be stored in the `artifacts` directory at the root of the repository. Produced packages will be under the `packages` subdirectory.

To publish the built packages use the `--publish` argument and specify the local NuGet feed publishing directory in the `OutputBlobFeedDir` property. Or use the `--pushNupkgsLocal` argument as a shorthand.

```bash
# Restores, builds and packages all projects, then publishes them to the specified path.
./build.sh --restore --build --pack --publish /p:OutputBlobFeedDir=~/MyLocalNuGetSource

# Similar to the command above but test projects are excluded.
./build.sh --productBuild --pushNupkgsLocal ~/MyLocalNuGetSource
```

## Versioning and build kind

The version used by the packages produced by the build scripts depends on the [_build kind_](https://github.com/dotnet/arcade/blob/777bc46bd883555cf89b8a68e3e2023fd4f1ee50/Documentation/CorePackages/Versioning.md#build-kind). The kinds of builds that can be produced are listed below:

- `dev` (e.g.: `4.2.0-dev`) - A local development build, this is the default when building locally and the kind of builds that contributors usually make during development.

- `ci` (e.g.: `4.2.0-ci`) - A CI build or PR validation build, this is the kind of build that is produced in CI workflows to validate PRs.

- `nightly` (e.g.: `4.2.0-nightly.24179.1`) - A preview build, this is an _official build_. The version contains a short date and the `nightly` prerelease label. This is the kind of build produced for automated nightly releases.

- `prerelease` (e.g.: `4.2.0-beta.1`) - A prerelease build, this is an _official build_. The version contains a prerelease label. This is the kind of build produced for official preview releases.

- `release` (e.g.: `4.2.0`) - A stable build, this is an _official build_. This is the kind of build produced for stable releases.

An **_official build_** is referred to as such because it's produced with the intention to be distributed through the official channels but they are also produced by users that want to produce stable packages (i.e.: an user building packages for their custom fork of Godot).

Producing _official builds_ requires the `--ci` argument because they are meant to be built using CI. They also require specifying the `OfficialBuildId` property, this is the current date and the revision number in the format `yyyymmdd.r`, for example `20240319.1` would be a build made on March 19, 2024 with revision 1.

The official `nightly` builds are only distributed using the private NuGet feed. The official `prerelease` and `release` builds are distributed using the NuGet.org feed and match the engine releases.

To produce `prelease` or `release` builds, the `FinalVersionKind` must also be set to `prerelease` or `release` respectively. If the build is a prerelease, use the `PreReleaseVersionLabel` property to set the prerelease label (i.e.: `alpha`, `beta`, `rc`).

For example, to build stable packages with version 4.2.0:

```bash
./build.sh --productBuild --ci /p:VersionPrefix=4.2.0 /p:OfficialBuildId=20240319.1 /p:FinalVersionKind=release
```

## Testing

To test the solution use the `test.sh`/`test.cmd` scripts at the root of the repository. Use the `--help` argument to get usage information.

These scripts are a shorthand for the `build.sh`/`build.cmd` scripts that use the `--test` argument. You may need to also use the `--build` argument if you haven't built the projects yet.

Test projects in this repository use XUnit, and the test script will use the XUnit runner. The test results can be found in the `artifacts/TestResults` directory at the root of the repository, and the logs in the `artifacts/log` directory.

By default the test scripts will execute all the test projects in the solution (the projects with the `IsTestProject` property set to `true`), use the `--projects` argument to specify which individual projects to test (allows globbing and relative paths).
