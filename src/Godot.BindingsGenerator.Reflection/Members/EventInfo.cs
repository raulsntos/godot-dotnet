namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines a C# event member.
/// </summary>
public class EventInfo : VisibleMemberInfo
{
    /// <summary>
    /// Indicates whether the event is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// The type of the underlying event-handler delegate associated
    /// with this event.
    /// </summary>
    public TypeInfo EventHandlerType { get; set; }

    /// <summary>
    /// Event's add accessor.
    /// </summary>
    public MethodInfo? AddAccessor { get; set; }

    /// <summary>
    /// Event's remove accessor.
    /// </summary>
    public MethodInfo? RemoveAccessor { get; set; }

    /// <summary>
    /// Constructs a new <see cref="EventInfo"/>.
    /// </summary>
    /// <param name="name">Name of the event.</param>
    /// <param name="eventHandlerType">Delegate type associated with the event.</param>
    public EventInfo(string name, TypeInfo eventHandlerType) : base(name)
    {
        EventHandlerType = eventHandlerType;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"Event: {EventHandlerType.FullName} {Name}";
}
