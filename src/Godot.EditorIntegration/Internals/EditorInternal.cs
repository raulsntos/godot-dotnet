using System;
using System.Diagnostics;
using System.Text;
using Godot.NativeInterop;

using unsafe GetProcAddressFunction = delegate* unmanaged[Cdecl]<byte*, nint>;

namespace Godot.EditorIntegration.Internals;

/// <summary>
/// Contains internal Godot APIs exposed by the dotnet module to implement the editor integration.
/// </summary>
internal static partial class EditorInternal
{
    private static unsafe GetProcAddressFunction _getProcAddress;

    public static unsafe void Initialize(nint getProcAddress)
    {
        _getProcAddress = (GetProcAddressFunction)getProcAddress;

        _get_editor_assemblies_path = (delegate* unmanaged[Cdecl]<NativeGodotString*, void>)LoadProcAddress("get_editor_assemblies_path"u8);
        _get_project_assemblies_path = (delegate* unmanaged[Cdecl]<NativeGodotString*, void>)LoadProcAddress("get_project_assemblies_path"u8);
        _get_project_output_path = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, void>)LoadProcAddress("get_project_output_path"u8);
        _get_project_sln_path = (delegate* unmanaged[Cdecl]<NativeGodotString*, void>)LoadProcAddress("get_project_sln_path"u8);
        _get_project_csproj_path = (delegate* unmanaged[Cdecl]<NativeGodotString*, void>)LoadProcAddress("get_project_csproj_path"u8);
        _get_project_assembly_name = (delegate* unmanaged[Cdecl]<NativeGodotString*, void>)LoadProcAddress("get_project_assembly_name"u8);
        _progress_add_task = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, int, bool, void>)LoadProcAddress("progress_add_task"u8);
        _progress_task_step = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, int, bool, bool>)LoadProcAddress("progress_task_step"u8);
        _progress_end_task = (delegate* unmanaged[Cdecl]<NativeGodotString*, void>)LoadProcAddress("progress_end_task"u8);
        _show_warning = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, void>)LoadProcAddress("show_warning"u8);
        _add_control_to_editor_run_bar = (delegate* unmanaged[Cdecl]<void*, void>)LoadProcAddress("add_control_to_editor_run_bar"u8);
        _is_macos_app_bundle_installed = (delegate* unmanaged[Cdecl]<NativeGodotString*, bool>)LoadProcAddress("is_macos_app_bundle_installed"u8);
        _lipo_create_file = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotPackedStringArray*, bool>)LoadProcAddress("lipo_create_file"u8);

        // TODO: The methods below should be moved to the bindings.

        _editor_def = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotVariant*, bool, NativeGodotVariant*, void>)LoadProcAddress("editor_def"u8);
        _editor_def_shortcut = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, long, bool, nint*, void>)LoadProcAddress("editor_def_shortcut"u8);
        _editor_shortcut_override = (delegate* unmanaged[Cdecl]<NativeGodotString*, NativeGodotString*, long, bool, void>)LoadProcAddress("editor_shortcut_override"u8);
    }

    private static unsafe nint LoadProcAddress(ReadOnlySpan<byte> nameUtf8)
    {
        Debug.Assert(_getProcAddress is not null);

        fixed (byte* namePtr = nameUtf8)
        {
            nint ptr = _getProcAddress(namePtr);
            if (ptr == 0)
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_UnableToLoadGDExtensionInterfaceFunction(Encoding.UTF8.GetString(nameUtf8)));
            }

            return ptr;
        }
    }
}
