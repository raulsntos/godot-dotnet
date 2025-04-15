using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Godot.NativeInterop;

using unsafe GetProcAddressFunction = delegate* unmanaged[Cdecl]<byte*, nint>;

namespace Godot.Bridge;

/// <summary>
/// Represents the bridge between the Godot engine and the .NET bindings.
/// </summary>
/// <example>
/// Use the <see cref="Initialize(nint, nint, nint, Action{GodotBridgeConfiguration})"/>
/// method to initialize the bridge from the extension entry-point.
/// <code>
/// [UnmanagedCallersOnly(EntryPoint = "my_library_init")]
/// private static bool MyLibraryInit(nint getProcAddress, nint library, nint initialization)
/// {
///     GodotBridge.Initialize(getProcAddress, library, initialization, config =>
///     {
///         config.SetMinimumLibraryInitializationLevel(InitializationLevel.Scene);
///         config.RegisterInitializer(InitializeMyLibrary),
///         config.RegisterTerminator(DeinitializeMyLibrary),
///     });
///
///     return true;
/// }
/// </code>
/// </example>
public static partial class GodotBridge
{
    private static Action<InitializationLevel>? _initCallback;
    private static Action<InitializationLevel>? _terminateCallback;

    private static unsafe GetProcAddressFunction _getProcAddress;
    private static unsafe void* _libraryPtr;

    private static GodotVersion? _godotVersion;

    private static GDExtensionInterface _gdextensionInterface;

    internal static unsafe void* LibraryPtr => _libraryPtr;
    internal static GodotVersion GodotVersion => _godotVersion ?? throw new InvalidOperationException(SR.InvalidOperation_GodotBridgeNotInitialized);
    internal static GDExtensionInterface GDExtensionInterface => _gdextensionInterface;

    private static bool _initialized;

    private static unsafe void Initialize(GetProcAddressFunction getProcAddress, void* library, GDExtensionInitialization* initialization, Action<GodotBridgeConfiguration> configure)
    {
        if (_initialized)
        {
            throw new InvalidOperationException(SR.InvalidOperation_GodotBridgeAlreadyInitialized);
        }

        _getProcAddress = getProcAddress;
        _libraryPtr = library;

        var configuration = new GodotBridgeConfiguration();
        configure(configuration);

        _initCallback = configuration.InitCallback;
        _terminateCallback = configuration.TerminateCallback;

        InitializeGDExtensionInterface();

        // Load the Godot version.
        GDExtensionGodotVersion2 godotVersion = default;
        _gdextensionInterface.get_godot_version2(&godotVersion);
        _godotVersion = GodotVersion.Create(godotVersion);

        *initialization = new GDExtensionInitialization()
        {
            initialize = &InitializeLevel_Native,
            deinitialize = &DeinitializeLevel_Native,
            minimum_initialization_level = (GDExtensionInitializationLevel)configuration.MinimumInitLevel,
        };

        _initialized = true;
    }

    /// <summary>
    /// Initialize the Godot bridge between the engine and the .NET bindings.
    /// The <paramref name="getProcAddress"/>, <paramref name="library"/>, and
    /// <paramref name="initialization"/> pointers are received from the entry-point
    /// and must just be passed thru to this function.
    /// The <paramref name="configure"/> callback must be provided to configure the
    /// bridge for the extension. Use it to register and unregister the extension types
    /// on initialization/termination and set the minimum initialization level required
    /// by the extension.
    /// </summary>
    /// <param name="getProcAddress">Function pointer for retrieving GDExtension API.</param>
    /// <param name="library">Pointer that identifies the library.</param>
    /// <param name="initialization">Initialization object to configure.</param>
    /// <param name="configure">Callback to configure the bridge.</param>
    public static unsafe void Initialize(nint getProcAddress, nint library, nint initialization, Action<GodotBridgeConfiguration> configure)
    {
        Initialize((GetProcAddressFunction)getProcAddress, (void*)library, (GDExtensionInitialization*)initialization, configure);
    }

    private static unsafe nint LoadProcAddress(ReadOnlySpan<byte> nameUtf8)
    {
        Debug.Assert(_getProcAddress is not null);

        fixed (byte* namePtr = nameUtf8)
        {
            nint ptr = _getProcAddress(namePtr);
            if (ptr == 0)
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_UnableToLoadGDExtensionFunction(Encoding.UTF8.GetString(nameUtf8)));
            }

            return ptr;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void InitializeLevel_Native(void* userData, GDExtensionInitializationLevel level)
    {
        _initCallback?.Invoke((InitializationLevel)level);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void DeinitializeLevel_Native(void* userData, GDExtensionInitializationLevel level)
    {
        _terminateCallback?.Invoke((InitializationLevel)level);

        // Only free everything once at the last level.
        if (level == 0)
        {
            GodotRegistry.RemoveAllEditorPlugins();
            GodotRegistry.UnregisterAllClasses();

            DisposablesTracker.DisposeAll();
        }
    }
}
