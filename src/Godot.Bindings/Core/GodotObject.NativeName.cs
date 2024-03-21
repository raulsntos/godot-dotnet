using System;
using System.Reflection;

namespace Godot;

partial class GodotObject
{
    /// <summary>
    /// Get the native Godot type name for <paramref name="type"/>.
    /// If <paramref name="type"/> is not a native Godot type, iterates the
    /// inheritance chain until a native Godot type is found and gets its
    /// native name.
    /// If there's no native Godot type in the inheritance chain, returns
    /// <see langword="null"/>.
    /// </summary>
    /// <param name="type">Type to get the native Godot name for.</param>
    /// <returns>The native Godot type name or <see langword="null"/>.</returns>
    internal static StringName? GetGodotNativeName(Type type)
    {
        Type? baseType = GetGodotNativeType(type);
        if (baseType is null)
        {
            return null;
        }

        StringName? nativeName;
        {
            // Try to retrieve the cached StringName that contains the native name of the type.
            // This avoids re-creating a StringName every time, but it may not be available
            // when trimming is enabled.
            nativeName = (StringName?)baseType.GetField(nameof(NativeName), BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
        }
        if (nativeName is null)
        {
            // The name of the type may not be the right name because the bindings
            // rename some types to follow .NET conventions more closely.
            // So we must get the right name from the [GodotNativeClassName] attribute.
            string typeName = baseType.GetCustomAttribute<GodotNativeClassNameAttribute>()?.Name ?? baseType.Name;
            nativeName = new StringName(typeName);
        }

        return nativeName;
    }

    /// <summary>
    /// Get the native Godot type that <paramref name="type"/> derives from,
    /// or <see langword="null"/> if the type does not derive from a native
    /// Godot type.
    /// </summary>
    /// <param name="type">Type to get the native Godot type for.</param>
    /// <returns>The native Godot base type.</returns>
    private static Type? GetGodotNativeType(Type type)
    {
        Type? t = type;

        while (t is not null)
        {
            if (t.Assembly == typeof(GodotObject).Assembly)
            {
                return t;
            }

            t = t.BaseType;
        }

        return null;
    }
}
