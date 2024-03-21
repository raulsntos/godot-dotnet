using System;
using System.CodeDom.Compiler;
using System.Linq;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

/// <summary>
/// Implements the base method body for all vararg calls.
/// </summary>
/// <typeparam name="TContext">Context that contains generation information.</typeparam>
internal abstract class VarargCallMethodBody<TContext> : CallMethodBody<TContext> where TContext : VarargCallMethodBodyContext
{
    protected TypeDB TypeDB { get; }

    public sealed override bool RequiresUnsafeCode => true;

    public VarargCallMethodBody(TypeDB typeDB)
    {
        TypeDB = typeDB;
    }

    protected abstract TContext CreateVarargCallContext(MethodBase owner);

    protected sealed override TContext CreateContext(MethodBase owner)
    {
        var context = CreateVarargCallContext(owner);

        // Vararg parameters must always be the last one.
        context.VarargParameter = context.Parameters[^1];

        // Number of parameters without the vararg parameter.
        int argsCount = context.Parameters.Count - 1;

        bool needsCleanup = false;

        var parameterMarshallers = new VariantMarshallerWriter[argsCount];
        for (int i = 0; i < argsCount; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = TypeDB.GetVariantMarshaller(parameter.Type);

            if (marshaller.NeedsCleanup)
            {
                needsCleanup = true;
            }

            parameterMarshallers[i] = marshaller;
        }

        VariantMarshallerWriter? returnTypeMarshaller = null;
        if (context.ReturnType is not null)
        {
            var marshaller = TypeDB.GetVariantMarshaller(context.ReturnType);

            if (marshaller.NeedsCleanup)
            {
                needsCleanup = true;
            }

            returnTypeMarshaller = marshaller;
        }

        if (needsCleanup)
        {
            context.NeedsCleanup = true;
        }

        context.ParameterMarshallers = parameterMarshallers;
        context.ReturnTypeMarshaller = returnTypeMarshaller;

        context.ParametersWithPreSetup = new bool[argsCount];

        return context;
    }

    /// <summary>
    /// Setup instance parameter. Only executes if <see cref="CallMethodBodyContext.IsStatic"/>
    /// is <see langword="true"/>.
    /// </summary>
    protected virtual void SetupInstanceParameter(TContext context, IndentedTextWriter writer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves the method bind that will be invoked in <see cref="CallMethodBody.InvokeMethodBind"/>.
    /// </summary>
    protected abstract void RetrieveMethodBind(TContext context, IndentedTextWriter writer);

    protected sealed override void Setup(TContext context, IndentedTextWriter writer)
    {
        if (!context.IsStatic)
        {
            SetupInstanceParameter(context, writer);
        }

        RetrieveMethodBind(context, writer);

        // Number of parameters without the vararg parameter.
        int argsCount = context.Parameters.Count - 1;
        ParameterInfo[] parameters = context.Parameters.Take(argsCount).ToArray();

        for (int i = 0; i < argsCount; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = context.ParameterMarshallers[i];

            string parameterName = SourceCodeWriter.EscapeIdentifier(parameter.Name);

            if (marshaller.WriteSetupToVariant(writer, parameter.Type, parameterName, $"{parameter.Name}Native"))
            {
                context.ParametersWithPreSetup[i] = true;
            }
        }

        if (context.ReturnType is not null)
        {
            // TODO: Can't skip init because NativeGodotVariant is a ref struct.
            // Change when C# is able to use ref structs in generic type arguments.
            // See: https://github.com/dotnet/csharplang/issues/1148
            if (context.ReturnType.IsByRefLike)
            {
                writer.WriteLine($"{context.ReturnType.FullNameWithGlobal} {context.ReturnVariableName} = default;");
            }
            else
            {
                writer.WriteLine($"{context.ReturnType.FullNameWithGlobal} {context.ReturnVariableName};");
                writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {context.ReturnVariableName});");
            }

            var marshaller = context.ReturnTypeMarshaller!;
            if (marshaller.WriteSetupToVariantUninitialized(writer, context.ReturnType, $"{context.ReturnVariableName}Native"))
            {
                context.ReturnTypeWithPreSetup = true;
            }

            if (context.ReturnType != KnownTypes.NativeGodotVariant)
            {
                // TODO: Can't skip init because NativeGodotVariant is a ref struct.
                // Change when C# is able to use ref structs in generic type arguments.
                // See: https://github.com/dotnet/csharplang/issues/1148
                // writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant {context.ReturnVariableName}Var;");
                // writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {context.ReturnVariableName}Var);");
                writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant {context.ReturnVariableName}Var = default;");
            }
        }

        writer.WriteLineNoTabs("");
        writer.WriteLine("// Setup - Prepare arguments and varargs.");

        writer.WriteLine($"int {context.ArgsCountVariableName} = {argsCount} + {context.VarargParameter.Name}.Length;");

        writer.WriteLine("const int VarArgsSpanThreshold = 10;");

        writer.WriteLine($"scoped global::System.Span<nint> {context.VarargParameter.Name}PtrSpan = ({parameters.Length} + {context.VarargParameter.Name}.Length) <= VarArgsSpanThreshold");
        writer.Indent++;
        writer.WriteLine($"? stackalloc nint[VarArgsSpanThreshold]");
        writer.WriteLine($": new nint[{context.VarargParameter.Name}.Length];");
        writer.Indent--;

        writer.WriteLine($"fixed (nint* {context.VarargParameter.Name}Ptr = &global::System.Runtime.InteropServices.MemoryMarshal.GetReference({context.VarargParameter.Name}PtrSpan))");
        writer.OpenBlock();

        writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant** {context.ArgsVariableName} = (global::Godot.NativeInterop.NativeGodotVariant**){context.VarargParameter.Name}Ptr;");
    }

