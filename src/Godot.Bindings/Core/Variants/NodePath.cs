using System;
using System.Diagnostics.CodeAnalysis;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// A pre-parsed relative or absolute path in a scene tree,
/// for use with <see cref="Node.GetNode(NodePath)"/> and similar functions.
/// It can reference a node, a resource within a node, or a property
/// of a node or resource.
/// For instance, <c>"Path2D/PathFollow2D/Sprite2D:texture:size"</c>
/// would refer to the <c>size</c> property of the <c>texture</c>
/// resource on the node named <c>"Sprite2D"</c> which is a child of
/// the other named nodes in the path.
/// You will usually just pass a string to <see cref="Node.GetNode(NodePath)"/>
/// and it will be automatically converted, but you may occasionally
/// want to parse a path ahead of time with NodePath.
/// Exporting a NodePath variable will give you a node selection widget
/// in the properties panel of the editor, which can often be useful.
/// A NodePath is composed of a list of slash-separated node names
/// (like a filesystem path) and an optional colon-separated list of
/// "subnames" which can be resources or properties.
///
/// Note: In the editor, NodePath properties are automatically updated when moving,
/// renaming or deleting a node in the scene tree, but they are never updated at runtime.
/// </summary>
/// <example>
/// Some examples of NodePaths include the following:
/// <code>
/// // No leading slash means it is relative to the current node.
/// new NodePath("A"); // Immediate child A.
/// new NodePath("A/B"); // A's child B.
/// new NodePath("."); // The current node.
/// new NodePath(".."); // The parent node.
/// new NodePath("../C"); // A sibling node C.
/// // A leading slash means it is absolute from the SceneTree.
/// new NodePath("/root"); // Equivalent to GetTree().Root
/// new NodePath("/root/Main"); // If your main scene's root node were named "Main".
/// new NodePath("/root/MyAutoload"); // If you have an autoloaded node or scene.
/// </code>
/// </example>
public sealed class NodePath : IDisposable, IEquatable<NodePath?>
{
    internal readonly NativeGodotNodePath.Movable NativeValue;

    private readonly WeakReference<IDisposable>? _weakReferenceToSelf;

    internal static NodePath Empty { get; } = new NodePath("");

    private NodePath(NativeGodotNodePath nativeValueToOwn)
    {
        NativeValue = nativeValueToOwn.AsMovable();
        _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
    }

    /// <summary>
    /// Constructs a new <see cref="NodePath"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static NodePath CreateTakingOwnership(NativeGodotNodePath nativeValueToOwn)
    {
        return new NodePath(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new <see cref="NodePath"/> from the value borrowed from
    /// <paramref name="nativeValueToCopy"/>, copying the value.
    /// Since the new instance is a copy of the value, the caller is responsible
    /// of disposing the new instance to avoid memory leaks.
    /// </summary>
    internal static NodePath CreateCopying(NativeGodotNodePath nativeValueToCopy)
    {
        return new NodePath(NativeGodotNodePath.Create(nativeValueToCopy));
    }

    /// <summary>
    /// Constructs a <see cref="NodePath"/> from a string <paramref name="path"/>,
    /// e.g.: <c>"Path2D/PathFollow2D/Sprite2D:texture:size"</c>.
    /// A path is absolute if it starts with a slash. Absolute paths
    /// are only valid in the global scene tree, not within individual
    /// scenes. In a relative path, <c>"."</c> and <c>".."</c> indicate
    /// the current node and its parent.
    /// The "subnames" optionally included after the path to the target
    /// node can point to resources or properties, and can also be nested.
    /// </summary>
    /// <example>
    /// Examples of valid NodePaths (assuming that those nodes exist and
    /// have the referenced resources or properties):
    /// <code>
    /// // Points to the Sprite2D node.
    /// "Path2D/PathFollow2D/Sprite2D"
    /// // Points to the Sprite2D node and its "texture" resource.
    /// // GetNode() would retrieve "Sprite2D", while GetNodeAndResource()
    /// // would retrieve both the Sprite2D node and the "texture" resource.
    /// "Path2D/PathFollow2D/Sprite2D:texture"
    /// // Points to the Sprite2D node and its "position" property.
    /// "Path2D/PathFollow2D/Sprite2D:position"
    /// // Points to the Sprite2D node and the "x" component of its "position" property.
    /// "Path2D/PathFollow2D/Sprite2D:position:x"
    /// // Absolute path (from "root")
    /// "/root/Level/Path2D"
    /// </code>
    /// </example>
    /// <param name="path">A string that represents a path in a scene tree.</param>
    public NodePath(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            using NativeGodotString src = NativeGodotString.Create(path);
            NativeValue = NativeGodotNodePath.Create(src).AsMovable();
            _weakReferenceToSelf = DisposablesTracker.RegisterDisposable(this);
        }
    }

    /// <summary>
    /// Converts a string to a <see cref="NodePath"/>.
    /// </summary>
    /// <param name="from">The string to convert.</param>
    [return: NotNullIfNotNull(nameof(from))]
    public static implicit operator NodePath?(string? from) => from is not null ? new NodePath(from) : null;

    /// <summary>
    /// Converts this <see cref="NodePath"/> to a string.
    /// </summary>
    /// <param name="from">The <see cref="NodePath"/> to convert.</param>
    [return: NotNullIfNotNull(nameof(from))]
    public static implicit operator string?(NodePath? from) => from?.ToString();

    /// <summary>
    /// Releases the unmanaged <see cref="StringName"/> instance.
    /// </summary>
    ~NodePath()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="NodePath"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // Always dispose `NativeValue` even if disposing is true.
        NativeValue.DangerousSelfRef.Dispose();

        if (_weakReferenceToSelf is not null)
        {
            DisposablesTracker.UnregisterDisposable(_weakReferenceToSelf);
        }
    }

