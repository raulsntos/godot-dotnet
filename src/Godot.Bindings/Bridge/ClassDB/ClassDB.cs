using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;

namespace Godot.Bridge;

/// <summary>
/// Database that registers classes and their members within the Godot engine.
/// </summary>
public static class ClassDB
{
    private static readonly Dictionary<StringName, ClassDBRegistrationContext> _registeredClasses = [];
    private static readonly Stack<StringName> _classRegisterStack = [];

    /// <summary>
    /// Registers a class with a configuration function that registers its members.
    /// Classes registered with this method will also run in the editor, to avoid this
    /// use <see cref="RegisterRuntimeClass{T}(Action{ClassDBRegistrationContext})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterClass<T>(Action<ClassDBRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: false, isExposed: true, isRuntime: false, configure);
    }

    /// <summary>
    /// Registers a runtime class with a configuration function that registers its members.
    /// Runtime classes don't run in the editor, to register classes that also run in the editor
    /// use <see cref="RegisterClass{T}(Action{ClassDBRegistrationContext})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterRuntimeClass<T>(Action<ClassDBRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: false, isExposed: true, isRuntime: true, configure);
    }

    /// <summary>
    /// Registers a virtual class with a configuration function that registers its members.
    /// Virtual classes can't be instantiated in user scripts but can be derived and used
    /// as an export type.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterVirtualClass<T>(Action<ClassDBRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: true, isAbstract: false, isExposed: true, isRuntime: false, configure);
    }

    /// <summary>
    /// Registers an abstract class with a configuration function that registers its members.
    /// Abstract classes can't be instantiated or derived in user scripts, but can still be
    /// used as an export type.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterAbstractClass<T>(Action<ClassDBRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: true, isExposed: true, isRuntime: false, configure);
    }

    /// <summary>
    /// Registers an internal class with a configuration function that registers its members.
    /// Internal classes are hidden in the editor so users won't see them.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterInternalClass<T>(Action<ClassDBRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: false, isExposed: false, isRuntime: false, configure);
    }

    private unsafe static void RegisterClassCore<T>(bool isVirtual, bool isAbstract, bool isExposed, bool isRuntime, Action<ClassDBRegistrationContext> configure) where T : GodotObject
    {
        if (typeof(T).IsAbstract && !isAbstract)
        {
            // T is an abstract type but the isAbstract parameter is false.
            throw new ArgumentException($"Can't register abstract type '{typeof(T)}' as a non-abstract class.", nameof(isAbstract));
        }

        StringName className = new StringName(typeof(T).Name);
        if (_registeredClasses.TryGetValue(className, out var context))
        {
            // If this class has been registered before, allow configuring it again.
            configure(context);
            return;
        }

        context = new ClassDBRegistrationContext(className);
        _registeredClasses[className] = context;
        _classRegisterStack.Push(className);

        var creationInfo = new GDExtensionClassCreationInfo3()
        {
            is_virtual = isVirtual,
            is_abstract = isAbstract,
            is_exposed = isExposed,
            is_runtime = isRuntime,
            set_func = &Set_Native,
            get_func = &Get_Native,
            get_property_list_func = &GetPropertyList_Native,
            free_property_list_func = &FreePropertyList_Native,
            property_can_revert_func = &PropertyCanRevert_Native,
            property_get_revert_func = &PropertyGetRevert_Native,
            validate_property_func = &ValidateProperty_Native,
            notification_func = &Notification_Native,
            to_string_func = &ToString_Native,
            reference_func = null,
            unreference_func = null,
            create_instance_func = &Create_Native,
            free_instance_func = &Free_Native,
            // recreate_instance_func = null, // TODO: We should implement this for GDExtension reloading.
            get_virtual_func = &GetVirtualFunctionPointer_Native,
            get_virtual_call_data_func = null,
            call_virtual_with_data_func = null,
            get_rid_func = null,
            class_userdata = (void*)GCHandle.ToIntPtr(context.GCHandle),
        };

        StringName? godotNativeName = GodotObject.GetGodotNativeName(typeof(T));

        // The 'BaseType' will never be null becase T has a constraint that
        // it must derive from GodotObject, but we assert this anyway so the
        // null analysis doesn't complain about it being null.
        Debug.Assert(godotNativeName is not null, $"Type '{typeof(T)}' must derive from a Godot type.");

        StringName baseClassName;
        if (typeof(T).BaseType?.Assembly != typeof(GodotObject).Assembly)
        {
            // If the base type is not a built-in Godot type,
            // construct the name from the type name.
            baseClassName = new StringName(typeof(T).BaseType!.Name);
        }
        else
        {
            // Otherwise, use the retrieved Godot native name
            // which may be different from the type name.
            baseClassName = godotNativeName;
        }

        NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;
        NativeGodotStringName baseClassNameNative = baseClassName.NativeValue.DangerousSelfRef;

        GodotBridge.GDExtensionInterface.classdb_register_extension_class3(GodotBridge.LibraryPtr, &classNameNative, &baseClassNameNative, &creationInfo);

        configure(context);

        if (InteropUtils.RegisterVirtualOverridesHelpers.TryGetValue(godotNativeName, out var registerVirtualOverrides))
        {
            registerVirtualOverrides(context);
        }
    }

    internal unsafe static void UnregisterAllClasses()
    {
        while (_classRegisterStack.TryPop(out StringName? className))
        {
            NativeGodotStringName classNameNative = className.NativeValue.DangerousSelfRef;

            GodotBridge.GDExtensionInterface.classdb_unregister_extension_class(GodotBridge.LibraryPtr, &classNameNative);

            _registeredClasses.Remove(className);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static bool Set_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* value)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            StringName nameManaged = StringName.CreateTakingOwnership(*name);
            Variant valueManaged = Variant.CreateTakingOwnership(*value);

            return instanceObj._Set(nameManaged, valueManaged);
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static bool Get_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* outRet)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            StringName nameManaged = StringName.CreateTakingOwnership(*name);

            bool ok = instanceObj._Get(nameManaged, out Variant valueManaged);

            *outRet = valueManaged.NativeValue.DangerousSelfRef;
            return ok;
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static GDExtensionPropertyInfo* GetPropertyList_Native(void* instance, uint* outCount)
    {
        if (instance is null)
        {
            if (outCount is not null)
            {
                *outCount = 0;
            }
            return null;
        }

        var gcHandle = GCHandle.FromIntPtr((nint)instance);
        var instanceObj = (GodotObject?)gcHandle.Target;

        Debug.Assert(instanceObj is not null);

        var propertyList = instanceObj.GetPropertyListStorage();
        Debug.Assert(propertyList.Count == 0, "Internal error, property list was not freed by engine!");

        instanceObj._GetPropertyList(propertyList);

        GDExtensionPropertyInfo* propertyListPtr = PropertyInfoList.ConvertToNative(propertyList);

        if (outCount is not null)
        {
            *outCount = (uint)propertyList.Count;
        }
        return propertyListPtr;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static void FreePropertyList_Native(void* instance, GDExtensionPropertyInfo* propertyListPtr, uint count)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            var propertyList = instanceObj.GetPropertyListStorage();
            propertyList.Clear();

            PropertyInfoList.FreeNative(propertyListPtr);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static bool PropertyCanRevert_Native(void* instance, NativeGodotStringName* name)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            StringName nameManaged = StringName.CreateTakingOwnership(*name);

            return instanceObj._PropertyCanRevert(nameManaged);
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static bool PropertyGetRevert_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* outRet)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            StringName nameManaged = StringName.CreateTakingOwnership(*name);

            bool ok = instanceObj._PropertyGetRevert(nameManaged, out Variant valueManaged);

            *outRet = valueManaged.NativeValue.DangerousSelfRef;
            return ok;
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static bool ValidateProperty_Native(void* instance, GDExtensionPropertyInfo* refProperty)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            // Convert internal property info to the public managed type.
            VariantType type = (VariantType)refProperty->type;
            StringName? name = StringNameMarshaller.ConvertFromUnmanaged(refProperty->name);
            Debug.Assert(name is not null);
            var propertyInfo = new PropertyInfo(name, type)
            {
                Hint = (PropertyHint)refProperty->hint,
                HintString = StringMarshaller.ConvertFromUnmanaged(refProperty->hint_string),
                ClassName = StringNameMarshaller.ConvertFromUnmanaged(refProperty->class_name),
                Usage = (PropertyUsageFlags)refProperty->usage,
            };

            instanceObj._ValidateProperty(propertyInfo);

            // Update the property info with the data from the managed type.
            refProperty->type = (GDExtensionVariantType)propertyInfo.Type;
            StringNameMarshaller.WriteUnmanaged(refProperty->name, propertyInfo.Name);
            refProperty->hint = (uint)propertyInfo.Hint;
            StringMarshaller.WriteUnmanaged(refProperty->hint_string, propertyInfo.HintString);
            StringNameMarshaller.WriteUnmanaged(refProperty->class_name, propertyInfo.ClassName);
            refProperty->usage = (uint)propertyInfo.Usage;

            return true;
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static void Notification_Native(void* instance, int what, bool reversed)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            instanceObj._Notification(what);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static void ToString_Native(void* instance, bool* outIsValid, NativeGodotString* outStr)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)instance);

        var instanceObj = gcHandle.Target;
        if (instanceObj is null)
        {
            *outIsValid = false;
            return;
        }

        *outStr = NativeGodotString.Create(instanceObj.ToString());
        *outIsValid = true;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static void* Create_Native(void* userData)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var context = (ClassDBRegistrationContext?)gcHandle.Target;

        Debug.Assert(context is not null);

        if (context.RegisteredConstructor is null)
        {
            throw new InvalidOperationException($"Can't instantiate type '{context.ClassName}'. A constructor has not been registered for the type.");
        }

        var instance = context.RegisteredConstructor.Invoke();
        return (void*)instance.NativePtr;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static void Free_Native(void* userData, void* instance)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (GodotObject?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            // The 'free' callback is called when the unmanaged object is released,
            // clear the native pointer so the Dispose doesn't try to release it again.
            // Also free the GCHandle so it can be released on the managed side.
            instanceObj.NativePtr = 0;
            gcHandle.Free();

            instanceObj.Dispose();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private unsafe static delegate* unmanaged[Cdecl]<void*, void**, void*, void> GetVirtualFunctionPointer_Native(void* userData, NativeGodotStringName* name)
    {
        var gcHandle = GCHandle.FromIntPtr((nint)userData);
        var context = (ClassDBRegistrationContext?)gcHandle.Target;

        Debug.Assert(context is not null);

        StringName methodNameStr = StringName.CreateTakingOwnership(*name);

        if (!context.RegisteredVirtualMethodOverrides.TryGetValue(methodNameStr, out var virtualMethodInfo))
        {
            throw new InvalidOperationException($"Virtual method '{methodNameStr}' has not been registered in class '{context.ClassName}'.");
        }

        return (delegate* unmanaged[Cdecl]<void*, void**, void*, void>)Marshal.GetFunctionPointerForDelegate(virtualMethodInfo.Invoker.CallVirtualWithPtrArgs);
    }
}
