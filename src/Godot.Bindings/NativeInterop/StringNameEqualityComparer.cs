using System.Collections.Generic;

namespace Godot.NativeInterop;

internal sealed class StringNameEqualityComparer : IEqualityComparer<StringName>, IAlternateEqualityComparer<NativeGodotStringName, StringName>
{
    public static StringNameEqualityComparer Default { get; } = new();

    public StringName Create(NativeGodotStringName alternate)
    {
        return StringName.CreateCopying(alternate);
    }

    public bool Equals(StringName? x, StringName? y)
    {
        return x == y;
    }

    public bool Equals(NativeGodotStringName alternate, StringName other)
    {
        return alternate == other;
    }

    public int GetHashCode(StringName obj)
    {
        return obj.GetHashCode();
    }

    public int GetHashCode(NativeGodotStringName alternate)
    {
        return alternate.GetHashCode();
    }
}
