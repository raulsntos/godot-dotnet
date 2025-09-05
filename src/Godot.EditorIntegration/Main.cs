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

internal static class Main
{
    public static void InitializeTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Editor)
        {
            return;
        }

        GodotRegistry.RegisterInternalClass<DotNetEditorPlugin>(DotNetEditorPlugin.BindMethods);
        GodotRegistry.RegisterInternalClass<DotNetExportPlugin>(DotNetExportPlugin.BindMethods);

        GodotRegistry.RegisterInternalClass<MSBuildPanel>(MSBuildPanel.BindMethods);
        GodotRegistry.RegisterInternalClass<BuildProblemsView>(BuildProblemsView.BindMethods);
        GodotRegistry.RegisterInternalClass<BuildOutputView>(BuildOutputView.BindMethods);

        GodotRegistry.AddEditorPluginByType<DotNetEditorPlugin>();
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
