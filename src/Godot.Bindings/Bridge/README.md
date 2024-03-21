# Godot.Bridge

This directory contains the APIs that bridge the C# code with the engine through the GDExtension APIs. It contains low-level APIs to initialize the GDExtension and register classes.

The `GodotBridge.Initialize` API can be used to setup the GDExtension interface APIs and initialize the GDExtension, and the `ClassDB` APIs can be used to register extension classes and their members. These APIs can be used directly, or the [source generators](../../Godot.SourceGenerators) can be used to generate them.
