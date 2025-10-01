using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;

namespace Godot.Bridge;

/// <summary>
/// Utility to register classes and their members within the Godot engine,
/// and to add or remove editor plugins.
/// </summary>
public static partial class GodotRegistry
{
    private static readonly Dictionary<Type, ClassRegistrationContext> _registeredClassesByType = [];
    private static readonly Dictionary<StringName, ClassRegistrationContext> _registeredClasses = new(StringNameEqualityComparer.Default);
    private static readonly Stack<StringName> _classRegisterStack = [];
    private static readonly HashSet<StringName> _manuallyUnregisteredClasses = new(StringNameEqualityComparer.Default);

    private readonly struct NotificationHandler
    {
        private readonly nint _methodPtr;

        public NotificationHandler(nint methodPtr)
        {
            _methodPtr = methodPtr;
        }

        public unsafe void Invoke(GodotObject instance, int what)
        {
            var function = (delegate* managed<GodotObject, int, void>)_methodPtr;
            function(instance, what);
        }
    }

    private static readonly Dictionary<Type, ImmutableArray<NotificationHandler>> _notificationHandlersByType = [];

    internal static bool TryGetClassRegistrationContext(Type type, [NotNullWhen(true)] out ClassRegistrationContext? context)
    {
        return _registeredClassesByType.TryGetValue(type, out context);
    }

    internal static bool TryGetClassRegistrationContext(NativeGodotStringName className, [NotNullWhen(true)] out ClassRegistrationContext? context)
    {
        var lookup = _registeredClasses.GetAlternateLookup<NativeGodotStringName>();
        return lookup.TryGetValue(className, out context);
    }

