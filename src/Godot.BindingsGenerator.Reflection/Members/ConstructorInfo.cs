namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# type's constructor.
/// </summary>
public class ConstructorInfo : MethodBase
{
    /// <summary>
    /// Constructs a new <see cref="ConstructorInfo"/>.
    /// </summary>
    public ConstructorInfo() : base(".ctor") { }

    /// <summary>
    /// Constructor initializer that will be executed before this constructor.
    /// Invokes another constructor using the <c>base</c> or <c>this</c> keyword.
    /// </summary>
    public string? Initializer { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Constructor: {Name}({string.Join(", ", Parameters)})";
}
