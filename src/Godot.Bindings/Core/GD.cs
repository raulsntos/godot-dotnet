using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot;

/// <summary>
/// Godot's global functions.
/// </summary>
public static partial class GD
{
    /// <summary>
    /// Decodes a byte array back to a <see cref="Variant"/> value, without decoding objects.
    /// Note: If you need object deserialization, see <see cref="BytesToVarWithObjects"/>.
    /// </summary>
    /// <param name="bytes">Byte array that will be decoded to a <see cref="Variant"/>.</param>
    /// <returns>The decoded <see cref="Variant"/>.</returns>
    public static Variant BytesToVar(ReadOnlySpan<byte> bytes)
    {
        using NativeGodotPackedByteArray array = NativeGodotPackedByteArray.Create(bytes);
        return Variant.CreateTakingOwnership(UtilityFunctions.BytesToVar(array));
    }

    /// <summary>
    /// Decodes a byte array back to a <see cref="Variant"/> value. Decoding objects is allowed.
    /// Warning: Deserialized object can contain code which gets executed. Do not use this
    /// option if the serialized object comes from untrusted sources to avoid potential security
    /// threats (remote code execution).
    /// </summary>
    /// <param name="bytes">Byte array that will be decoded to a <see cref="Variant"/>.</param>
    /// <returns>The decoded <see cref="Variant"/>.</returns>
    public static Variant BytesToVarWithObjects(ReadOnlySpan<byte> bytes)
    {
        using NativeGodotPackedByteArray array = NativeGodotPackedByteArray.Create(bytes);
        return Variant.CreateTakingOwnership(UtilityFunctions.BytesToVarWithObjects(array));
    }

    /// <summary>
    /// Converts <paramref name="what"/> to <paramref name="type"/> in the best way possible.
    /// The <paramref name="type"/> parameter uses the <see cref="VariantType"/> values.
    /// </summary>
    /// <example>
    /// <code>
    /// Variant a = new GodotArray { 4, 2.5, 1.2 };
    /// GD.Print(a.VariantType == VariantType.Array); // Prints true
    ///
    /// var b = GD.TypeConvert(a, VariantType.PackedByteArray);
    /// GD.Print(b); // Prints [4, 2, 1]
    /// GD.Print(b.VariantType == VariantType.Array); // Prints false
    /// </code>
    /// </example>
    /// <returns>The <c>Variant</c> converted to the given <paramref name="type"/>.</returns>
    public static Variant TypeConvert(Variant what, VariantType type)
    {
        return Variant.CreateTakingOwnership(UtilityFunctions.TypeConvert(what.NativeValue.DangerousSelfRef, (long)type));
    }

    /// <summary>
    /// Returns the integer hash of the passed <paramref name="var"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Print(GD.Hash("a")); // Prints 177670
    /// </code>
    /// </example>
    /// <param name="var">Variable that will be hashed.</param>
    /// <returns>Hash of the variable passed.</returns>
    public static int Hash(Variant var)
    {
        return (int)UtilityFunctions.Hash(var.NativeValue.DangerousSelfRef);
    }

    /// <summary>
    /// Loads a resource from the filesystem located at <paramref name="path"/>.
    /// The resource is loaded on the method call (unless it's referenced already
    /// elsewhere, e.g. in another script or in the scene), which might cause slight delay,
    /// especially when loading scenes. To avoid unnecessary delays when loading something
    /// multiple times, either store the resource in a variable.
    ///
    /// Note: Resource paths can be obtained by right-clicking on a resource in the FileSystem
    /// dock and choosing "Copy Path" or by dragging the file from the FileSystem dock into the script.
    ///
    /// Important: The path must be absolute, a local path will just return <see langword="null"/>.
    /// This method is a simplified version of <see cref="ResourceLoader.Load"/>, which can be used
    /// for more advanced scenarios.
    /// </summary>
    /// <example>
    /// <code>
    /// // Load a scene called main located in the root of the project directory and cache it in a variable.
    /// var main = GD.Load("res://main.tscn"); // main will contain a PackedScene resource.
    /// </code>
    /// </example>
    /// <param name="path">Path of the <see cref="Resource"/> to load.</param>
    /// <returns>The loaded <see cref="Resource"/>.</returns>
    public static Resource Load(string path)
    {
        return ResourceLoader.Singleton.Load(path);
    }

