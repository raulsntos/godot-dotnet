using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Godot.Bridge;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;

namespace Godot;

partial class GodotObject : IDisposable
{
    internal nint NativePtr;
    internal readonly GCHandle GCHandle;

    private readonly WeakReference<GodotObject>? _weakReferenceToSelf;

    private static readonly AsyncLocal<GodotObjectCreationOptions?> _creationOptions = new();

    private bool _disposing;
    private bool _disposed;

    private readonly PropertyInfoList _properties = [];
    internal PropertyInfoList GetPropertyListStorage() => _properties;

    internal struct GodotObjectCreationOptions
    {
        /// <summary>
        /// Name of the built-in Godot class, not the user-defined type.
        /// If the type is user-defined, this must be the name of its closest built-in ancestor.
        /// </summary>
        internal required StringName NativeClassName;

        /// <summary>
        /// The pointer to the existing native instance to construct the managed object for,
        /// or zero to create a new native instance.
        /// This must always be zero, unless the object is being recreated from an existing
        /// native instance (<see cref="GodotRegistry.Recreate_Native"/>).
        /// </summary>
        internal nint NativePtr;

        /// <summary>
        /// Whether the instance binding is already bound to the native instance.
        /// This must only be <see langword="true"/> when constructing from
        /// <see cref="CreateBindingCallback_Native(void*, void*)"/> because the engine already bound it.
        /// </summary>
        internal bool InstanceBindingAlreadyBound;

        /// <summary>
        /// Whether to emit the <see cref="NotificationPostinitialize"/> notification.
        /// This must be <see langword="true"/> to properly finish initialization of the object.
        /// However, when constructing from <see cref="GodotRegistry.Create_Native(void*, bool)"/>,
        /// it depends on the <c>notifyPostInitialize</c> parameter because the engine controls it.
        /// </summary>
        internal bool EmitPostInitializeNotification;

        /// <summary>
        /// If the type is a <see cref="RefCounted"/>, whether to call <see cref="RefCounted.InitRef"/>
        /// after construction.
        /// This must be <see langword="true"/> when creating new ref counted instances from C#.
        /// </summary>
        internal bool InitRef;
    }

    // IMPORTANT: This method relies on a static variable to pass the creation options to the constructor.
    // This means recursive calls to Create() will not work as expected, and creating other GodotObjects
    // from within a registered constructor method may lead to unexpected behavior. That's why it should
    // be generally avoided unless you really know what you're doing.
    internal static T Create<T>(Func<T> constructor, GodotObjectCreationOptions options) where T : GodotObject
    {
        try
        {
            _creationOptions.Value = options;
            return constructor();
        }
        finally
        {
            _creationOptions.Value = null;
        }
    }

    private unsafe GodotObject(GodotObjectCreationOptions options)
    {
        GCHandle = GCHandle.Alloc(this, GCHandleType.Normal);
        nint gcHandlePtr = GCHandle.ToIntPtr(GCHandle);
        _weakReferenceToSelf = DisposablesTracker.RegisterGodotObject(this);

        Debug.Assert(options.NativeClassName is not null, "'NativeClassName' must be provided.");

        NativePtr = options.NativePtr;
        if (NativePtr == 0)
        {
            NativePtr = (nint)GodotBridge.GDExtensionInterface.classdb_construct_object2(options.NativeClassName.NativeValue.DangerousSelfRef.GetUnsafeAddress());
        }

        if (IsUserDefinedType())
        {
            using NativeGodotStringName extensionClassName = NativeGodotStringName.Create(GetType().Name);
            GodotBridge.GDExtensionInterface.object_set_instance((void*)NativePtr, &extensionClassName, (void*)gcHandlePtr);
        }

        if (!options.InstanceBindingAlreadyBound)
        {
            var bindingCallbacks = GDExtensionInstanceBindingCallbacks.Default;
            if (!IsUserDefinedType())
            {
                if (!InteropUtils.BindingCallbacks.TryGetValue(options.NativeClassName, out bindingCallbacks))
                {
                    throw new InvalidOperationException(SR.FormatInvalidOperation_BindingCallbacksNotFound(GetType()));
                }
            }

            GodotBridge.GDExtensionInterface.object_set_instance_binding((void*)NativePtr, GodotBridge.LibraryPtr, (void*)gcHandlePtr, &bindingCallbacks);
        }

        if (options.EmitPostInitializeNotification)
        {
            Notification((int)NotificationPostinitialize);
        }

        if (this is RefCounted refCounted && options.InitRef)
        {
            refCounted.InitRef();
        }

        bool IsUserDefinedType()
        {
            // If this type is not defined in this assembly, it must be a user-defined type.
            return GetType().Assembly != typeof(GodotObject).Assembly;
        }
    }

