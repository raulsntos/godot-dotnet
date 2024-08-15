# Godot.PluginLoader

This project implements an `AssemblyLoadContext` used by Godot to load .NET assemblies with hostfxr. This allows Godot to unload the loaded .NET assemblies, so they can be reloaded when the .NET project is rebuilt.

This package should not be referenced directly by users, it will be retrieved on-demand by the Godot editor.

## Usage

For each .NET GDExtension, load its assembly using the `Main.LoadAssembly` method and unload it using the `Main.UnloadAssembly` method. These methods are very similar to hostfxr and use the same conventions.

Each assembly loaded this way creates its own isolated `AssemblyLoadContext` to load the assembly and its dependencies.