    /// <summary>
    /// Loads a resource from the filesystem located at <paramref name="path"/>.
    /// The resource is loaded on the method call (unless it's referenced already
    /// elsewhere, e.g. in another script or in the scene), which might cause slight delay,
    /// especially when loading scenes. To avoid unnecessary delays when loading something
    /// multiple times, either store the resource in a variable.
    ///
    /// Note: Resource paths can be obtained by right-clicking on a resource in the FileSystem
    /// dock and choosing "Copy Path" or by dragging the file from the FileSystem dock into the script.
    ///
    /// Important: The path must be absolute, a local path will just return <see langword="null"/>.
    /// This method is a simplified version of <see cref="ResourceLoader.Load"/>, which can be used
    /// for more advanced scenarios.
    /// </summary>
    /// <example>
    /// <code>
    /// // Load a scene called main located in the root of the project directory and cache it in a variable.
    /// var main = GD.Load&lt;PackedScene&gt;("res://main.tscn"); // main will contain a PackedScene resource.
    /// </code>
    /// </example>
    /// <param name="path">Path of the <see cref="Resource"/> to load.</param>
    /// <typeparam name="T">The type to cast to. Should be a descendant of <see cref="Resource"/>.</typeparam>
    public static T Load<T>(string path) where T : class
    {
        return (T)(object)ResourceLoader.Singleton.Load(path);
    }

    private static string AppendPrintParams(object[] parameters)
    {
        if (parameters is null)
        {
            return "null";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < parameters.Length; i++)
        {
            sb.Append(parameters[i]?.ToString() ?? "null");
        }
        return sb.ToString();
    }

    private static string AppendPrintParams(char separator, object[] parameters)
    {
        if (parameters is null)
        {
            return "null";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i != 0)
            {
                sb.Append(separator);
            }
            sb.Append(parameters[i]?.ToString() ?? "null");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Prints a message to the console.
    ///
    /// Note: Consider using <see cref="PushError(string)"/> and <see cref="PushWarning(string)"/>
    /// to print error and warning messages instead of <see cref="Print(string)"/>.
    /// This distinguishes them from print messages used for debugging purposes,
    /// while also displaying a stack trace when an error or warning is printed.
    /// </summary>
    /// <param name="what">Message that will be printed.</param>
    public static void Print(string what)
    {
        using NativeGodotVariant strNative = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(what));
        UtilityFunctions.Print(strNative);
    }

    /// <summary>
    /// Converts one or more arguments of any type to string in the best way possible
    /// and prints them to the console.
    ///
    /// Note: Consider using <see cref="PushError(object[])"/> and <see cref="PushWarning(object[])"/>
    /// to print error and warning messages instead of <see cref="Print(object[])"/>.
    /// This distinguishes them from print messages used for debugging purposes,
    /// while also displaying a stack trace when an error or warning is printed.
    /// </summary>
    /// <example>
    /// <code>
    /// GodotArray a = [1, 2, 3];
    /// GD.Print("a", "b", a); // Prints ab[1, 2, 3]
    /// </code>
    /// </example>
    /// <param name="what">Arguments that will be printed.</param>
    public static void Print(params object[] what)
    {
        Print(AppendPrintParams(what));
    }