    protected sealed override void MarshalParameters(TContext context, IndentedTextWriter writer)
    {
        // Number of parameters without the vararg parameter.
        int argsCount = context.Parameters.Count - 1;

        for (int i = 0; i < argsCount; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = context.ParameterMarshallers[i];

            // If the marshaller initialized an aux variable, use that one instead.
            string parameterName = context.ParametersWithPreSetup[i]
                ? $"{parameter.Name}Native"
                : SourceCodeWriter.EscapeIdentifier(parameter.Name);

            marshaller.WriteConvertToVariant(writer, parameter.Type, parameterName, $"{context.ArgsVariableName}[{i}]");
        }

        // Add vararg parameter.
        {
            writer.WriteLine($"for (int i = 0; i < {context.VarargParameter.Name}.Length; i++)");
            writer.OpenBlock();
            writer.Write($"{context.ArgsVariableName}[{argsCount} + i] = ");
            writer.WriteLine("args[i].NativeValue.DangerousSelfRef.GetUnsafeAddress();");
            writer.CloseBlock();
        }
    }

    protected sealed override void UnmarshalParameters(TContext context, IndentedTextWriter writer)
    {
        if (context.ReturnType is null || context.ReturnType == KnownTypes.NativeGodotVariant)
        {
            return;
        }

        var marshaller = context.ReturnTypeMarshaller!;

        marshaller.WriteConvertFromVariant(writer, context.ReturnType, $"&{context.ReturnVariableName}Var", context.ReturnVariableName);
    }

    protected override void Return(TContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"return {context.ReturnVariableName};");
    }

    protected override void Cleanup(TContext context, IndentedTextWriter writer)
    {
        // Number of parameters without the vararg parameter.
        int argsCount = context.Parameters.Count - 1;

        for (int i = 0; i < argsCount; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = context.ParameterMarshallers[i];

            marshaller.WriteFree(writer, parameter.Type, $"{context.ArgsVariableName}[{i}]");
        }

        if (context.ReturnType is not null || context.ReturnType == KnownTypes.NativeGodotVariant)
        {
            var marshaller = context.ReturnTypeMarshaller!;
            marshaller.WriteFree(writer, context.ReturnType, $"&{context.ReturnVariableName}Var");
        }
    }

    protected sealed override void End(TContext context, IndentedTextWriter writer)
    {
        // Close fixed block opened in Setup.
        writer.CloseBlock();
    }
}
