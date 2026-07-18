using System.Runtime.CompilerServices;
using Godot.Collections;
using Godot.NativeInterop;

namespace Godot.Bindings.Tests;

// Regression guard for godot-dotnet#36. GodotArray<T>/GodotDictionary<TKey, TValue> register their
// Variant marshalling callbacks lazily, from their static constructors. Forcing the static
// constructor to run (as the fixed Marshalling.GenericConversion<T> and the generated BindMembers
// now do) must populate those callbacks.
//
// These run engine-free: RunClassConstructor triggers the static constructor without instantiating
// a native collection, and the callback fields are populated with function pointers to managed
// methods. They exercise the registration mechanism and its engine-safety; they pass under JIT
// today and are not the NativeAOT/trimming repro itself.
public class CollectionMarshallingRegistrationTests
{
    [Fact]
    public unsafe void RunClassConstructorRegistersGodotArrayMarshalling()
    {
        RuntimeHelpers.RunClassConstructor(typeof(GodotArray<int>).TypeHandle);

        Assert.True(Marshalling.GenericConversion<GodotArray<int>>.ToVariantCb != null);
    }

    [Fact]
    public unsafe void RunClassConstructorRegistersGodotDictionaryMarshalling()
    {
        RuntimeHelpers.RunClassConstructor(typeof(GodotDictionary<int, int>).TypeHandle);

        Assert.True(Marshalling.GenericConversion<GodotDictionary<int, int>>.ToVariantCb != null);
    }
}
