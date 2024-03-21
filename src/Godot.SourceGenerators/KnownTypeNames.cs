namespace Godot.SourceGenerators;

internal static class KnownTypeNames
{
    // System types.
    public const string SystemVoid = "System.Void";
    public const string SystemByte = "System.Byte";
    public const string SystemBoolean = "System.Boolean";
    public const string SystemSByte = "System.SByte";
    public const string SystemChar = "System.Char";
    public const string SystemInt16 = "System.Int16";
    public const string SystemInt32 = "System.Int32";
    public const string SystemInt64 = "System.Int64";
    public const string SystemUInt16 = "System.UInt16";
    public const string SystemUInt32 = "System.UInt32";
    public const string SystemUInt64 = "System.UInt64";
    public const string SystemHalf = "System.Half";
    public const string SystemSingle = "System.Single";
    public const string SystemDouble = "System.Double";
    public const string SystemDecimal = "System.Decimal";
    public const string SystemString = "System.String";
    public const string SystemIntPtr = "System.IntPtr";
    public const string SystemUIntPtr = "System.UIntPtr";

    // Godot types.
    public const string GodotAabb = "Godot.Aabb";
    public const string GodotBasis = "Godot.Basis";
    public const string GodotCallable = "Godot.Callable";
    public const string GodotColor = "Godot.Color";
    public const string GodotNodePath = "Godot.NodePath";
    public const string GodotPlane = "Godot.Plane";
    public const string GodotProjection = "Godot.Projection";
    public const string GodotQuaternion = "Godot.Quaternion";
    public const string GodotRect2 = "Godot.Rect2";
    public const string GodotRect2I = "Godot.Rect2I";
    public const string GodotRid = "Godot.Rid";
    public const string GodotSignal = "Godot.Signal";
    public const string GodotStringName = "Godot.StringName";
    public const string GodotTransform2D = "Godot.Transform2D";
    public const string GodotTransform3D = "Godot.Transform3D";
    public const string GodotVector2 = "Godot.Vector2";
    public const string GodotVector2I = "Godot.Vector2I";
    public const string GodotVector3 = "Godot.Vector3";
    public const string GodotVector3I = "Godot.Vector3I";
    public const string GodotVector4 = "Godot.Vector4";
    public const string GodotVector4I = "Godot.Vector4I";
    public const string GodotVariant = "Godot.Variant";
    public const string GodotObject = "Godot.GodotObject";
    public const string GodotNode = "Godot.Node";
    public const string GodotResource = "Godot.Resource";

    // Godot collections.
    public const string GodotPackedByteArray = "Godot.Collections.PackedByteArray";
    public const string GodotPackedInt32Array = "Godot.Collections.PackedInt32Array";
    public const string GodotPackedInt64Array = "Godot.Collections.PackedInt64Array";
    public const string GodotPackedFloat32Array = "Godot.Collections.PackedFloat32Array";
    public const string GodotPackedFloat64Array = "Godot.Collections.PackedFloat64Array";
    public const string GodotPackedStringArray = "Godot.Collections.PackedStringArray";
    public const string GodotPackedVector2Array = "Godot.Collections.PackedVector2Array";
    public const string GodotPackedVector3Array = "Godot.Collections.PackedVector3Array";
    public const string GodotPackedColorArray = "Godot.Collections.PackedColorArray";
    public const string GodotArray = "Godot.Collections.GodotArray";
    public const string GodotDictionary = "Godot.Collections.GodotDictionary";

    // Godot attributes.
    public const string GodotNativeClassNameAttribute = "Godot.GodotNativeClassNameAttribute";
    public const string GodotClassAttribute = "Godot.GodotClassAttribute";
    public const string BindConstructorAttribute = "Godot.BindConstructorAttribute";
    public const string BindMethodAttribute = "Godot.BindMethodAttribute";
    public const string BindPropertyAttribute = "Godot.BindPropertyAttribute";
    public const string SignalAttribute = "Godot.SignalAttribute";
}
