# Godot.EditorIntegration (internals)

This directory contains internal Godot APIs that aren't publicly exposed through ClassDB. Instead, the [`dotnet`](https://github.com/godotengine/godot/tree/master/modules/dotnet) module exposes these APIs that we need to implement the editor integration.

It's preferable to expose APIs through ClassDB first and only use this internal mechanism as a last resort when absolutely necessary, when there's no other way to access the required APIs and we don't want to expose the API publicly. Therefore, adding more APIs to `EditorInternal` should be extremely rare.
