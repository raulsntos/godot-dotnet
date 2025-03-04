using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Godot.Bridge;
using Godot.NativeInterop;
using Godot.NativeInterop.Marshallers;

namespace Godot;

partial class GodotObject : IDisposable
{
    internal nint NativePtr;
    private readonly GCHandle _gcHandle;

    private readonly WeakReference<GodotObject>? _weakReferenceToSelf;

    private bool _disposed;

    private readonly PropertyInfoList _properties = [];
    internal PropertyInfoList GetPropertyListStorage() => _properties;

    /// <summary>
    /// Constructs a <see cref="GodotObject"/> with the given <paramref name="nativePtr"/>.
    /// </summary>
    /// <param name="nativePtr">The pointer to the native object in the engine's side.</param>
    protected internal GodotObject(nint nativePtr)
    {
        _gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);

        NativePtr = nativePtr;

        // If this object is RefCounted, initialize the reference count.
        if (this is RefCounted rc)
        {
            rc.InitRef();
        }

        _weakReferenceToSelf = DisposablesTracker.RegisterGodotObject(this);

        PostInitialize();
    }

    /// <summary>
    /// Constructs a <see cref="GodotObject"/> with the given <paramref name="nativeClassName"/>.
    /// </summary>
    /// <param name="nativeClassName">The name of the Godot engine class.</param>
    private protected GodotObject(scoped NativeGodotStringName nativeClassName) : this(ConstructGodotObject(nativeClassName)) { }

    private static unsafe nint ConstructGodotObject(scoped NativeGodotStringName nativeClassName)
    {
        return (nint)GodotBridge.GDExtensionInterface.classdb_construct_object2(&nativeClassName);
    }

    /// <summary>
    /// Constructs a new <see cref="GodotObject"/>.
    /// </summary>
    public GodotObject() : this(NativeName.NativeValue.DangerousSelfRef) { }

    private unsafe void PostInitialize()
    {
        nint gcHandlePtr = GCHandle.ToIntPtr(_gcHandle);

        if (IsUserDefinedType())
        {
            using NativeGodotStringName extensionClassName = NativeGodotStringName.Create(GetType().Name);
            GodotBridge.GDExtensionInterface.object_set_instance((void*)NativePtr, &extensionClassName, (void*)gcHandlePtr);
        }

        GDExtensionInstanceBindingCallbacks bindingsCallbacks = default;

        GodotBridge.GDExtensionInterface.object_set_instance_binding((void*)NativePtr, GodotBridge.LibraryPtr, (void*)gcHandlePtr, &bindingsCallbacks);

        Notification((int)NotificationPostinitialize);

        bool IsUserDefinedType()
        {
            // If this type is not defined in this assembly, it must be a user-defined type.
            return GetType().Assembly != typeof(GodotObject).Assembly;
        }
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (NativePtr != 0)
        {
            GodotBridge.GDExtensionInterface.object_free_instance_binding((void*)NativePtr, GodotBridge.LibraryPtr);

            _gcHandle.Free();
            NativePtr = 0;
        }

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
