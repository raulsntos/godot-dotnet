# Godot.Common.CodeAnalysis (polyfills)

This directory contains _polyfills_ for multiple features introduced in future versions of .NET that we can't otherwise use when targetting `netstandard2.0`. These features are available when using a newer version of C# as long as we provide the APIs ourselves.

These files are derived from an implementation originally in the [.NET Runtime](https://github.com/dotnet/runtime) repository and should be kept in sync with minimal changes.