    /// <summary>
    /// Returns a node path with a colon character (<c>:</c>) prepended,
    /// transforming it to a pure property path with no node name (defaults
    /// to resolving from the current node).
    /// </summary>
    /// <example>
    /// <code>
    /// // This will be parsed as a node path to the "x" property in the "position" node.
    /// var nodePath = new NodePath("position:x");
    /// // This will be parsed as a node path to the "x" component of the "position" property in the current node.
    /// NodePath propertyPath = nodePath.GetAsPropertyPath();
    /// GD.Print(propertyPath); // :position:x
    /// </code>
    /// </example>
    /// <returns>The <see cref="NodePath"/> as a pure property path.</returns>
    public NodePath GetAsPropertyPath()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        NativeGodotNodePath propertyPath = NativeGodotNodePath.GetAsPropertyPath(in self);
        return CreateTakingOwnership(propertyPath);
    }

    /// <summary>
    /// Returns all names concatenated with a slash character (<c>/</c>).
    /// </summary>
    /// <example>
    /// <code>
    /// var nodepath = new NodePath("Path2D/PathFollow2D/Sprite2D:texture:load_path");
    /// GD.Print(nodepath.GetConcatenatedNames()); // Path2D/PathFollow2D/Sprite2D
    /// </code>
    /// </example>
    /// <returns>The names concatenated with <c>/</c>.</returns>
    public StringName GetConcatenatedNames()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        NativeGodotStringName names = NativeGodotNodePath.GetConcatenatedNames(in self);
        return StringName.CreateTakingOwnership(names);
    }

    /// <summary>
    /// Returns all subnames concatenated with a colon character (<c>:</c>)
    /// as separator, i.e. the right side of the first colon in a node path.
    /// </summary>
    /// <example>
    /// <code>
    /// var nodepath = new NodePath("Path2D/PathFollow2D/Sprite2D:texture:load_path");
    /// GD.Print(nodepath.GetConcatenatedSubnames()); // texture:load_path
    /// </code>
    /// </example>
    /// <returns>The subnames concatenated with <c>:</c>.</returns>
    public StringName GetConcatenatedSubNames()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        NativeGodotStringName subNames = NativeGodotNodePath.GetConcatenatedSubnames(in self);
        return StringName.CreateTakingOwnership(subNames);
    }

    /// <summary>
    /// Gets the node name indicated by <paramref name="idx"/> (0 to <see cref="GetNameCount"/>).
    /// </summary>
    /// <example>
    /// <code>
    /// var nodePath = new NodePath("Path2D/PathFollow2D/Sprite2D");
    /// GD.Print(nodePath.GetName(0)); // Path2D
    /// GD.Print(nodePath.GetName(1)); // PathFollow2D
    /// GD.Print(nodePath.GetName(2)); // Sprite
    /// </code>
    /// </example>
    /// <param name="idx">The name index.</param>
    /// <returns>The name at the given index <paramref name="idx"/>.</returns>
    public StringName GetName(int idx)
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        NativeGodotStringName name = NativeGodotNodePath.GetName(in self, idx);
        return StringName.CreateTakingOwnership(name);
    }

    /// <summary>
    /// Gets the number of node names which make up the path.
    /// Subnames (see <see cref="GetSubNameCount"/>) are not included.
    /// For example, <c>"Path2D/PathFollow2D/Sprite2D"</c> has 3 names.
    /// </summary>
    /// <returns>The number of node names which make up the path.</returns>
    public int GetNameCount()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        return (int)NativeGodotNodePath.GetNameCount(in self);
    }

    /// <summary>
    /// Gets the resource or property name indicated by <paramref name="idx"/> (0 to <see cref="GetSubNameCount"/>).
    /// </summary>
    /// <param name="idx">The subname index.</param>
    /// <returns>The subname at the given index <paramref name="idx"/>.</returns>
    public StringName GetSubName(int idx)
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        NativeGodotStringName subName = NativeGodotNodePath.GetSubname(in self, idx);
        return StringName.CreateTakingOwnership(subName);
    }

    /// <summary>
    /// Gets the number of resource or property names ("subnames") in the path.
    /// Each subname is listed after a colon character (<c>:</c>) in the node path.
    /// For example, <c>"Path2D/PathFollow2D/Sprite2D:texture:load_path"</c> has 2 subnames.
    /// </summary>
    /// <returns>The number of subnames in the path.</returns>
    public int GetSubNameCount()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        return (int)NativeGodotNodePath.GetSubnameCount(in self);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the node path is absolute (as opposed to relative),
    /// which means that it starts with a slash character (<c>/</c>). Absolute node paths can
    /// be used to access the root node (<c>"/root"</c>) or autoloads (e.g. <c>"/global"</c>
    /// if a "global" autoload was registered).
    /// </summary>
    /// <returns>If the <see cref="NodePath"/> is an absolute path.</returns>
    public bool IsAbsolute()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        return NativeGodotNodePath.IsAbsolute(in self);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the node path is empty.
    /// </summary>
    /// <returns>If the <see cref="NodePath"/> is empty.</returns>
    public bool IsEmpty => NativeValue.DangerousSelfRef.IsEmpty;

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="NodePath"/>
    /// instances are equal.
    /// </summary>
    /// <param name="left">The left <see cref="NodePath"/>.</param>
    /// <param name="right">The right <see cref="NodePath"/>.</param>
    /// <returns>
    /// Whether or not the <see cref="NodePath"/> instances are equal.
    /// </returns>
    public static bool operator ==(NodePath? left, NodePath? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="NodePath"/>
    /// instances are not equal.
    /// </summary>
    /// <param name="left">The left <see cref="NodePath"/>.</param>
    /// <param name="right">The right <see cref="NodePath"/>.</param>
    /// <returns>
    /// Whether or not the <see cref="NodePath"/> instances are not equal.
    /// </returns>
    public static bool operator !=(NodePath? left, NodePath? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="NodePath"/>
    /// instances are equal.
    /// </summary>
    /// <param name="other">The other <see cref="NodePath"/>.</param>
    /// <returns>
    /// Whether or not the <see cref="NodePath"/> instances are equal.
    /// </returns>
    public bool Equals([NotNullWhen(true)] NodePath? other)
    {
        if (other is null)
        {
            return false;
        }

        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        NativeGodotNodePath otherNative = other.NativeValue.DangerousSelfRef;
        return NativeGodotNodePath.OperatorEqual(self, otherNative);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="NodePath"/> is
    /// equal to the given object (<paramref name="obj"/>).
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>
    /// Whether or not the <see cref="NodePath"/> and the object are equal.
    /// </returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return ReferenceEquals(this, obj)
            || (obj is NodePath other && Equals(other));
    }

    /// <summary>
    /// Serves as the hash function for <see cref="NodePath"/>.
    /// </summary>
    /// <returns>A hash code for this <see cref="NodePath"/>.</returns>
    public override int GetHashCode()
    {
        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        return (int)NativeGodotNodePath.Hash(in self);
    }

    /// <summary>
    /// Converts this <see cref="NodePath"/> to a string.
    /// </summary>
    /// <returns>A string representation of this <see cref="NodePath"/>.</returns>
    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        ref NativeGodotNodePath self = ref NativeValue.DangerousSelfRef;
        using NativeGodotString str = NativeGodotString.Create(self);
        return str.ToString();
    }
}
