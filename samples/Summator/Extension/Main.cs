using System.Runtime.InteropServices;
using Godot.Bridge;

namespace GDExtensionSummator;

public class Main
{
    public static void InitializeSummatorTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }

        ClassDB.RegisterClass<Summator>(Summator.BindMethods);
        ClassDB.RegisterClass<SummatorNode>(SummatorNode.BindMethods);
    }

    public static void DeinitializeSummatorTypes(InitializationLevel level)
    {
        if (level != InitializationLevel.Scene)
        {
            return;
        }
    }

    // Initialization

    [UnmanagedCallersOnly(EntryPoint = "summator_library_init")]
    public static bool SummatorLibraryInit(nint getProcAddress, nint library, nint initialization)
    {
        GodotBridge.Initialize(getProcAddress, library, initialization, config =>
        {
            config.SetMinimumLibraryInitializationLevel(InitializationLevel.Scene);
            config.RegisterInitializer(InitializeSummatorTypes);
            config.RegisterTerminator(DeinitializeSummatorTypes);
        });

        return true;
    }
}
