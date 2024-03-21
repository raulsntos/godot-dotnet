namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# member with visibility attributes.
/// </summary>
public abstract class VisibleMemberInfo : MemberInfo
{
    /// <summary>
    /// Member visibility attributes.
    /// </summary>
    public VisibilityAttributes VisibilityAttributes { get; set; }

    /// <summary>
    /// Indicates whether the member is public.
    /// </summary>
    /// <seealso cref="IsPrivate"/>
    /// <seealso cref="IsAssembly"/>
    /// <seealso cref="IsFamily"/>
    /// <seealso cref="IsFamilyOrAssembly"/>
    /// <seealso cref="IsFamilyAndAssembly"/>
    public bool IsPublic => VisibilityAttributes is VisibilityAttributes.Public;

    /// <summary>
    /// Indicates whether the member is private.
    /// </summary>
    /// <seealso cref="IsPublic"/>
    /// <seealso cref="IsAssembly"/>
    /// <seealso cref="IsFamily"/>
    /// <seealso cref="IsFamilyOrAssembly"/>
    /// <seealso cref="IsFamilyAndAssembly"/>
    public bool IsPrivate => VisibilityAttributes is VisibilityAttributes.Private;

    /// <summary>
    /// Indicates whether the member is internal.
    /// </summary>
    /// <seealso cref="IsPublic"/>
    /// <seealso cref="IsPrivate"/>
    /// <seealso cref="IsFamily"/>
    /// <seealso cref="IsFamilyOrAssembly"/>
    /// <seealso cref="IsFamilyAndAssembly"/>
    public bool IsAssembly => VisibilityAttributes is VisibilityAttributes.Assembly;

    /// <summary>
    /// Indicates whether the member is protected.
    /// </summary>
    /// <seealso cref="IsPublic"/>
    /// <seealso cref="IsPrivate"/>
    /// <seealso cref="IsAssembly"/>
    /// <seealso cref="IsFamilyOrAssembly"/>
    /// <seealso cref="IsFamilyAndAssembly"/>
    public bool IsFamily => VisibilityAttributes is VisibilityAttributes.Family;

    /// <summary>
    /// Indicates whether the member is protected internal (also known as protected public).
    /// </summary>
    /// <seealso cref="IsPublic"/>
    /// <seealso cref="IsPrivate"/>
    /// <seealso cref="IsAssembly"/>
    /// <seealso cref="IsFamily"/>
    /// <seealso cref="IsFamilyAndAssembly"/>
    public bool IsFamilyOrAssembly => VisibilityAttributes is VisibilityAttributes.FamilyOrAssembly;

    /// <summary>
    /// Indicates whether the member is private protected.
    /// </summary>
    /// <seealso cref="IsPublic"/>
    /// <seealso cref="IsPrivate"/>
    /// <seealso cref="IsAssembly"/>
    /// <seealso cref="IsFamily"/>
    /// <seealso cref="IsFamilyOrAssembly"/>
    public bool IsFamilyAndAssembly => VisibilityAttributes is VisibilityAttributes.FamilyAndAssembly;

    /// <summary>
    /// Constructs a new <see cref="VisibleMemberInfo"/>.
    /// </summary>
    /// <param name="name">Name of the member.</param>
    public VisibleMemberInfo(string name) : base(name) { }
}
