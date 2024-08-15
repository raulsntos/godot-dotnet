using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Godot.EditorIntegration.Utils;

[SupportedOSPlatform("windows")]
internal static partial class User32Dll
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllowSetForegroundWindow(int dwProcessId);
}
