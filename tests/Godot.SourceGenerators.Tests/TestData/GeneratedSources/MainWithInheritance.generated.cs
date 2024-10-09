[assembly: global::System.Runtime.CompilerServices.DisableRuntimeMarshalling]

#nullable enable

namespace TestProject;

static partial class Main
{
    internal static void InitializeTypes(global::Godot.Bridge.InitializationLevel level)
    {
        ClassDBExtensions.InitializeUserTypes(level);
    }
    internal static void DeinitializeTypes(global::Godot.Bridge.InitializationLevel level)
    {
        ClassDBExtensions.DeinitializeUserTypes(level);
    }
    [global::System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "init")]
    internal static bool Init(nint getProcAddress, nint library, nint initialization)
    {
        global::Godot.Bridge.GodotBridge.Initialize(getProcAddress, library, initialization, config =>
        {
            config.SetMinimumLibraryInitializationLevel(global::Godot.Bridge.InitializationLevel.Scene);
            config.RegisterInitializer(InitializeTypes);
            config.RegisterTerminator(DeinitializeTypes);
        });
        return true;
    }
}
internal static class ClassDBExtensions
{
    internal static void InitializeUserTypes(global::Godot.Bridge.InitializationLevel level)
    {
        if (level != global::Godot.Bridge.InitializationLevel.Scene)
        {
            return;
        }
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::NS.BaseType>(global::NS.BaseType.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::NS.DerivedType3>(global::NS.DerivedType3.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::NS.DerivedType>(global::NS.DerivedType.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::NS.DerivedType2>(global::NS.DerivedType2.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::NS.HighlyDerivedType>(global::NS.HighlyDerivedType.BindMethods);
    }
    internal static void DeinitializeUserTypes(global::Godot.Bridge.InitializationLevel level)
    {
        if (level != global::Godot.Bridge.InitializationLevel.Scene)
        {
            return;
        }
    }
}
