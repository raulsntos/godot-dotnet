namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Attributes that define the contract of a member.
/// If they can or must be overriden.
/// </summary>
public enum ContractAttributes
{
    /// <summary>
    /// No contract defined.
    /// This is an invalid value, every member must define a contract.
    /// </summary>
    None,

    /// <summary>
    /// The member is sealed and cannot be overridden.
    /// </summary>
    Final,

    /// <summary>
    /// The member is virtual and can be overridden.
    /// </summary>
    Virtual,

    /// <summary>
    /// The member is abstract and must be overridden.
    /// </summary>
    Abstract,
}
