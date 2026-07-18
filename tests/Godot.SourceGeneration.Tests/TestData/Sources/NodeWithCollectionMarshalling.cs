using Godot;
using Godot.Collections;

namespace NS;

// Verifies that the generated BindMembers forces the static constructor of every distinct
// generic GodotArray<...>/GodotDictionary<...> used by a bound member (so their lazy marshalling
// registration runs under trimming/NativeAOT), while ignoring the non-generic and packed
// collection types (which register eagerly).

[GodotClass]
public partial class NodeWithCollectionMarshalling : Node
{
    // Reported case: bound property whose marshal type is a generic GodotArray<T>.
    [BindProperty]
    public GodotArray<int> ArrayProperty { get; set; }

    [BindProperty]
    public GodotDictionary<int, string> DictionaryProperty { get; set; }

    // Method parameter reuses GodotArray<int> (should be deduped with ArrayProperty).
    [BindMethod]
    public void MethodWithArrayParameter(GodotArray<int> array) { }

    // Method return introduces a distinct collection type.
    [BindMethod]
    public GodotArray<float> MethodThatReturnsArray() => [];

    // Non-generic GodotArray/GodotDictionary register eagerly, so no RunClassConstructor is emitted.
    [BindProperty]
    public GodotArray UntypedArrayProperty { get; set; }

    [BindProperty]
    public GodotDictionary UntypedDictionaryProperty { get; set; }

    // Packed arrays are core Variant types, so no RunClassConstructor is emitted.
    [BindProperty]
    public PackedInt32Array PackedProperty { get; set; }

    // Signal parameter introduces a distinct generic collection type (GodotArray<double>) not used
    // by any other bound member, so its RunClassConstructor line is attributable to the signal walk.
    [Signal]
    public delegate void CollectionSignalEventHandler(GodotArray<double> values);
}
