namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Attributes that define the visibility of a member.
/// If it's public, private, protected or internal.
/// </summary>
public enum VisibilityAttributes
{
    /// <summary>
    /// No visibility defined.
    /// </summary>
    None,

    /// <summary>
    /// The member is private and can only be accessed by the type that declares it.
    /// </summary>
    Private,

    /// <summary>
    /// The member is internal and can only be accessed by types defined in the same
    /// assembly that contains the type that declares it.
    /// </summary>
    Assembly,

    /// <summary>
    /// The member is protected and can only be accessed by the type that declares it
    /// or types that derive from it.
    /// </summary>
    Family,

    /// <summary>
    /// The member is public and can be accessed by any type.
    /// </summary>
    Public,

    /// <summary>
    /// The member is protected internal (also known as protected public) and can only
    /// be accessed by the type that declares it, types that derive from it or any type
    /// contained in the same assembly that contains the type that declares it.
    /// </summary>
    FamilyOrAssembly,

    /// <summary>
    /// The member is private protected and can only be accessed by the type that declares it
    /// or types that derive from it as long as these types are contained in the same assembly
    /// that contains the type that declares it.
    /// </summary>
    FamilyAndAssembly,
}
