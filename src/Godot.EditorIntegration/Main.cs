using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.EditorIntegration.Build.UI;
using Godot.EditorIntegration.Export;
using Godot.EditorIntegration.Internals;

[assembly: DisableRuntimeMarshalling]
[assembly: DisableGodotEntryPointGeneration]

namespace Godot.EditorIntegration;
using ClassDB = Godot.Bridge.ClassDB;

internal static class Main
{
    public static void InitializeTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Editor)
        {
            return;
        }

        ClassDB.RegisterClass<DotNetEditorPlugin>(DotNetEditorPlugin.BindMethods);
        ClassDB.RegisterClass<DotNetExportPlugin>(DotNetExportPlugin.BindMethods);

        ClassDB.RegisterClass<MSBuildPanel>(MSBuildPanel.BindMethods);
        ClassDB.RegisterClass<BuildProblemsView>(BuildProblemsView.BindMethods);
        ClassDB.RegisterClass<BuildOutputView>(BuildOutputView.BindMethods);

        EditorPlugins.AddByType<DotNetEditorPlugin>();
    }

    public static void DeinitializeTypes(InitializationLevel level)
    {
    }

    // Initialization

    [UnmanagedCallersOnly]
    public static bool Init(nint getProcAddress, nint library, nint initialization)
    {
        GodotBridge.Initialize(getProcAddress, library, initialization, config =>
        {
            config.SetMinimumLibraryInitializationLevel(InitializationLevel.Editor);
            config.RegisterInitializer(InitializeTypes);
            config.RegisterTerminator(DeinitializeTypes);
        });

        EditorInternal.Initialize(getProcAddress);

        return true;
    }
}
