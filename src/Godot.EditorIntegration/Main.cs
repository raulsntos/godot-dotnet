using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.EditorIntegration.Build.UI;
using Godot.EditorIntegration.Export;
using Godot.EditorIntegration.Internals;
using Godot.EditorIntegration.UpgradeAssistant;

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

        GodotRegistry.RegisterInternalClass<DotNetEditorPlugin>(DotNetEditorPlugin.BindMembers);
        GodotRegistry.RegisterInternalClass<DotNetExportPlugin>(DotNetExportPlugin.BindMembers);
        GodotRegistry.RegisterInternalClass<DotNetEditorExtensionSourceCodePlugin>(DotNetEditorExtensionSourceCodePlugin.BindMembers);

        GodotRegistry.RegisterInternalClass<MSBuildPanel>(MSBuildPanel.BindMembers);
        GodotRegistry.RegisterInternalClass<BuildProblemsView>(BuildProblemsView.BindMembers);
        GodotRegistry.RegisterInternalClass<BuildOutputView>(BuildOutputView.BindMembers);

        GodotRegistry.RegisterInternalClass<ResourceFormatLoaderCSharpScript>(ResourceFormatLoaderCSharpScript.BindMembers);
        GodotRegistry.RegisterInternalClass<CSharpScript>(CSharpScript.BindMembers);

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
