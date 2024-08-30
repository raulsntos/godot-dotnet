using System;
using System.Collections.Generic;

namespace Godot.Bridge;

partial class ClassDBRegistrationContext
{
    internal readonly Dictionary<StringName, VirtualMethodOverrideInfo> RegisteredVirtualMethodOverrides = [];

    /// <summary>
    /// Register the override of a built-in virtual method in the class.
    /// This is used to bind the specified callback as the override of a virtual method
    /// in a built-in class. To register virtual methods provided by the extension class
    /// use <see cref="BindVirtualMethod(StringName)"/>.
    /// </summary>
    /// <param name="virtualMethodInfo">Information that describes the method to register.</param>
    /// <exception cref="ArgumentException">
    /// A method has already been registered with the same name.
    /// </exception>
    internal void BindVirtualMethodOverride(VirtualMethodOverrideInfo virtualMethodInfo)
    {
        if (!_registeredMethods.Add(virtualMethodInfo.Name))
        {
            throw new ArgumentException(SR.FormatArgument_VirtualMethodOverrideAlreadyRegistered(virtualMethodInfo.Name, ClassName), nameof(virtualMethodInfo));
        }

        RegisteredVirtualMethodOverrides[virtualMethodInfo.Name] = virtualMethodInfo;
    }
}