    /// <summary>
    /// Constructs a <see cref="GodotObject"/> with the given <paramref name="nativeClassName"/>.
    /// </summary>
    /// <param name="nativeClassName">The name of the Godot engine class.</param>
    private protected GodotObject(scoped NativeGodotStringName nativeClassName) : this(GetCreationOptions() ?? new GodotObjectCreationOptions()
    {
        NativeClassName = StringName.CreateTakingOwnership(nativeClassName),
        EmitPostInitializeNotification = true,
        InitRef = true,
    })
    { }

    /// <summary>
    /// Constructs a new <see cref="GodotObject"/>.
    /// </summary>
    public GodotObject() : this(GetCreationOptions() ?? new GodotObjectCreationOptions()
    {
        NativeClassName = NativeName,
        EmitPostInitializeNotification = true,
        InitRef = true,
    })
    { }

    private static GodotObjectCreationOptions? GetCreationOptions()
    {
        var creationOptions = _creationOptions.Value;

        if (creationOptions is not null)
        {
            _creationOptions.Value = null;
            return creationOptions;
        }

        return null;
    }

    /// <summary>
    /// Get the pointer to the native instance represented by the object <paramref name="instance"/>,
    /// or <see cref="IntPtr.Zero"/> if the object is null.
    /// </summary>
    /// <param name="instance">Godot object to get the pointer from.</param>
    /// <returns>The pointer to the Godot object.</returns>
    /// <exception cref="ObjectDisposedException">
    /// <paramref name="instance"/> has been previously disposed or its native instance has been released.
    /// </exception>
    internal static nint GetNativePtr(GodotObject? instance)
    {
        if (instance is null)
        {
            return 0;
        }

        // We check if NativePtr is null because this may be called by the debugger.
        // If the debugger puts a breakpoint in one of the base constructors, before
        // NativePtr is assigned, that would result in UB or crashes when calling
        // native functions that receive the pointer, which can happen because the
        // debugger calls ToString() and tries to get the value of properties.
        ObjectDisposedException.ThrowIf(instance._disposed || instance.NativePtr == 0, instance);

        return instance.NativePtr;
    }

    /// <summary>
    /// Returns the <see cref="GodotObject"/> that corresponds to <paramref name="instanceId"/>.
    /// All Objects have a unique instance ID. See also <see cref="GetInstanceId"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// public partial class MyNode : Node
    /// {
    ///     public string Foo { get; set; } = "bar";
    ///
    ///     public override void _Ready()
    ///     {
    ///         ulong id = GetInstanceId();
    ///         var inst = (MyNode)InstanceFromId(Id);
    ///         GD.Print(inst.Foo); // Prints bar
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <param name="instanceId">Instance ID of the Object to retrieve.</param>
    /// <returns>The <see cref="GodotObject"/> instance.</returns>
    public static unsafe GodotObject? InstanceFromId(ulong instanceId)
    {
        nint objectPtr = (nint)GodotBridge.GDExtensionInterface.object_get_instance_from_id(instanceId);
        return GodotObjectMarshaller.GetOrCreateManagedInstance(objectPtr);
    }

