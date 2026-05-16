using System.Diagnostics;
using System.Runtime.Versioning;
using Godot.Collections;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;

namespace Godot.EditorIntegration.Internals;

unsafe partial class EditorInternal
{
    private static delegate* unmanaged[Cdecl]<void> _module_complete_initialization;

    public static void ModuleCompleteInitialization()
    {
        Debug.Assert(_module_complete_initialization is not null);
        _module_complete_initialization();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _module_fail_initialization;

    public static void ModuleFailInitialization(string errorMessage)
    {
        Debug.Assert(_module_fail_initialization is not null);
        using NativeGodotString errorMessageNative = NativeGodotString.Create(errorMessage);
        _module_fail_initialization(&errorMessageNative);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, long*, long*, void> _module_get_assembly_load_state;

    public static void ModuleGetAssemblyLoadState(out string? assemblyName, out AssemblyLoadState loadState)
    {
        Debug.Assert(_module_get_assembly_load_state is not null);
        using NativeGodotString assemblyNameNative = default;
        long initState = 0;
        long failedState = 0;
        _module_get_assembly_load_state(&assemblyNameNative, &initState, &failedState);
        assemblyName = assemblyNameNative.ToString();
        loadState = (InitState)initState switch
        {
            InitState.Uninitialized => AssemblyLoadState.NotLoaded,
            InitState.Initializing => AssemblyLoadState.Loading,
            InitState.Initialized => AssemblyLoadState.Loaded,
            InitState.Failed => (AssemblyLoadFailedState)failedState switch
            {
                AssemblyLoadFailedState.ProjectNotFound =>
                    AssemblyLoadState.ProjectNotFound,
                AssemblyLoadFailedState.DllNotFound =>
                    AssemblyLoadState.DllNotFound,
                AssemblyLoadFailedState.FailedToLoad =>
                    AssemblyLoadState.FailedToLoad,
                AssemblyLoadFailedState.FailedToLoad_GDExtensionEntryPoint =>
                    AssemblyLoadState.FailedToResolveGDExtensionEntryPoint,
                AssemblyLoadFailedState.FailedToLoad_GDExtensionInit =>
                    AssemblyLoadState.FailedToInitializeGDExtension,

                // Fallback to the generic failure state.
                // Note that errors related to hostfxr or the plugin loader will never reach here
                // because those would prevent loading this assembly as well.
                _ => AssemblyLoadState.FailedToLoad,
            },
            _ => AssemblyLoadState.NotLoaded,
        };
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _module_change_project_assembly;

    public static void ModuleChangeProjectAssembly(string assemblyName)
    {
        Debug.Assert(_module_change_project_assembly is not null);
        using NativeGodotString assemblyNameNative = NativeGodotString.Create(assemblyName);
        _module_change_project_assembly(&assemblyNameNative);
    }

    private static delegate* unmanaged[Cdecl]<void> _status_indicator_notify_state_changed;

    public static void StatusIndicatorNotifyStateChanged()
    {
        Debug.Assert(_status_indicator_notify_state_changed is not null);
        _status_indicator_notify_state_changed();
    }

    private static delegate* unmanaged[Cdecl]<long, void> _status_indicator_update_severity;

    public static void StatusIndicatorUpdateSeverity(StatusIndicatorSeverity severity)
    {
        Debug.Assert(_status_indicator_update_severity is not null);
        _status_indicator_update_severity((long)severity);
    }

    private static delegate* unmanaged[Cdecl]<nint, void> _status_panel_set_content;

    public static void StatusPanelSetContent(Control? content)
    {
        Debug.Assert(_status_panel_set_content is not null);
        _status_panel_set_content(content?.NativePtr ?? 0);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _get_editor_assemblies_path;

    public static string GetEditorAssembliesPath()
    {
        Debug.Assert(_get_editor_assemblies_path is not null);
        using NativeGodotString dest = default;
        _get_editor_assemblies_path(&dest);
        return dest.ToString();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _get_project_assemblies_path;

    public static string GetProjectAssembliesPath()
    {
        Debug.Assert(_get_project_assemblies_path is not null);
        using NativeGodotString dest = default;
        _get_project_assemblies_path(&dest);
        return dest.ToString();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, void> _get_project_output_path;

    public static string GetProjectOutputPath(string projectPath)
    {
        Debug.Assert(_get_project_output_path is not null);
        using NativeGodotString projectPathNative = NativeGodotString.Create(projectPath);
        using NativeGodotString dest = default;
        _get_project_output_path(&projectPathNative, &dest);
        return dest.ToString();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _get_project_solution_path;

    public static string GetProjectSolutionPath()
    {
        Debug.Assert(_get_project_solution_path is not null);
        using NativeGodotString dest = default;
        _get_project_solution_path(&dest);
        return dest.ToString();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _get_project_csproj_path;

    public static string GetProjectCSProjPath()
    {
        Debug.Assert(_get_project_csproj_path is not null);
        using NativeGodotString dest = default;
        _get_project_csproj_path(&dest);
        return dest.ToString();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _get_project_assembly_name;

    public static string GetProjectAssemblyName()
    {
        Debug.Assert(_get_project_assembly_name is not null);
        using NativeGodotString dest = default;
        _get_project_assembly_name(&dest);
        return dest.ToString();
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, long, bool, void> _progress_add_task;

    public static void ProgressAddTask(string task, string label, int steps, bool canCancel = false)
    {
        Debug.Assert(_progress_add_task is not null);
        using NativeGodotString taskNative = NativeGodotString.Create(task);
        using NativeGodotString labelNative = NativeGodotString.Create(label);
        _progress_add_task(&taskNative, &labelNative, steps, canCancel);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, long, bool, bool> _progress_task_step;

    public static bool ProgressTaskStep(string task, string state, int step = -1, bool forceRefresh = true)
    {
        Debug.Assert(_progress_task_step is not null);
        using NativeGodotString taskNative = NativeGodotString.Create(task);
        using NativeGodotString stateNative = NativeGodotString.Create(state);
        return _progress_task_step(&taskNative, &stateNative, step, forceRefresh);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, void> _progress_end_task;

    public static void ProgressEndTask(string task)
    {
        Debug.Assert(_progress_end_task is not null);
        using NativeGodotString taskNative = NativeGodotString.Create(task);
        _progress_end_task(&taskNative);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, void> _show_warning;

    public static void ShowWarning(string text)
    {
        ShowWarning(text, SR.DotNetEditorPlugin_AlertTitleWarning);
    }

    public static void ShowWarning(string text, string title)
    {
        Debug.Assert(_show_warning is not null);
        using NativeGodotString textNative = NativeGodotString.Create(text);
        using NativeGodotString titleNative = NativeGodotString.Create(title);
        _show_warning(&textNative, &titleNative);
    }

    private static delegate* unmanaged[Cdecl]<nint, void> _add_control_to_editor_run_bar;

    public static void AddControlToEditorRunBar(Control control)
    {
        Debug.Assert(_add_control_to_editor_run_bar is not null);
        _add_control_to_editor_run_bar(control.NativePtr);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, bool> _is_macos_app_bundle_installed;

    [SupportedOSPlatform("macos")]
    public static bool IsMacOSAppBundleInstalled(string bundleId)
    {
        Debug.Assert(_is_macos_app_bundle_installed is not null);
        using NativeGodotString bundleIdNative = NativeGodotString.Create(bundleId);
        return _is_macos_app_bundle_installed(&bundleIdNative);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotPackedStringArray*, bool> _lipo_create_file;

    public static bool LipOCreateFile(string outputPath, PackedStringArray files)
    {
        Debug.Assert(_lipo_create_file is not null);
        using NativeGodotString outputPathNative = NativeGodotString.Create(outputPath);
        NativeGodotPackedStringArray filesNative = files.NativeValue.DangerousSelfRef;
        return _lipo_create_file(&outputPathNative, &filesNative);
    }

    private static delegate* unmanaged[Cdecl]<nint, void> _register_dotnet_source_code_plugin;

    public static void RegisterDotNetSourceCodePlugin(EditorExtensionSourceCodePlugin plugin)
    {
        Debug.Assert(_register_dotnet_source_code_plugin is not null);
        _register_dotnet_source_code_plugin(plugin.NativePtr);
    }

    private static delegate* unmanaged[Cdecl]<nint, void> _unregister_dotnet_source_code_plugin;

    public static void UnregisterDotNetSourceCodePlugin(EditorExtensionSourceCodePlugin plugin)
    {
        Debug.Assert(_unregister_dotnet_source_code_plugin is not null);
        _unregister_dotnet_source_code_plugin(plugin.NativePtr);
    }

    // TODO: The methods below should be moved to the bindings.

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotVariant*, bool, NativeGodotVariant*, void> _editor_def;

    public static Variant EditorDefineSetting(string setting, Variant defaultValue, bool restartIfChanged = false)
    {
        Debug.Assert(_editor_def is not null);
        using NativeGodotString settingNative = NativeGodotString.Create(setting);
        NativeGodotVariant defaultValueNative = defaultValue.NativeValue.DangerousSelfRef;
        NativeGodotVariant dest = default;
        _editor_def(&settingNative, &defaultValueNative, restartIfChanged, &dest);
        return Variant.CreateTakingOwnership(dest);
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, long, bool, nint*, void> _editor_def_shortcut;

    public static Shortcut EditorDefineShortcut(string path, string name, Key keycode = Key.None, bool physical = false)
    {
        Debug.Assert(_editor_def_shortcut is not null);
        using NativeGodotString pathNative = NativeGodotString.Create(path);
        using NativeGodotString nameNative = NativeGodotString.Create(name);
        nint dest = default;
        _editor_def_shortcut(&pathNative, &nameNative, (long)keycode, physical, &dest);
        return (Shortcut)GodotObjectMarshaller.ConvertFromUnmanaged(&dest)!;
    }

    private static delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, long, bool, void> _editor_shortcut_override;

    public static void EditorShortcutOverride(string path, string feature, Key keycode = Key.None, bool physical = false)
    {
        Debug.Assert(_editor_shortcut_override is not null);
        using NativeGodotString pathNative = NativeGodotString.Create(path);
        using NativeGodotString featureNative = NativeGodotString.Create(feature);
        _editor_shortcut_override(&pathNative, &featureNative, (long)keycode, physical);
    }
}
