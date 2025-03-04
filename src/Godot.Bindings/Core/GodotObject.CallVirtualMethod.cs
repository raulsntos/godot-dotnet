using System;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot;

partial class GodotObject
{
    /// <summary>
    /// Try to call the script implementation of a method that was bound as virtual in ClassDB.
    /// If this instance doesn't have a script attached, or the attached script doesn't override
    /// the method it returns <see langword="false"/>.
    /// If the virtual method is required to be implemented by scripts, use
    /// <see cref="CallVirtualMethodCore"/> to invoke it instead.
    /// <example>
    /// <code>
    /// public int GetEnemyBaseHealth(string enemyName)
    /// {
    ///     // Try to call the virtual method if it was implemented by an user script.
    ///     if (TryCallVirtualMethod(MethodName.GetEnemyBaseHealth, enemyName, out int baseHealth))
    ///     {
    ///         return baseHealth;
    ///     }
    ///
    ///     // The user script doesn't override the method, return the default base health.
    ///     return 100;
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="name">Name of the virtual method.</param>
    /// <param name="args">Arguments to use in the method invocation.</param>
    /// <param name="returnValue">The method's return value.</param>
    /// <returns>Whether the method was invoked successfully.</returns>
    internal unsafe bool TryCallVirtualMethodCore(StringName name, NativeGodotVariantPtrSpan args, out NativeGodotVariant returnValue)
    {
        NativeGodotStringName nameNative = name.NativeValue.DangerousSelfRef;
        returnValue = default;

        if (GodotBridge.GDExtensionInterface.object_has_script_method((void*)NativePtr, &nameNative))
        {
            GDExtensionCallError error;
            fixed (NativeGodotVariant** argsPtr = args)
            {
                GodotBridge.GDExtensionInterface.object_call_script_method((void*)NativePtr, &nameNative, argsPtr, args.Length, returnValue.GetUnsafeAddress(), &error);
            }
            if (error.error == GDExtensionCallErrorType.GDEXTENSION_CALL_OK)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Call the script implementation of a method that was bound as virtual in ClassDB.
    /// If this instance has an script attached and doesn't override the method, it
    /// throws an exception.
    /// If the virtual method is not required to be implemented by scripts, use
    /// <see cref="TryCallVirtualMethodCore"/> to invoke it instead.
    /// <example>
    /// <code>
    /// public int GetEnemyBaseHealth(string enemyName)
    /// {
    ///     // Call the required virtual method implemented by an user script.
    ///     return CallVirtualMethod&lt;string, int&gt;(MethodName.GetEnemyBaseHealth, enemyName));
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="name">Name of the virtual method.</param>
    /// <param name="args">Arguments to use in the method invocation.</param>
    /// <returns>The method's return value.</returns>
    /// <exception cref="InvalidOperationException">
    /// There is no script attached.
    /// -or-
    /// The attached script doesn't implement the required virtual method.
    /// </exception>
    internal unsafe NativeGodotVariant CallVirtualMethodCore(StringName name, NativeGodotVariantPtrSpan args)
    {
        if (!TryCallVirtualMethodCore(name, args, out var returnValue))
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_RequiredVirtualMethodMustBeOverridden(GetType(), name));
        }

        return returnValue;
    }
}
