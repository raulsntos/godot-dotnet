# Godot .NET

This repository contains the .NET bindings for Godot using the GDExtension APIs.

> [!CAUTION]
> This repository is still a work in progress. API is subject to change and new versions may break compatibility until the first stable release.

Directory structure:

- [`src`](./src) - Source projects
- [`tests`](./tests) - Test projects
- [`samples`](./samples) - Sample projects
- [`eng`](./eng) - Build infrastructure (build scripts and MSBuild tasks)
- [`artifacts`](./artifacts) - Build outputs (nupkgs, dlls, pdbs, etc.)
- [`build.cmd`](./build.cmd) - Bootstrap the build for windows
- [`build.sh`](./build.sh) - Bootstrap the build for *nix
- [`test.cmd`](./test.cmd) - Shortcut for `build.cmd -test`
- [`test.sh`](./test.sh) - Shortcut for `build.sh --test`

## Generate the bindings

To generate the bindings, see the instructions in the [Godot.BindingsGenerator](src/Godot.BindingsGenerator) tool project.

Normally the tool does not need to be used directly, use the `build.sh`/`build.cmd` scripts at the root of the repository to build the projects in this repository which will also generate the bindings. See more instructions below.

## Build the bindings

Use the `build.sh`/`build.cmd` scripts at the root of the repository. Use the `--help` argument to get usage information. See the [build infrastructure documentation](eng/common) for more detailed information.

To produce a development build and push the packages to a local NuGet feed so they can be used during development:

```bash
./build.sh --productBuild --pushNupkgsLocal ~/MyLocalNuGetSource
```

To produce a stable build for a custom fork of Godot, generate the `extension_api.json` and `gdextension_interface.h` files from the custom engine build and replace the files in the `gdextension` directory. Then, build the stable packages:

```bash
./build.sh --productBuild --ci /p:OfficialBuildId=20240319.1 /p:FinalVersionKind=release
```

More information about package versioning and the _build kinds_ in the [build infrastructure documentation](eng/common).

The output of the build will be stored in the `artifacts` directory at the root of the repository. Only shipping packages are meant to be published through official channels.

## Use the bindings

To use the bindings in a C# project, see the instructions in the [Godot.Bindings](src/Godot.Bindings) project.
