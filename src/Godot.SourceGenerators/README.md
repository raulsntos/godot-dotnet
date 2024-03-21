# Godot.SourceGenerators

C# source generators for the .NET Godot bindings.

## Usage

The source generators generate the C# code that registers your extension classes in ClassDB to avoid manually calling the [`Godot.Bridge`](../Godot.Bindings/Bridge) API.

To register a class, annotate it with the `[GodotClass]` attribute. Then, annotate the members that you want to expose with the `[BindProperty]` and `[BindMethod]` attributes. Delegates can be annoted with the `[Signal]` attribute to register a signal.

```csharp
[GodotClass]
public partial class Summator : RefCounted
{
	private int _count;

	[BindMethod]
	public void Add(int value = 1)
	{
		_count += value;
	}

	[BindMethod]
	public void Reset()
	{
		_count = 0;
	}

	[BindMethod]
	public int GetTotal()
	{
		return _count;
	}
}
```

The attributes contain optional properties that can be set to configure how the class and its members will be exposed. By default, a class will be registered as a _runtime_ class, which means it won't run in the editor. To let the class run in the editor, use the `Tool` property of the `[GodotClass]` attribute.

```csharp
[GodotClass(Tool = true)]
public partial class Sumator : RefCounted { }
```

You can specify a different name for the exposed members that may be useful to match the engine conventions where snake_case naming is preferred.

```csharp
[BindProperty(Name = "count")]
public int Count { get; set; }

[BindMethod(Name = "get_total")]
public int GetTotal() { ... }
```

The types used in the exposed members must be Variant-compatible, that is, a type that can be marshalled as a Variant.
