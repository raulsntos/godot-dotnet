using System;
using System.CodeDom.Compiler;
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

        PtrMarshallerWriter? returnPtrMarshaller = null;
        VariantMarshallerWriter? returnVariantMarshaller = null;
        if (context.ReturnType is not null)
        {
            if (context.MarshalReturnTypeAsPtr)
            {
                var marshaller = TypeDB.GetPtrMarshaller(context.ReturnType);

                if (marshaller.NeedsCleanup)
                {
                    needsCleanup = true;
                }

                returnPtrMarshaller = marshaller;
            }
            else
            {
                var marshaller = TypeDB.GetVariantMarshaller(context.ReturnType);

                if (marshaller.NeedsCleanup)
                {
                    needsCleanup = true;
                }

                returnVariantMarshaller = marshaller;
            }
        }

        if (needsCleanup)
        {
            context.NeedsCleanup = true;
        }

        context.ParameterMarshallers = parameterMarshallers;
        context.ReturnPtrMarshaller = returnPtrMarshaller;
        context.ReturnVariantMarshaller = returnVariantMarshaller;

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
            writer.WriteLine($"{context.ReturnType.FullNameWithGlobal} {context.ReturnVariableName};");
            writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {context.ReturnVariableName});");

            if (context.MarshalReturnTypeAsPtr)
            {
                var marshaller = context.ReturnPtrMarshaller!;
                if (marshaller.WriteSetupToUnmanagedUninitialized(writer, context.ReturnType, $"{context.ReturnVariableName}Native"))
                {
                    context.ReturnTypeWithPreSetup = true;
                }

                writer.WriteLine($"{marshaller.UnmanagedPointerType.FullNameWithGlobal} {context.ReturnVariableName}Ptr = default;");
            }
            else
            {
                var marshaller = context.ReturnVariantMarshaller!;
                if (marshaller.WriteSetupToVariantUninitialized(writer, context.ReturnType, $"{context.ReturnVariableName}Var"))
                {
                    context.ReturnTypeWithPreSetup = true;
                }

                if (context.ReturnType != KnownTypes.NativeGodotVariant)
                {
                    writer.WriteLine($"global::Godot.NativeInterop.NativeGodotVariant {context.ReturnVariableName}Var;");
                    writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {context.ReturnVariableName}Var);");
                }
            }
        }

        writer.WriteLineNoTabs("");
        writer.WriteLine("// Setup - Prepare arguments and varargs.");

        writer.WriteLine($"int {context.ArgsCountVariableName} = {argsCount} + {context.VarargParameter.Name}.Length;");

        writer.WriteLine("const int VarArgsSpanThreshold = 10;");

        writer.WriteLine($"scoped global::System.Span<global::Godot.NativeInterop.NativeGodotVariant.Movable> {context.VarargParameter.Name}MovableSpan = {context.VarargParameter.Name}.Length <= VarArgsSpanThreshold");
        writer.Indent++;
        writer.WriteLine($"? stackalloc global::Godot.NativeInterop.NativeGodotVariant.Movable[VarArgsSpanThreshold]");
        writer.WriteLine($": new global::Godot.NativeInterop.NativeGodotVariant.Movable[{context.VarargParameter.Name}.Length];");
        writer.Indent--;

        writer.WriteLine($"scoped global::System.Span<nint> {context.VarargParameter.Name}PtrSpan = {context.ArgsCountVariableName} <= VarArgsSpanThreshold");
        writer.Indent++;
        writer.WriteLine($"? stackalloc nint[VarArgsSpanThreshold]");
        writer.WriteLine($": new nint[{context.ArgsCountVariableName}];");
        writer.Indent--;

        writer.WriteLine($"fixed (global::Godot.NativeInterop.NativeGodotVariant.Movable* {context.VarargParameter.Name}MovablePtr = &global::System.Runtime.InteropServices.MemoryMarshal.GetReference({context.VarargParameter.Name}MovableSpan))");
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
            writer.Write($"{context.VarargParameter.Name}MovablePtr[i] = ");
            writer.WriteLine("args[i].NativeValue;");
            writer.Write($"{context.ArgsVariableName}[{argsCount} + i] = ");
            writer.WriteLine($"{context.VarargParameter.Name}MovablePtr[i].DangerousSelfRef.GetUnsafeAddress();");
            writer.CloseBlock();
        }

        if (context.ReturnType is not null && context.MarshalReturnTypeAsPtr)
        {
            var marshaller = context.ReturnPtrMarshaller!;

            // If the marshaller initialized an aux variable, use that one instead.
            string returnName = context.ReturnTypeWithPreSetup
                ? $"{context.ReturnVariableName}Native"
                : context.ReturnVariableName;

            marshaller.WriteConvertToUnmanaged(writer, context.ReturnType, returnName, $"{context.ReturnVariableName}Ptr");
        }
    }

    protected sealed override void UnmarshalParameters(TContext context, IndentedTextWriter writer)
    {
        if (context.ReturnType is null || context.ReturnType == KnownTypes.NativeGodotVariant)
        {
            return;
        }

        if (context.MarshalReturnTypeAsPtr)
        {
            var marshaller = context.ReturnPtrMarshaller!;

            if (marshaller.UnmanagedPointerType.PointedAtType == context.ReturnType)
            {
                // When the pointed at type is the same as the return type (e.g.: 'Color*')
                // then we don't need to unmarshal because we already have the value.
                return;
            }

            marshaller.WriteConvertFromUnmanaged(writer, context.ReturnType, $"{context.ReturnVariableName}Ptr", context.ReturnVariableName);
        }
        else
        {
            var marshaller = context.ReturnVariantMarshaller!;

            marshaller.WriteConvertFromVariant(writer, context.ReturnType, $"&{context.ReturnVariableName}Var", context.ReturnVariableName);
        }
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

        if (context.ReturnType is not null)
        {
            if (context.MarshalReturnTypeAsPtr)
            {
                var marshaller = context.ReturnPtrMarshaller!;
                marshaller.WriteFree(writer, context.ReturnType, $"{context.ReturnVariableName}Ptr");
            }
            else
            {
                var marshaller = context.ReturnVariantMarshaller!;
                marshaller.WriteFree(writer, context.ReturnType, $"&{context.ReturnVariableName}Var");
            }
        }
    }

    protected sealed override void End(TContext context, IndentedTextWriter writer)
    {
        // Close fixed block opened in Setup.
        writer.CloseBlock();
    }
}
