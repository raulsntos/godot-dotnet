# Godot.Common.CodeAnalysis (enums)

This directory contains enums that mirror those defined in [`Godot.Bindings`](../../Godot.Bindings). These enums are used by the analyzers and source generators to retrieve their values from the analyzed source code, write generated code with their values, and check whether certain types are Variant compatible, so their values must be kept in sync with the values in the bindings to prevent issues. A comment at the top of each file indicates the path of the original file that the enum was copied from.
