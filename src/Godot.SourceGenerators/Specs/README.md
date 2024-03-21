# Godot.SourceGenerators (Specs)

The specs are the model types that contain the information collected from the C# syntax and symbols by the generators.

The purpose of these types is to collect the necessary data to produce the generated source code, incremental generators can cache this data so we can avoid collecting it on every key stroke. In order to prevent accidentally invalidating the cache and ensuring the generators performance is not degraded, these types must follow these rules:

- Must implement the `IEquatable<T>` interface so they can be compared. Comparing these types is very important because that's how incremental generators decide if the data needs to be collected again.
- Must not contain any `ISymbol` types. These types are never equatable, and holding them can potentially root compilations preventing Roslyn from freeing unnecessary memory.
- Must not contain any `SyntaxNode` types. These types are not equatable between runs. They can also potentially root compilations but not as much as symbols.
- Always use `EquatableArray<T>` for collections of data. This is a custom type that wraps a `T[]` and makes it equatable so it can be compared.

More best practices of writing incremental generators in the [cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md).
