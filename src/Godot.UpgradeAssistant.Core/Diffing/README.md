# Godot.UpgradeAssistant.Core (diffing)

This directory contains helpers to merge and apply text changes to .NET documents. This allows applying multiple code fixes that may intersect or overlap (such as adding the `partial` modifier and the `[GodotClass]` attribute to a class).

These files are derived from an implementation originally in the [Roslyn](https://github.com/dotnet/roslyn) repository and should be kept in sync with minimal changes.
