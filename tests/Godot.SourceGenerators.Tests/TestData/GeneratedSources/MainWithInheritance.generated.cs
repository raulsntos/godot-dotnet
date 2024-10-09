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
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::BaseType>(global::BaseType.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::DerivedType3>(global::DerivedType3.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::DerivedType>(global::DerivedType.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::DerivedType2>(global::DerivedType2.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::HighlyDerivedType>(global::HighlyDerivedType.BindMethods);
    }
    internal static void DeinitializeUserTypes(global::Godot.Bridge.InitializationLevel level)
    {
        if (level != global::Godot.Bridge.InitializationLevel.Scene)
        {
            return;
        }
    }
}
