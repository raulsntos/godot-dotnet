using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// Callable is a first class object which can be held in variables and passed to functions.
/// It represents a given method in an Object, and is typically used for signal callbacks.
/// </summary>
/// <example>
/// <code>
/// public void PrintArgs(object ar1, object arg2, object arg3 = null)
/// {
///     GD.PrintS(arg1, arg2, arg3);
/// }
///
/// public void Test()
/// {
///     // This Callable object will call the PrintArgs method defined above.
///     Callable callable = new Callable(this, nameof(PrintArgs));
///     callable.Call("hello", "world"); // Prints "hello world null".
///     callable.Call(Vector2.Up, 42, callable); // Prints "(0, -1) 42 Node(Node.cs)::PrintArgs".
///     callable.Call("invalid"); // Invalid call, should have at least 2 arguments.
/// }
/// </code>
/// </example>
public readonly partial struct Callable
{
    internal readonly NativeGodotCallable.Movable NativeValue;

    private readonly CustomCallable? _customCallable;

    /// <summary>
    /// Object that contains the method or delegate. May be <see langword="null"/>
    /// if the method is static, or the object is not a <see cref="GodotObject"/>.
    /// </summary>
    public GodotObject? Target
    {
        get
        {
            ulong objectId = NativeValue.DangerousSelfRef.ObjectId;
            if (objectId == 0)
            {
                return null;
            }

            return GodotObject.InstanceFromId(objectId);
        }
    }

    /// <summary>
    /// Name of the method that will be called if this Callable was created from a method name.
    /// </summary>
    public StringName? Method
    {
        get
        {
            NativeGodotStringName method = NativeValue.DangerousSelfRef.Method;
            if (!method.IsAllocated)
            {
                return null;
            }

            return StringName.CreateTakingOwnership(method);
        }
    }

    /// <summary>
    /// If this is a custom Callable, contains the <see cref="CustomCallable"/> that created it.
    /// Otherwise, <see langword="null"/>.
    /// </summary>
    public CustomCallable? Custom => _customCallable;

    /// <summary>
    /// Delegate of the method that will be called if this Callable was created from a delegate.
    /// </summary>
    public Delegate? Delegate =>
        _customCallable is DelegateCallable delegateCallable
            ? delegateCallable.Delegate
            : null;

    private Callable(NativeGodotCallable nativeValueToOwn)
    {
        NativeValue = nativeValueToOwn.AsMovable();
    }

    private Callable(CustomCallable nativeValueToOwn)
    {
        NativeValue = nativeValueToOwn.ConstructCallable().AsMovable();
        _customCallable = nativeValueToOwn;
    }

    /// <summary>
    /// Constructs a new <see cref="Callable"/> from the value borrowed from
    /// <paramref name="nativeValueToOwn"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static Callable CreateTakingOwnership(NativeGodotCallable nativeValueToOwn)
    {
        return new Callable(nativeValueToOwn);
    }

    /// <summary>
    /// Constructs a new <see cref="Callable"/> from the value borrowed from
    /// <paramref name="customCallable"/>, taking ownership of the value.
    /// Since the new instance references the same value, disposing the new
    /// instance will also dispose the original value.
    /// </summary>
    internal static Callable CreateTakingOwnership(CustomCallable customCallable)
    {
        return new Callable(customCallable);
    }

    /// <summary>
    /// Constructs a new <see cref="Callable"/> for the method called <paramref name="method"/>
    /// in the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">Object that contains the method.</param>
    /// <param name="method">Name of the method that will be called.</param>
    public unsafe Callable(GodotObject target, StringName method)
    {
        NativeValue = NativeGodotCallable.Create(GodotObject.GetNativePtr(target), method.NativeValue.DangerousSelfRef).AsMovable();
    }

    /// <summary>
    /// Calls the method represented by this <see cref="Callable"/>.
    /// Arguments can be passed and should match the method's signature.
    /// </summary>
    /// <param name="args">Arguments that will be passed to the method call.</param>
    /// <returns>The value returned by the method.</returns>
    public Variant Call(ReadOnlySpan<Variant> args = default)
    {
        ref NativeGodotCallable self = ref NativeValue.DangerousSelfRef;
        NativeGodotVariant ret = NativeGodotCallable.Call(ref self, args);
        return Variant.CreateTakingOwnership(ret);
    }

    /// <summary>
    /// Calls the method represented by this <see cref="Callable"/> in deferred mode, i.e. during the idle frame.
    /// Arguments can be passed and should match the method's signature.
    /// </summary>
    /// <param name="args">Arguments that will be passed to the method call.</param>
    public void CallDeferred(ReadOnlySpan<Variant> args = default)
    {
        ref NativeGodotCallable self = ref NativeValue.DangerousSelfRef;
        NativeGodotCallable.CallDeferred(ref self, args);
    }

    /// <summary>
    /// Constructs a new <see cref="Callable"/> with one or more arguments bound.
    /// When called, the bound arguments are passed <i>after</i> the arguments supplied by
    /// <see cref="Call"/>. See also <see cref="Unbind(int)"/>.
    /// <b>Note:</b> When this method is chained with other similar methods, the order in
    /// which the argument list is modified is read from right to left.
    /// </summary>
    /// <param name="args">Arguments to bind to the new Callable.</param>
    /// <returns>A new Callable with the arguments bound.</returns>
    public Callable Bind(ReadOnlySpan<Variant> args)
    {
        ref NativeGodotCallable self = ref NativeValue.DangerousSelfRef;
        return new Callable(NativeGodotCallable.Bind(ref self, args));
    }

    /// <summary>
    /// Constructs a new <see cref="Callable"/> with a number of arguments unbound.
    /// In other words, when the new callable is called the last few arguments supplied
    /// by the user are ignored, according to <paramref name="argCount"/>.
    /// The remaining arguments are passed to the callable. This allows to use the
    /// original callable in a context that attempts to pass more arguments than this
    /// callable can handle, e.g. a signal with a fixed number of arguments.
    /// See also <see cref="Bind(ReadOnlySpan{Variant})"/>.
    /// <b>Note:</b> When this method is chained with other similar methods, the order in
    /// which the argument list is modified is read from right to left.
    /// </summary>
    /// <example>
    /// <code>
    /// foo.Unbind(1).Call(1, 2); // Calls foo(1).
    /// foo.Bind(3, 4).Unbind(1).Call(1, 2); // Calls foo(1, 3, 4), note that it does not change the arguments from bind.
    /// </code>
    /// </example>
    /// <param name="argCount">Number of arguments to unbind in the new Callable.</param>
    /// <returns>A new Callable with the arguments unbound.</returns>
    public Callable Unbind(int argCount)
    {
        if (argCount < 0)
        {
            throw new ArgumentException("Argument count can't be negative.", nameof(argCount));
        }

        ref NativeGodotCallable self = ref NativeValue.DangerousSelfRef;
        return new Callable(NativeGodotCallable.Unbind(ref self, argCount));
    }

    /// <summary>
    /// <para>
    /// Constructs a new <see cref="Callable"/> using the <paramref name="trampoline"/>
    /// function pointer to dynamically invoke the given <paramref name="delegate"/>.
    /// </para>
    /// <para>
    /// The parameters passed to the <paramref name="trampoline"/> function are:
    /// </para>
    /// <list type="number">
    ///    <item>
    ///        <term>delegateObj</term>
    ///        <description>The given <paramref name="delegate"/>, upcast to <see cref="object"/>.</description>
    ///    </item>
    ///    <item>
    ///        <term>args</term>
    ///        <description>Array of <see cref="NativeGodotVariant"/> arguments.</description>
    ///    </item>
    ///    <item>
    ///        <term>ret</term>
    ///        <description>Return value of type <see cref="NativeGodotVariant"/>.</description>
    ///    </item>
    ///</list>
    /// <para>
    /// The delegate should be downcast to a more specific delegate type before invoking.
    /// </para>
    /// </summary>
    /// <example>
    /// Usage example:
    ///
    /// <code>
    ///     static void Trampoline(object delegateObj, NativeGodotVariantPtrSpan args, out NativeGodotVariant ret)
    ///     {
    ///         if (args.Count != 1)
    ///             throw new ArgumentException($&quot;Callable expected {1} arguments but received {args.Count}.&quot;);
    ///
    ///         TResult res = ((Func&lt;int, string&gt;)delegateObj)(
    ///             VariantConversionCallbacks.GetToManagedCallback&lt;int&gt;()(args[0])
    ///         );
    ///
    ///         ret = VariantConversionCallbacks.GetToVariantCallback&lt;string&gt;()(res);
    ///     }
    ///
    ///     var callable = Callable.CreateWithUnsafeTrampoline((int num) =&gt; &quot;foo&quot; + num.ToString(), &amp;Trampoline);
    ///     var res = (string)callable.Call(10);
    ///     Console.WriteLine(res);
    /// </code>
    /// </example>
    /// <param name="delegate">Delegate method that will be called.</param>
    /// <param name="trampoline">Trampoline function pointer for invoking the delegate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe Callable CreateWithUnsafeTrampoline(Delegate @delegate, delegate* managed<object, NativeGodotVariantPtrSpan, out NativeGodotVariant, void> trampoline)
    {
        return new Callable(new DelegateCallable(@delegate, trampoline));
    }

    private static void ThrowIfArgCountMismatch(NativeGodotVariantPtrSpan args, int countExpected,
        [CallerArgumentExpression(nameof(args))] string? paramName = null)
    {
        if (countExpected != args.Length)
        {
            ThrowArgCountMismatch(countExpected, args.Length, paramName);
        }

        [DoesNotReturn]
        static void ThrowArgCountMismatch(int countExpected, int countReceived, string? paramName)
        {
            throw new ArgumentException($"Invalid argument count for invoking callable. Expected {countExpected} arguments, received {countReceived}.", paramName);
        }
    }
}
