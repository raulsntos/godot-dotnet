#nullable enable

namespace TestProject;

internal static class ClassDBExtensions
{
    internal static void InitializeUserTypes(global::Godot.Bridge.InitializationLevel level)
    {
        if (level != global::Godot.Bridge.InitializationLevel.Scene)
        {
            return;
        }
        global::Godot.Bridge.ClassDB.RegisterRuntimeClass<global::NS.MyNode>(global::NS.MyNode.BindMethods);
        global::Godot.Bridge.ClassDB.RegisterClass<global::NS.MyToolNode>(global::NS.MyToolNode.BindMethods);
    }
    internal static void DeinitializeUserTypes(global::Godot.Bridge.InitializationLevel level)
    {
        if (level != global::Godot.Bridge.InitializationLevel.Scene)
        {
            return;
        }
    }
}
