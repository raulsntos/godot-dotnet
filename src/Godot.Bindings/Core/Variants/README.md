# Godot.Bindings (Variants)

This directory contains the Variant types. These types are not generated to avoid the overhead of interop with the engine because they are meant to be fast. The struct types should match the same layout as the engine types so they can be used for interop, otherwise there will be an interop struct in [InteropStructs](../NativeInterop/InteropStructs) that the type should be converted to/from when doing interop.
