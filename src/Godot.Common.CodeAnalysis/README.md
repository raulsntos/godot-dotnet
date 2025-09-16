# Godot.Common.CodeAnalysis

This project contains shared helpers, utilities, or extensions that are used for across multiple projects.

Unlike the more general [Godot.Common](../Godot.Common) project, this project focuses on Roslyn utils that depend on the `Microsoft.CodeAnalysis.*` packages.

Rather than a normal C# project (`.csproj`), this shared project is just a collection of items to include in another project (`.projitems`). We do it this way because referencing other projects from an analyzer or source generator project would require us to pack the other project's DLL, which is a pain, and doesn't seem to work reliably.
