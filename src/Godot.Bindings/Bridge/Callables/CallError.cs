using Godot.NativeInterop;

namespace Godot.Bridge;

/// <summary>
/// Represents the status that can be returned when calling a <see cref="CustomCallable"/>.
/// </summary>
public enum CallError : uint
{
    /// <summary>
    /// The call completed successfully.
    /// </summary>
    Ok = GDExtensionCallErrorType.GDEXTENSION_CALL_OK,

    /// <summary>
    /// The method that the callable intended to call was invalid
    /// (e.g.: the method was not found).
    /// </summary>
    InvalidMethod = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INVALID_METHOD,

    /// <summary>
    /// The callable received an invalid argument
    /// (e.g.: the argument couldn't be converted to the expected type).
    /// </summary>
    InvalidArgument = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INVALID_ARGUMENT,

    /// <summary>
    /// The callable received more arguments than expected.
    /// </summary>
    TooManyArguments = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_TOO_MANY_ARGUMENTS,

    /// <summary>
    /// The callable received less arguments than expected.
    /// </summary>
    TooFewArguments = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_TOO_FEW_ARGUMENTS,

    /// <summary>
    /// The callable instance or its owner is null.
    /// </summary>
    InstanceIsNull = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_INSTANCE_IS_NULL,

    /// <summary>
    /// The callable attempted to call a non-constant method in a read-only instance.
    /// Only constant methods can be called in read-only instances to prevent mutation.
    /// </summary>
    MethodNotConstant = GDExtensionCallErrorType.GDEXTENSION_CALL_ERROR_METHOD_NOT_CONST,
}
