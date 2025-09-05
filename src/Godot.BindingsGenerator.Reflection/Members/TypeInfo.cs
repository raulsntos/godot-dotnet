using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# type.
/// </summary>
public class TypeInfo : VisibleMemberInfo, IEquatable<TypeInfo>
{
    /// <summary>
    /// The type that contains this type, if this is a nested type.
    /// </summary>
    public TypeInfo? ContainingType { get; set; }

    /// <summary>
    /// The base type that this type inherits from.
    /// </summary>
    public TypeInfo? BaseType { get; set; }

    /// <summary>
    /// The type's namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// The type's fully qualified name, including its namespace.
    /// </summary>
    public string FullName => GetFullName(includeGlobalNamespace: false);

    /// <summary>
    /// The type's fully qualified name, including its namespace and the global namespace.
    /// </summary>
    public string FullNameWithGlobal => GetFullName(includeGlobalNamespace: true);

    private string GetFullName(bool includeGlobalNamespace)
    {
        if (this is TypeParameterInfo)
        {
            // This is a generic type parameter so it should not have a namespace
            // or include the global namespace.
            return Name;
        }

        // If the type has no namespace it may be a fake type.
        if (string.IsNullOrEmpty(Namespace))
        {
            if (Name.StartsWith("delegate*", StringComparison.Ordinal))
            {
                // This is a fake type, it represents a function pointer.
                // Just return the name as-is.
                return Name;
            }
        }

        if (IsPointerType)
        {
            return $"{PointedAtType.GetFullName(includeGlobalNamespace)}*";
        }

        if (TryGetKeyword(out string? keyword))
        {
            return keyword;
        }

        var sb = new StringBuilder(includeGlobalNamespace ? "global::" : null);

        if (ContainingType is not null)
        {
            sb.Append(ContainingType.GetFullName(includeGlobalNamespace: false));
            sb.Append('.');
        }
        else if (!string.IsNullOrEmpty(Namespace))
        {
            sb.Append(Namespace);
            sb.Append('.');
        }

        sb.Append(Name);

        if (IsGenericType)
        {
            sb.Append('<');
            if (IsGenericTypeDefinition)
            {
                sb.Append(',', GenericTypeArgumentCount - 1);
            }
            else
            {
                Debug.Assert(GenericTypeArgumentCount == GenericTypeArguments.Count);

                for (int i = 0; i < GenericTypeArguments.Count; i++)
                {
                    sb.Append(GenericTypeArguments[i].GetFullName(includeGlobalNamespace));
                    if (i < GenericTypeArguments.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }
            sb.Append('>');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Indicates whether the type is read-only.
    /// Currently this is only supported for struct types.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// The type attributes.
    /// </summary>
    public TypeAttributes TypeAttributes { get; set; }

    /// <summary>
    /// Indicates whether the type is a reference type (e.g.: class).
    /// </summary>
    public bool IsReferenceType => TypeAttributes is TypeAttributes.ReferenceType;

    /// <summary>
    /// Indicates whether the type is a value type (e.g.: struct).
    /// </summary>
    public bool IsValueType => TypeAttributes is TypeAttributes.ValueType or TypeAttributes.ByRefLikeType;

    /// <summary>
    /// Indicates whether the type is a byref-like structure (i.e.: ref struct).
    /// </summary>
    public bool IsByRefLike => TypeAttributes is TypeAttributes.ByRefLikeType;

    /// <summary>
    /// Indicates whether the type is an enum.
    /// </summary>
    public bool IsEnum => this is EnumInfo;

    /// <summary>
    /// Type contract attributes.
    /// </summary>
    public ContractAttributes ContractAttributes { get; set; }

    /// <summary>
    /// Indicates whether the type is sealed (cannot be overridden).
    /// </summary>
    /// <seealso cref="IsVirtual"/>
    /// <seealso cref="IsAbstract"/>
    public bool IsFinal => ContractAttributes is ContractAttributes.Final;

    /// <summary>
    /// Indicates whether the type is virtual (can be overridden).
    /// </summary>
    /// <seealso cref="IsFinal"/>
    /// <seealso cref="IsAbstract"/>
    public bool IsVirtual => ContractAttributes is ContractAttributes.Virtual;

    /// <summary>
    /// Indicates whether the type is abstract (must be overridden).
    /// </summary>
    /// <seealso cref="IsFinal"/>
    /// <seealso cref="IsVirtual"/>
    public bool IsAbstract => ContractAttributes is ContractAttributes.Abstract;

    /// <summary>
    /// Indicates whether the type is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Indicates whether the type is partially defined, this allows extending the generated type.
    /// This type contains the <c>partial</c> keyword.
    /// </summary>
    public bool IsPartial { get; set; }

    /// <summary>
    /// Indicates whether the type represents a generic type.
    /// If <see cref="IsGenericTypeDefinition"/> is <see langword="true"/> the type
    /// arguments are unbound, otherwise they are bound and can be accessed through
    /// <see cref="GenericTypeArguments"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(GenericTypeDefinition))]
    public bool IsGenericType => GenericTypeArgumentCount > 0;

    /// <summary>
    /// Indicates whether the type represents a generic type and the type arguments are unbound.
    /// </summary>
    public bool IsGenericTypeDefinition => IsGenericType && GenericTypeDefinition is null;

    /// <summary>
    /// If this type is generic, contains the <see cref="TypeInfo"/> that represents
    /// the generic type with the type arguments unbound.
    /// </summary>
    public TypeInfo? GenericTypeDefinition { get; private init; }

    /// <summary>
    /// Number of generic type arguments or <c>0</c> if the type is not generic.
    /// </summary>
    public int GenericTypeArgumentCount { get; set; }

    private readonly List<TypeInfo> _genericTypeArguments = [];

    /// <summary>
    /// Collection of types used as the type arguments of this type if this type is generic.
    /// If <see cref="IsGenericTypeDefinition"/> is <see langword="true"/> the type
    /// arguments are unbound and this collection is empty.
    /// </summary>
    public IReadOnlyList<TypeInfo> GenericTypeArguments => _genericTypeArguments.AsReadOnly();

    /// <summary>
    /// If this type is generic type definition, construct the generic type with
    /// the type arguments bound to the provided <paramref name="typeArguments"/>.
    /// </summary>
    /// <param name="typeArguments">Types to use as the generic type arguments.</param>
    /// <returns>The constructed generic type.</returns>
    public TypeInfo MakeGenericType(IEnumerable<TypeInfo> typeArguments)
    {
        if (!IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"Type '{FullName}' is not a generic type definition.");
        }

        if (IsPointerType)
        {
            throw new InvalidOperationException($"Type is a pointer type. Pointer types can't make generic types.");
        }

        int typeArgumentsCount = typeArguments.Count();
        if (GenericTypeArgumentCount != typeArgumentsCount)
        {
            throw new ArgumentException($"Generic type argument mismatch. Expected {GenericTypeArgumentCount} arguments, received {typeArgumentsCount}.", nameof(typeArguments));
        }

        var genericType = new TypeInfo(this)
        {
            GenericTypeDefinition = this,
        };
        genericType._genericTypeArguments.AddRange(typeArguments);
        return genericType;
    }

    /// <summary>
    /// If this type represents a pointer to a type.
    /// </summary>
    [MemberNotNullWhen(true, nameof(PointedAtType))]
    public bool IsPointerType => PointedAtType is not null;

    /// <summary>
    /// If this type is a pointer, contains the <see cref="TypeInfo"/> that it points to.
    /// </summary>
    public TypeInfo? PointedAtType { get; private init; }

    /// <summary>
    /// Constructs a type that represents a pointer to the current type instance.
    /// </summary>
    /// <returns>The constructed pointer type.</returns>
    public TypeInfo MakePointerType()
    {
        if (IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"Type is a generic type definition. Generic types can't make pointer types unless the type arguments are bounded.");
        }

        return new TypeInfo(this)
        {
            PointedAtType = this,
        };
    }

    /// <summary>
    /// Collection of interfaces implemented by this type.
    /// </summary>
    public List<TypeInfo> ImplementedInterfaces { get; set; } = [];

    /// <summary>
    /// Collection of nested types declared in this type.
    /// </summary>
    public List<TypeInfo> NestedTypes { get; set; } = [];

    /// <summary>
    /// Collection of events declared in this type.
    /// </summary>
    public List<EventInfo> DeclaredEvents { get; set; } = [];

    /// <summary>
    /// Collection of fields declared in this type.
    /// </summary>
    public List<FieldInfo> DeclaredFields { get; set; } = [];

    /// <summary>
    /// Collection of properties declared in this type.
    /// </summary>
    public List<PropertyInfo> DeclaredProperties { get; set; } = [];

    /// <summary>
    /// Collection of methods declared in this type.
    /// </summary>
    public List<MethodInfo> DeclaredMethods { get; set; } = [];

    /// <summary>
    /// Collection of constructors declared in this type.
    /// </summary>
    public List<ConstructorInfo> DeclaredConstructors { get; set; } = [];

    /// <summary>
    /// Constructs a new <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="name">Name of the type.</param>
    /// <param name="namespace">Namespace that contains the type.</param>
    public TypeInfo(string name, string? @namespace = null) : base(name)
    {
        Namespace = @namespace;
    }

    private TypeInfo(TypeInfo underlyingType) : this(underlyingType.Name, underlyingType.Namespace)
    {
        ContainingType = underlyingType.ContainingType;
        BaseType = underlyingType.BaseType;
        TypeAttributes = underlyingType.TypeAttributes;
        VisibilityAttributes = underlyingType.VisibilityAttributes;
        ContractAttributes = underlyingType.ContractAttributes;
        IsStatic = underlyingType.IsStatic;
        IsPartial = underlyingType.IsPartial;
        GenericTypeDefinition = underlyingType.GenericTypeDefinition;
        GenericTypeArgumentCount = underlyingType.GenericTypeArgumentCount;
        PointedAtType = underlyingType.PointedAtType;
        ImplementedInterfaces = underlyingType.ImplementedInterfaces;
        NestedTypes = underlyingType.NestedTypes;
        DeclaredEvents = underlyingType.DeclaredEvents;
        DeclaredFields = underlyingType.DeclaredFields;
        DeclaredProperties = underlyingType.DeclaredProperties;
        DeclaredMethods = underlyingType.DeclaredMethods;
        DeclaredConstructors = underlyingType.DeclaredConstructors;
    }

    /// <summary>
    /// Check if <paramref name="left"/> and <paramref name="right"/> represent the same type.
    /// </summary>
    /// <param name="left">The type to compare against <paramref name="right"/>.</param>
    /// <param name="right">The type to compare against <paramref name="left"/>.</param>
    /// <returns><see langword="true"/> if the types represent the same type.</returns>
    public static bool operator ==(TypeInfo? left, TypeInfo? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Check if <paramref name="left"/> and <paramref name="right"/> don't represent the same type.
    /// </summary>
    /// <param name="left">The type to compare against <paramref name="right"/></param>
    /// <param name="right">The type to compare against <paramref name="left"/></param>
    /// <returns><see langword="true"/> if the types do not represent the same type.</returns>
    public static bool operator !=(TypeInfo? left, TypeInfo? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is TypeInfo other && Equals(other);
    }

    /// <inheritdoc/>
    public bool Equals([NotNullWhen(true)] TypeInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        // If the types have the same fully-qualified name they should be the same type.
        return Name == other.Name && Namespace == other.Namespace
            && IsPointerType == other.IsPointerType
            && GenericTypeArgumentCount == other.GenericTypeArgumentCount
            && _genericTypeArguments.SequenceEqual(other._genericTypeArguments);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Namespace);
        hash.Add(IsPointerType);
        hash.Add(GenericTypeArgumentCount);
        foreach (var type in _genericTypeArguments)
        {
            hash.Add(type);
        }
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Type: {Name}";

    private bool TryGetKeyword([NotNullWhen(true)] out string? keyword)
    {
        if (Namespace != "System")
        {
            keyword = null;
            return false;
        }

        keyword = Name switch
        {
            "Void" => "void",
            "Byte" => "byte",
            "Boolean" => "bool",
            "SByte" => "sbyte",
            "Int16" => "short",
            "Int32" => "int",
            "Int64" => "long",
            "UInt16" => "ushort",
            "UInt32" => "uint",
            "UInt64" => "ulong",
            "Single" => "float",
            "Double" => "double",
            "String" => "string",
            "IntPtr" => "nint",
            "UIntPtr" => "nuint",
            "Object" => "object",
            _ => null,
        };

        if (Name == "Array")
        {
            if (IsGenericTypeDefinition)
            {
                keyword = "T[]";
            }
            else
            {
                keyword = $"{GenericTypeArguments[0].FullNameWithGlobal}[]";
            }
        }

        return keyword is not null;
    }
}