    /// <summary>
    /// Returns the object's unique instance ID. This ID can be saved in <see cref="EncodedObjectAsId"/>,
    /// and can be used to retrieve this object instance with <see cref="InstanceFromId"/>.
    /// </summary>
    public unsafe ulong GetInstanceId()
    {
        return GodotBridge.GDExtensionInterface.object_get_instance_id((void*)NativePtr);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="GodotObject"/> that corresponds
    /// to <paramref name="instanceId"/> is a valid object (e.g. has not been deleted from
    /// memory). All Objects have a unique instance ID.
    /// </summary>
    /// <param name="instanceId">The Object ID to check.</param>
    /// <returns>If the instance with the given ID is a valid object.</returns>
    public static unsafe bool IsInstanceIdValid(ulong instanceId)
    {
        return GodotBridge.GDExtensionInterface.object_get_instance_from_id(instanceId) is not null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="instance"/> is a
    /// valid <see cref="GodotObject"/> (e.g. has not been deleted from memory).
    /// </summary>
    /// <param name="instance">The instance to check.</param>
    /// <returns>If the instance is a valid object.</returns>
    public static bool IsInstanceValid([NotNullWhen(true)] GodotObject? instance)
    {
        return instance is not null && instance.NativePtr != 0;
    }

    /// <summary>
    /// Releases the unmanaged <see cref="GodotObject"/> instance.
    /// </summary>
    ~GodotObject()
    {
        Dispose(false);
    }

    /// <summary>
    /// Disposes of this <see cref="GodotObject"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes implementation of this <see cref="GodotObject"/>.
    /// </summary>
    protected virtual unsafe void Dispose(bool disposing)
    {
        if (_disposing || _disposed)
        {
            return;
        }

        // IMPORTANT: We set '_disposing' to true very early to avoid re-entrancy issues.
        // The free/destroy methods below may trigger a callback that tries to dispose again.
        // However, '_disposed' must only be set to true at the end, because the 'Unreference'
        // method checks if the object is already disposed and would throw.
        _disposing = true;

        if (NativePtr != 0)
        {
            nint nativePtr = NativePtr;
            GodotBridge.GDExtensionInterface.object_free_instance_binding((void*)NativePtr, GodotBridge.LibraryPtr);

            if (this is RefCounted rc)
            {
                // If this object is RefCounted, decrease the reference count.
                // The previous call to `object_free_instance_binding` will have cleared `NativePtr`,
                // we need to restore it for the `Unreference` call.
                NativePtr = nativePtr;
                if (rc.Unreference())
                {
                    // If the reference count reached zero, we need to free the native instance.
                    GodotBridge.GDExtensionInterface.object_destroy((void*)NativePtr);
                }
            }

            NativePtr = 0;
        }

        _disposed = true;

        if (_weakReferenceToSelf is not null)
        {
            DisposablesTracker.UnregisterGodotObject(_weakReferenceToSelf);
        }
    }

    /// <summary>
    /// Destroyes the <see cref="GodotObject"/> instance in the Godot engine.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Object is a <see cref="RefCounted"/> and cannot be freed manually.
    /// </exception>
    public unsafe void Free()
    {
        if (this is RefCounted)
        {
            throw new InvalidOperationException("RefCounted objects are freed automatically when their reference count reaches zero.");
        }

        ObjectDisposedException.ThrowIf(_disposing || _disposed, this);

        _disposing = true;

        if (NativePtr != 0)
        {
            GodotBridge.GDExtensionInterface.object_destroy((void*)NativePtr);
            NativePtr = 0;
        }

        _disposed = true;

        if (_weakReferenceToSelf is not null)
        {
            DisposablesTracker.UnregisterGodotObject(_weakReferenceToSelf);
        }
    }

    /// <summary>
    /// Converts this <see cref="GodotObject"/> to a string.
    /// </summary>
    /// <returns>A string representation of this object.</returns>
    public override string ToString()
    {
        // Cannot happen in C#; would get an ObjectDisposedException instead.
        Debug.Assert(GetNativePtr(this) != 0);

        // Can't call 'Object::to_string()' here, as that can end up calling
        // 'ToString' again resulting in an endless circular loop.
        return $"<{GetClass()}#{GetInstanceId()}>";
    }

    /// <summary>
    /// Returns a weak reference to an object, or <see langword="null"/>
    /// if the argument is invalid.
    /// A weak reference to an object is not enough to keep the object alive:
    /// when the only remaining references to a referent are weak references,
    /// garbage collection is free to destroy the referent and reuse its memory
    /// for something else. However, until the object is actually destroyed the
    /// weak reference may return the object even if there are no strong references
    /// to it.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>
    /// The <see cref="WeakRef"/> reference to the object or <see langword="null"/>.
    /// </returns>
    public static WeakRef? WeakRef(GodotObject? obj)
    {
        if (!IsInstanceValid(obj))
        {
            return null;
        }

        // The utility function `weakref` returns Variant because it may
        // return null, in this case `As<T>` will also return null.
        NativeGodotVariant objNative = NativeGodotVariant.CreateFromObject(GetNativePtr(obj));
        Variant weakref = Variant.CreateTakingOwnership(UtilityFunctions.Weakref(objNative));
        return weakref.As<WeakRef?>();
    }

    /// <summary>
    /// Returns a new <see cref="SignalAwaiter"/> awaiter configured to complete when the instance
    /// <paramref name="source"/> emits the signal specified by the <paramref name="signal"/> parameter.
    /// </summary>
    /// <param name="source">
    /// The instance the awaiter will be listening to.
    /// </param>
    /// <param name="signal">
    /// The signal the awaiter will be waiting for.
    /// </param>
    /// <example>
    /// This sample prints a message once every frame up to 100 times.
    /// <code>
    /// public override void _Ready()
    /// {
    ///     for (int i = 0; i &lt; 100; i++)
    ///     {
    ///         await ToSignal(GetTree(), "process_frame");
    ///         GD.Print($"Frame {i}");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <returns>
    /// A <see cref="SignalAwaiter"/> that completes when
    /// <paramref name="source"/> emits the <paramref name="signal"/>.
    /// </returns>
    public SignalAwaiter ToSignal(GodotObject source, StringName signal)
    {
        return new SignalAwaiter(source, signal, this);
    }
}
