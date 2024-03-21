using System.Collections.Generic;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal class CallMethodBodyContext
{
    /// <summary>
    /// Indicates whether the method is static.
    /// For example, instance method binds require an instance parameter.
    /// </summary>
    public required bool IsStatic { get; init; }

    /// <summary>
    /// Indicates whether the method requires cleanup.
    /// For example, marshalling parameters usually needs cleanup later
    /// to release the memory that was allocated when they were marshalled.
    /// </summary>
    public bool NeedsCleanup { get; set; }

    /// <summary>
    /// List of parameters to pass to the method bind.
    /// </summary>
    public required IList<ParameterInfo> Parameters { get; init; }

    /// <summary>
    /// The return type of the method, or <see langword="null"/>
    /// if the method returns void.
    /// </summary>
    public required TypeInfo? ReturnType { get; init; }

    public string InstanceVariableName { get; set; } = "__instance";
    public string ArgsVariableName { get; set; } = "__args";
    public string ReturnVariableName { get; set; } = "__ret";
}