    /// <summary>
    /// Registers a class with a configuration function that registers its members.
    /// Classes registered with this method will also run in the editor, to avoid this
    /// use <see cref="RegisterRuntimeClass{T}(Action{ClassRegistrationContext})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] T>(Action<ClassRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: false, isExposed: true, isRuntime: false, configure);
    }

    /// <summary>
    /// Registers a runtime class with a configuration function that registers its members.
    /// Runtime classes don't run in the editor, to register classes that also run in the editor
    /// use <see cref="RegisterClass{T}(Action{ClassRegistrationContext})"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterRuntimeClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] T>(Action<ClassRegistrationContext> configure) where T : GodotObject
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
    public static void RegisterVirtualClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] T>(Action<ClassRegistrationContext> configure) where T : GodotObject
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
    public static void RegisterAbstractClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] T>(Action<ClassRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: true, isExposed: true, isRuntime: false, configure);
    }

    /// <summary>
    /// Registers an internal class with a configuration function that registers its members.
    /// Internal classes are hidden in the editor so users won't see them.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    /// <param name="configure">The configuration function.</param>
    public static void RegisterInternalClass<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] T>(Action<ClassRegistrationContext> configure) where T : GodotObject
    {
        RegisterClassCore<T>(isVirtual: false, isAbstract: false, isExposed: false, isRuntime: false, configure);
    }

    private static unsafe void RegisterClassCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] T>(bool isVirtual, bool isAbstract, bool isExposed, bool isRuntime, Action<ClassRegistrationContext> configure) where T : GodotObject
    {
        if (typeof(T).IsAbstract && !isAbstract)
        {
            // T is an abstract type but the isAbstract parameter is false.
            throw new ArgumentException(SR.FormatArgument_AbstractTypeCantBeRegisteredAsNonAbstract(typeof(T)), nameof(isAbstract));
        }

        StringName className = new StringName(typeof(T).Name);
        if (_registeredClasses.TryGetValue(className, out var context))
        {
            // If this class has been registered before, allow configuring it again.
            configure(context);
            context.RegisterBindings();
            return;
        }

        StringName? godotNativeName = GodotObject.GetGodotNativeName(typeof(T));

        // The 'BaseType' will never be null becase T has a constraint that
        // it must derive from GodotObject, but we assert this anyway so the
        // null analysis doesn't complain about it being null.
        Debug.Assert(godotNativeName is not null, $"Type '{typeof(T)}' must derive from a Godot type.");

        context = new ClassRegistrationContext(className, godotNativeName);
        _registeredClassesByType[typeof(T)] = context;
        _registeredClasses[className] = context;
        _classRegisterStack.Push(className);

        CacheNotificationHandlers(typeof(T));

        configure(context);

        NativeGodotString iconPathNative = default;
        if (!string.IsNullOrEmpty(context.IconPath))
        {
            iconPathNative = NativeGodotString.Create(context.IconPath);
        }

        var creationInfo = new GDExtensionClassCreationInfo6()
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
            recreate_instance_func = &Recreate_Native,
            get_virtual_func = null,
            get_virtual_call_data_func = &GetVirtualMethodUserData_Native,
            call_virtual_with_data_func = &CallVirtualMethod_Native,
            class_userdata = (void*)GCHandle<ClassRegistrationContext>.ToIntPtr(context.GCHandle),
            icon_path = &iconPathNative,
        };

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

        NativeGodotStringName* classNameNativePtr = className.NativeValue.DangerousSelfRef.GetUnsafeAddress();
        NativeGodotStringName* baseClassNameNativePtr = baseClassName.NativeValue.DangerousSelfRef.GetUnsafeAddress();

        GodotBridge.GDExtensionInterface.classdb_register_extension_class6(GodotBridge.LibraryPtr, classNameNativePtr, baseClassNameNativePtr, &creationInfo);

        context.RegisterBindings();

        InteropUtils.RegisterVirtualOverrides(typeof(T), context);
    }

    internal static unsafe void UnregisterAllClasses()
    {
        _registeredClassesByType.Clear();

        while (_classRegisterStack.TryPop(out StringName? className))
        {
            if (_manuallyUnregisteredClasses.Remove(className))
            {
                // This class was already unregistered manually, skip it.
                continue;
            }

            NativeGodotStringName* classNameNativePtr = className.NativeValue.DangerousSelfRef.GetUnsafeAddress();

            GodotBridge.GDExtensionInterface.classdb_unregister_extension_class(GodotBridge.LibraryPtr, classNameNativePtr);

            _registeredClasses[className].Dispose();
            _registeredClasses.Remove(className);
        }
    }

    /// <summary>
    /// Unregisters a single class that was previously registered.
    /// Child classes must be unregistered before parent classes.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    internal static unsafe void UnregisterClass<T>() where T : GodotObject
    {
        StringName className = new StringName(typeof(T).Name);

        if (!_registeredClasses.TryGetValue(className, out var context))
        {
            return;
        }

        NativeGodotStringName* classNameNativePtr = className.NativeValue.DangerousSelfRef.GetUnsafeAddress();

        GodotBridge.GDExtensionInterface.classdb_unregister_extension_class(GodotBridge.LibraryPtr, classNameNativePtr);

        context.Dispose();
        _registeredClasses.Remove(className);
        _registeredClassesByType.Remove(typeof(T));

        // Track that this class was manually unregistered so
        // 'UnregisterAllClasses' skips it when popping from the stack.
        _manuallyUnregisteredClasses.Add(className);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool Set_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* value)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                StringName nameManaged = StringName.CreateCopying(*name);
                Variant valueManaged = Variant.CreateCopying(*value);

                return instanceObj._Set(nameManaged, valueManaged);
            }

            return false;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return false;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool Get_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* outRet)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                StringName nameManaged = StringName.CreateCopying(*name);

                bool ok = instanceObj._Get(nameManaged, out Variant valueManaged);

                *outRet = NativeGodotVariant.Create(valueManaged.NativeValue.DangerousSelfRef);
                return ok;
            }

            return false;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return false;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe GDExtensionPropertyInfo* GetPropertyList_Native(void* instance, uint* outCount)
    {
        try
        {
            if (instance is null)
            {
                if (outCount is not null)
                {
                    *outCount = 0;
                }
                return null;
            }

            var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
            var instanceObj = gcHandle.Target;

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
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            if (outCount is not null)
            {
                *outCount = 0;
            }

            return null;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void FreePropertyList_Native(void* instance, GDExtensionPropertyInfo* propertyListPtr, uint count)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                var propertyList = instanceObj.GetPropertyListStorage();
                Debug.Assert(propertyList.Count == count);
                propertyList.Clear();

                PropertyInfoList.FreeNative(propertyListPtr, (int)count);
            }
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception)) { }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool PropertyCanRevert_Native(void* instance, NativeGodotStringName* name)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                StringName nameManaged = StringName.CreateCopying(*name);

                return instanceObj._PropertyCanRevert(nameManaged);
            }

            return false;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return false;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool PropertyGetRevert_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* outRet)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                StringName nameManaged = StringName.CreateCopying(*name);

                bool ok = instanceObj._PropertyGetRevert(nameManaged, out Variant valueManaged);

                *outRet = NativeGodotVariant.Create(valueManaged.NativeValue.DangerousSelfRef);
                return ok;
            }

            return false;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return false;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool ValidateProperty_Native(void* instance, GDExtensionPropertyInfo* refProperty)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                // Convert internal property info to the public managed type.
                VariantType type = (VariantType)refProperty->type;
                using StringName? name = StringNameMarshaller.ConvertFromUnmanaged(refProperty->name);
                using StringName? className = StringNameMarshaller.ConvertFromUnmanaged(refProperty->class_name);
                var propertyInfo = new PropertyInfo(name ?? StringName.Empty, type)
                {
                    Hint = (PropertyHint)refProperty->hint,
                    HintString = StringMarshaller.ConvertFromUnmanaged(refProperty->hint_string),
                    ClassName = className,
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
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return false;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void Notification_Native(void* instance, int what, bool reversed)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                DispatchNotification(instanceObj, what, reversed);
            }
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception)) { }
    }

    private static void DispatchNotification(GodotObject instanceObj, int what, bool reversed)
    {
        if (!_notificationHandlersByType.TryGetValue(instanceObj.GetType(), out var handlers))
        {
            // If there are no handlers, this type doesn't override _Notification.
            return;
        }

        if (!reversed)
        {
            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                handlers[i].Invoke(instanceObj, what);
            }
        }
        else
        {
            for (int i = 0; i < handlers.Length; i++)
            {
                handlers[i].Invoke(instanceObj, what);
            }
        }
    }

    private static void CacheNotificationHandlers([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
        if (_notificationHandlersByType.ContainsKey(type))
        {
            // Handlers for this type have already been cached.
            return;
        }

        var handlersList = new List<NotificationHandler>();

        MethodInfo? notificationMethod = type.GetMethod(
            nameof(GodotObject._Notification),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
            binder: null,
            [typeof(int)],
            modifiers: null);

        if (notificationMethod is not null && notificationMethod.GetBaseDefinition() != notificationMethod)
        {
            handlersList.Add(new NotificationHandler(notificationMethod.MethodHandle.GetFunctionPointer()));
        }

        Type? baseType = type.BaseType;
        if (baseType is not null && _notificationHandlersByType.TryGetValue(baseType, out ImmutableArray<NotificationHandler> baseHandlers))
        {
            handlersList.AddRange(baseHandlers);
        }

        _notificationHandlersByType[type] = [.. handlersList];
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void ToString_Native(void* instance, bool* outIsValid, NativeGodotString* outStr)
    {
        try
        {
            var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
            var instanceObj = gcHandle.Target;

            *outStr = NativeGodotString.Create(instanceObj.ToString());
            *outIsValid = true;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            *outIsValid = false;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void* Create_Native(void* userData, bool notifyPostInitialize)
    {
        try
        {
            var gcHandle = GCHandle<ClassRegistrationContext>.FromIntPtr((nint)userData);
            var context = gcHandle.Target;

            if (context.RegisteredConstructor is null)
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_CantInstantiateTypeConstructorNotRegistered(context.ClassName));
            }

            Debug.Assert(context.NativeClassName is not null);

            var instance = GodotObject.Create(context.RegisteredConstructor, new()
            {
                NativeClassName = context.NativeClassName,
                SkipPostInitializeNotification = true,
            });

            if (notifyPostInitialize)
            {
                instance.Notification((int)GodotObject.NotificationPostInitialize);
            }

            return (void*)instance.NativePtr;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return null;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void* Recreate_Native(void* userData, void* instanceNativePtr)
    {
        try
        {
            var gcHandleContext = GCHandle<ClassRegistrationContext>.FromIntPtr((nint)userData);
            var context = gcHandleContext.Target;

            if (context.RegisteredConstructor is null)
            {
                throw new InvalidOperationException(SR.FormatInvalidOperation_CantInstantiateTypeConstructorNotRegistered(context.ClassName));
            }

            Debug.Assert(context.NativeClassName is not null);

            var instance = GodotObject.Create(context.RegisteredConstructor, new()
            {
                NativePtr = (nint)instanceNativePtr,
                NativeClassName = context.NativeClassName,
            });

            return (void*)GCHandle<GodotObject>.ToIntPtr(instance.GCHandle);
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return null;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void Free_Native(void* userData, void* instance)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                // The 'free' callback is called when the unmanaged object is released,
                // clear the native pointer so the Dispose doesn't try to release it again.
                // Also free the GCHandle so it can be released on the managed side.
                instanceObj.NativePtr = 0;
                gcHandle.Dispose();

                instanceObj.Dispose();
            }
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception)) { }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe void FreeBindingCallback_Native(void* token, void* nativePtr, void* instance)
    {
        try
        {
            if (instance is not null)
            {
                var gcHandle = GCHandle<GodotObject>.FromIntPtr((nint)instance);
                var instanceObj = gcHandle.Target;

                // The 'free' callback is called when the unmanaged object is released,
                // clear the native pointer so the Dispose doesn't try to release it again.
                // Also free the GCHandle so it can be released on the managed side.
                instanceObj.NativePtr = 0;
                gcHandle.Dispose();

                instanceObj.Dispose();
            }
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception)) { }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe bool ReferenceBindingCallback_Native(void* token, void* nativePtr, bool reference)
    {
        return true;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void* GetVirtualMethodUserData_Native(void* userData, NativeGodotStringName* name, uint hash)
    {
        try
        {
            var gcHandle = GCHandle<ClassRegistrationContext>.FromIntPtr((nint)userData);
            var context = gcHandle.Target;

            Debug.Assert(name is not null);

            var lookup = context.RegisteredVirtualMethodOverrides.GetAlternateLookup<NativeGodotStringName>();
            if (!lookup.TryGetValue(*name, out var virtualMethodInfo))
            {
                // Virtual method not registered, it likely means it wasn't overridden.
                // Returning null so it falls back to the default implementation.
                return null;
            }

            return userData;
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception))
        {
            return null;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void CallVirtualMethod_Native(void* instance, NativeGodotStringName* name, void* userData, void** args, void* outRet)
    {
        try
        {
            var gcHandle = GCHandle<ClassRegistrationContext>.FromIntPtr((nint)userData);
            var context = gcHandle.Target;

            Debug.Assert(name is not null);

            var lookup = context.RegisteredVirtualMethodOverrides.GetAlternateLookup<NativeGodotStringName>();

            // We already checked that the method is registered in 'GetVirtualMethodUserData_Native',
            // this method would not have been called otherwise.
            Debug.Assert(lookup.ContainsKey(*name), $"Virtual method '{name->ToString()}' has not been registered in class '{context.ClassName}'.");

            var virtualMethodInfo = lookup[*name];
            virtualMethodInfo.Invoker.CallVirtualWithPtrArgs(instance, args, outRet);
        }
        catch (Exception exception) when (ExceptionHandling.IsHandled(exception)) { }
    }
}