    /// <summary>
    /// Prints a message to the console.
    /// The following BBCode tags are supported: b, i, u, s, indent, code, url, center,
    /// right, color, bgcolor, fgcolor.
    /// Color tags only support named colors such as <c>red</c>, not hexadecimal color codes.
    /// Unsupported tags will be left as-is in standard output.
    /// When printing to standard output, the supported subset of BBCode is converted to
    /// ANSI escape codes for the terminal emulator to display. Displaying ANSI escape codes
    /// is currently only supported on Linux and macOS. Support for ANSI escape codes may vary
    /// across terminal emulators, especially for italic and strikethrough.
    ///
    /// Note: Consider using <see cref="PushError(string)"/> and <see cref="PushWarning(string)"/>
    /// to print error and warning messages instead of <see cref="Print(string)"/> or
    /// <see cref="PrintRich(string)"/>.
    /// This distinguishes them from print messages used for debugging purposes,
    /// while also displaying a stack trace when an error or warning is printed.
    /// </summary>
    /// <param name="what">Message that will be printed.</param>
    public static void PrintRich(string what)
    {
        using NativeGodotVariant strNative = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(what));
        UtilityFunctions.PrintRich(strNative);
    }

    /// <summary>
    /// Converts one or more arguments of any type to string in the best way possible
    /// and prints them to the console.
    /// The following BBCode tags are supported: b, i, u, s, indent, code, url, center,
    /// right, color, bgcolor, fgcolor.
    /// Color tags only support named colors such as <c>red</c>, not hexadecimal color codes.
    /// Unsupported tags will be left as-is in standard output.
    /// When printing to standard output, the supported subset of BBCode is converted to
    /// ANSI escape codes for the terminal emulator to display. Displaying ANSI escape codes
    /// is currently only supported on Linux and macOS. Support for ANSI escape codes may vary
    /// across terminal emulators, especially for italic and strikethrough.
    ///
    /// Note: Consider using <see cref="PushError(object[])"/> and <see cref="PushWarning(object[])"/>
    /// to print error and warning messages instead of <see cref="Print(object[])"/> or
    /// <see cref="PrintRich(object[])"/>.
    /// This distinguishes them from print messages used for debugging purposes,
    /// while also displaying a stack trace when an error or warning is printed.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PrintRich("[code][b]Hello world![/b][/code]"); // Prints out: [b]Hello world![/b]
    /// </code>
    /// </example>
    /// <param name="what">Arguments that will be printed.</param>
    public static void PrintRich(params object[] what)
    {
        PrintRich(AppendPrintParams(what));
    }

    /// <summary>
    /// Prints a message to standard error line.
    /// </summary>
    /// <param name="what">Message that will be printed.</param>
    public static void PrintErr(string what)
    {
        using NativeGodotVariant strNative = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(what));
        UtilityFunctions.Printerr(strNative);
    }

    /// <summary>
    /// Prints one or more arguments to strings in the best way possible to standard error line.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PrintErr("prints to stderr");
    /// </code>
    /// </example>
    /// <param name="what">Arguments that will be printed.</param>
    public static void PrintErr(params object[] what)
    {
        PrintErr(AppendPrintParams(what));
    }

    /// <summary>
    /// Prints a message to the OS terminal.
    /// Unlike <see cref="Print(string)"/>, no newline is added at the end.
    /// </summary>
    /// <param name="what">Message that will be printed.</param>
    public static void PrintRaw(string what)
    {
        using NativeGodotVariant strNative = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(what));
        UtilityFunctions.Printraw(strNative);
    }

    /// <summary>
    /// Prints one or more arguments to strings in the best way possible to the OS terminal.
    /// Unlike <see cref="Print(object[])"/>, no newline is added at the end.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PrintRaw("A");
    /// GD.PrintRaw("B");
    /// GD.PrintRaw("C");
    /// // Prints ABC to terminal
    /// </code>
    /// </example>
    /// <param name="what">Arguments that will be printed.</param>
    public static void PrintRaw(params object[] what)
    {
        PrintRaw(AppendPrintParams(what));
    }

    /// <summary>
    /// Prints one or more arguments to the console with a space between each argument.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PrintS("A", "B", "C"); // Prints A B C
    /// </code>
    /// </example>
    /// <param name="what">Arguments that will be printed.</param>
    public static void PrintS(params object[] what)
    {
        string message = AppendPrintParams(' ', what);
        using NativeGodotVariant strNative = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(message));
        UtilityFunctions.Prints(strNative);
    }

    /// <summary>
    /// Prints one or more arguments to the console with a tab between each argument.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PrintT("A", "B", "C"); // Prints A       B       C
    /// </code>
    /// </example>
    /// <param name="what">Arguments that will be printed.</param>
    public static void PrintT(params object[] what)
    {
        string message = AppendPrintParams('\t', what);
        using NativeGodotVariant strNative = NativeGodotVariant.CreateFromStringTakingOwnership(NativeGodotString.Create(message));
        UtilityFunctions.Printt(strNative);
    }

    private enum ErrorHandlerType
    {
        Error,
        Warning,
        Script,
        Shader,
    }

    [StackTraceHidden]
    private unsafe static void ErrPrintError(string message, ErrorHandlerType type = ErrorHandlerType.Error)
    {
        // TODO: Getting the caller information using the StackTrace API is problematic:
        //
        // - It won't work in NativeAOT:
        //   - StackFrame.GetMethod requires unrefenced code, the method may be trimmed.
        //     See: https://github.com/dotnet/runtime/issues/96528
        //   - The file name may also be unknown.
        //   - The line number is always 0.
        //     See: https://github.com/dotnet/runtime/issues/68714
        // - Even when not using AOT, it may not retrieve the expected information:
        //   - It's getting the frames at runtime and the JIT may have inlined some methods.
        //
        // We want the caller information as determined at compile time, which
        // means the caller info attributes would be better for this, however we'd have
        // to add those parameters to the caller methods (e.g.: PushError) which means
        // we can't have a `params` parameter.

        // Skip this method and its caller so the first frame is user code.
        var stackTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: true);
        var stackFrame = stackTrace.GetFrame(0);

        string? callerName = stackFrame?.GetMethod()?.Name;
        string? callerFilePath = stackFrame?.GetFileName();
        int callerLineNumber = stackFrame?.GetFileLineNumber() ?? 0;

        ReadOnlySpan<byte> messageUtf8 = Encoding.UTF8.GetBytes(message);
        ReadOnlySpan<byte> callerNameUtf8 = !string.IsNullOrEmpty(callerName)
            ? Encoding.UTF8.GetBytes(callerName)
            : default;
        ReadOnlySpan<byte> callerFilePathUtf8 = !string.IsNullOrEmpty(callerFilePath)
            ? Encoding.UTF8.GetBytes(callerFilePath)
            : default;

        fixed (byte* messagePtr = messageUtf8)
        fixed (byte* callerNamePtr = callerNameUtf8)
        fixed (byte* callerFilePathPtr = callerFilePathUtf8)
        {
            switch (type)
            {
                case ErrorHandlerType.Warning:
                    GodotBridge.GDExtensionInterface.print_warning(messagePtr, callerNamePtr, callerFilePathPtr, callerLineNumber, /* editor_notify */ false);
                    break;
                case ErrorHandlerType.Script:
                    GodotBridge.GDExtensionInterface.print_script_error(messagePtr, callerNamePtr, callerFilePathPtr, callerLineNumber, /* editor_notify */ false);
                    break;
                case ErrorHandlerType.Error:
                default:
                    GodotBridge.GDExtensionInterface.print_error(messagePtr, callerNamePtr, callerFilePathPtr, callerLineNumber, /* editor_notify */ false);
                    break;
            }
        }
    }

    /// <summary>
    /// Pushes an error message to Godot's built-in debugger and to the OS terminal.
    ///
    /// Note: Errors printed this way will not pause project execution.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PushError("test error"); // Prints "test error" to debugger and terminal as error call
    /// </code>
    /// </example>
    /// <param name="message">Error message.</param>
    public static void PushError(string message)
    {
        ErrPrintError(message, ErrorHandlerType.Error);
    }

    /// <summary>
    /// Pushes an error message to Godot's built-in debugger and to the OS terminal.
    ///
    /// Note: Errors printed this way will not pause project execution.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PushError("test_error"); // Prints "test error" to debugger and terminal as error call
    /// </code>
    /// </example>
    /// <param name="what">Arguments that form the error message.</param>
    public static void PushError(params object[] what)
    {
        ErrPrintError(AppendPrintParams(what), ErrorHandlerType.Error);
    }

    /// <summary>
    /// Pushes a warning message to Godot's built-in debugger and to the OS terminal.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PushWarning("test warning"); // Prints "test warning" to debugger and terminal as warning call
    /// </code>
    /// </example>
    /// <param name="message">Warning message.</param>
    public static void PushWarning(string message)
    {
        ErrPrintError(message, ErrorHandlerType.Warning);
    }

    /// <summary>
    /// Pushes a warning message to Godot's built-in debugger and to the OS terminal.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.PushWarning("test warning"); // Prints "test warning" to debugger and terminal as warning call
    /// </code>
    /// </example>
    /// <param name="what">Arguments that form the warning message.</param>
    public static void PushWarning(params object[] what)
    {
        ErrPrintError(AppendPrintParams(what), ErrorHandlerType.Warning);
    }

    /// <summary>
    /// Returns a random floating point value between <c>0.0</c> and <c>1.0</c> (inclusive).
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Randf(); // Returns e.g. 0.375671
    /// </code>
    /// </example>
    /// <returns>A random <see langword="float"/> number.</returns>
    public static float Randf()
    {
        return (float)UtilityFunctions.Randf();
    }

    /// <summary>
    /// Returns a normally-distributed pseudo-random floating point value
    /// using Box-Muller transform with the specified <pararmref name="mean"/>
    /// and a standard <paramref name="deviation"/>.
    /// This is also called Gaussian distribution.
    /// </summary>
    /// <returns>A random normally-distributed <see langword="float"/> number.</returns>
    public static double Randfn(double mean, double deviation)
    {
        return UtilityFunctions.Randfn(mean, deviation);
    }

    /// <summary>
    /// Returns a random unsigned 32-bit integer.
    /// Use remainder to obtain a random value in the interval <c>[0, N - 1]</c>
    /// (where N is smaller than 2^32).
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Randi();           // Returns random integer between 0 and 2^32 - 1
    /// GD.Randi() % 20;      // Returns random integer between 0 and 19
    /// GD.Randi() % 100;     // Returns random integer between 0 and 99
    /// GD.Randi() % 100 + 1; // Returns random integer between 1 and 100
    /// </code>
    /// </example>
    /// <returns>A random <see langword="uint"/> number.</returns>
    public static uint Randi()
    {
        return (uint)UtilityFunctions.Randi();
    }

    /// <summary>
    /// Randomizes the seed (or the internal state) of the random number generator.
    /// The current implementation uses a number based on the device's time.
    ///
    /// Note: This method is called automatically when the project is run.
    /// If you need to fix the seed to have consistent, reproducible results,
    /// use <see cref="Seed(ulong)"/> to initialize the random number generator.
    /// </summary>
    public static void Randomize()
    {
        UtilityFunctions.Randomize();
    }

    /// <summary>
    /// Returns a random floating point value between <paramref name="from"/>
    /// and <paramref name="to"/> (inclusive).
    /// </summary>
    /// <example>
    /// <code>
    /// GD.RandRange(0.0, 20.5);   // Returns e.g. 7.45315
    /// GD.RandRange(-10.0, 10.0); // Returns e.g. -3.844535
    /// </code>
    /// </example>
    /// <returns>A random <see langword="double"/> number inside the given range.</returns>
    public static double RandRange(double from, double to)
    {
        return UtilityFunctions.RandfRange(from, to);
    }

    /// <summary>
    /// Returns a random signed 32-bit integer between <paramref name="from"/>
    /// and <paramref name="to"/> (inclusive). If <paramref name="to"/> is lesser than
    /// <paramref name="from"/>, they are swapped.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.RandRange(0, 1);      // Returns either 0 or 1
    /// GD.RandRange(-10, 1000); // Returns random integer between -10 and 1000
    /// </code>
    /// </example>
    /// <returns>A random <see langword="int"/> number inside the given range.</returns>
    public static int RandRange(int from, int to)
    {
        return (int)UtilityFunctions.RandiRange(from, to);
    }

    /// <summary>
    /// Given a <paramref name="seed"/>, returns a randomized <see langword="uint"/>
    /// value. The <paramref name="seed"/> may be modified.
    /// Passing the same <paramref name="seed"/> consistently returns the same value.
    ///
    /// Note: "Seed" here refers to the internal state of the pseudo random number
    /// generator, currently implemented as a 64 bit integer.
    /// </summary>
    /// <example>
    /// <code>
    /// var a = GD.RandFromSeed(4);
    /// </code>
    /// </example>
    /// <param name="seed">
    /// Seed to use to generate the random number.
    /// If a different seed is used, its value will be modified.
    /// </param>
    /// <returns>A random <see langword="uint"/> number.</returns>
    public static uint RandFromSeed(ref ulong seed)
    {
        using var ret = UtilityFunctions.RandFromSeed((long)seed);
        var retSpan = ret.AsSpan();
        seed = (ulong)retSpan[1];
        return (uint)retSpan[0];
    }

    /// <summary>
    /// Returns a <see cref="IEnumerable{T}"/> that iterates from
    /// <c>0</c> (inclusive) to <paramref name="end"/> (exclusive)
    /// in steps of <c>1</c>.
    /// </summary>
    /// <param name="end">The last index.</param>
    public static IEnumerable<int> Range(int end)
    {
        return Range(0, end, 1);
    }

    /// <summary>
    /// Returns a <see cref="IEnumerable{T}"/> that iterates from
    /// <paramref name="start"/> (inclusive) to <paramref name="end"/> (exclusive)
    /// in steps of <c>1</c>.
    /// </summary>
    /// <param name="start">The first index.</param>
    /// <param name="end">The last index.</param>
    public static IEnumerable<int> Range(int start, int end)
    {
        return Range(start, end, 1);
    }

    /// <summary>
    /// Returns a <see cref="IEnumerable{T}"/> that iterates from
    /// <paramref name="start"/> (inclusive) to <paramref name="end"/> (exclusive)
    /// in steps of <paramref name="step"/>.
    /// The argument <paramref name="step"/> can be negative, but not <c>0</c>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="step"/> is 0.
    /// </exception>
    /// <param name="start">The first index.</param>
    /// <param name="end">The last index.</param>
    /// <param name="step">The amount by which to increment the index on each iteration.</param>
    public static IEnumerable<int> Range(int start, int end, int step)
    {
        ArgumentOutOfRangeException.ThrowIfZero(start);

        if (end < start && step > 0)
        {
            yield break;
        }

        if (end > start && step < 0)
        {
            yield break;
        }

        if (step > 0)
        {
            for (int i = start; i < end; i += step)
            {
                yield return i;
            }
        }
        else
        {
            for (int i = start; i > end; i += step)
            {
                yield return i;
            }
        }
    }

    /// <summary>
    /// Sets seed for the random number generator to <paramref name="seed"/>.
    /// Setting the seed manually can ensure consistent, repeatable results for
    /// most random functions.
    /// </summary>
    /// <example>
    /// <code>
    /// ulong mySeed = (ulong)GD.Hash("Godot Rocks");
    /// GD.Seed(mySeed);
    /// var a = GD.Randf() + GD.Randi();
    /// GD.Seed(mySeed);
    /// var b = GD.Randf() + GD.Randi();
    /// // a and b are now identical
    /// </code>
    /// </example>
    /// <param name="seed">Seed that will be used.</param>
    public static void Seed(ulong seed)
    {
        UtilityFunctions.Seed((long)seed);
    }

    /// <summary>
    /// Converts a formatted string that was returned by <see cref="VarToStr(Variant)"/>
    /// to the original value.
    /// </summary>
    /// <example>
    /// <code>
    /// string a = "{ \"a\": 1, \"b\": 2 }";        // a is a string
    /// var b = GD.StrToVar(a).AsGodotDictionary(); // b is a Dictionary
    /// GD.Print(b["a"]);                           // Prints 1
    /// </code>
    /// </example>
    /// <param name="str">String that will be converted to Variant.</param>
    /// <returns>The decoded <c>Variant</c>.</returns>
    public static Variant StrToVar(string str)
    {
        using NativeGodotString strNative = NativeGodotString.Create(str);
        return Variant.CreateTakingOwnership(UtilityFunctions.StrToVar(strNative));
    }

    /// <summary>
    /// Encodes a <see cref="Variant"/> value to a byte array, without encoding objects.
    /// Deserialization can be done with <see cref="BytesToVar"/>.
    /// Note: If you need object serialization, see <see cref="VarToBytesWithObjects"/>.
    /// </summary>
    /// <param name="var"><see cref="Variant"/> that will be encoded.</param>
    /// <returns>The <see cref="Variant"/> encoded as an array of bytes.</returns>
    public static byte[] VarToBytes(Variant var)
    {
        return UtilityFunctions.VarToBytes(var.NativeValue.DangerousSelfRef).AsSpan().ToArray();
    }

    /// <summary>
    /// Encodes a <see cref="Variant"/>. Encoding objects is allowed (and can potentially
    /// include executable code). Deserialization can be done with <see cref="BytesToVarWithObjects"/>.
    /// </summary>
    /// <param name="var"><see cref="Variant"/> that will be encoded.</param>
    /// <returns>The <see cref="Variant"/> encoded as an array of bytes.</returns>
    public static byte[] VarToBytesWithObjects(Variant var)
    {
        return UtilityFunctions.VarToBytesWithObjects(var.NativeValue.DangerousSelfRef).AsSpan().ToArray();
    }

    /// <summary>
    /// Converts a <see cref="Variant"/> <paramref name="var"/> to a formatted string that
    /// can later be parsed using <see cref="StrToVar(string)"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// var a = new GodotDictionary { ["a"] = 1, ["b"] = 2 };
    /// GD.Print(GD.VarToStr(a));
    /// // Prints:
    /// // {
    /// //     "a": 1,
    /// //     "b": 2
    /// // }
    /// </code>
    /// </example>
    /// <param name="var">Variant that will be converted to string.</param>
    /// <returns>The <see cref="Variant"/> encoded as a string.</returns>
    public static string VarToStr(Variant var)
    {
        return UtilityFunctions.VarToStr(var.NativeValue.DangerousSelfRef).ToString();
    }
}
