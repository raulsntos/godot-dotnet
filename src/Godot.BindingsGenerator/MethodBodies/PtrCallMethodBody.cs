using System;
using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

/// <summary>
/// Implements the base method body for all ptrcalls.
/// </summary>
/// <typeparam name="TContext">Context that contains generation information.</typeparam>
internal abstract class PtrCallMethodBody<TContext> : CallMethodBody<TContext> where TContext : PtrCallMethodBodyContext
{
    protected TypeDB TypeDB { get; }

    public sealed override bool RequiresUnsafeCode => true;

    public PtrCallMethodBody(TypeDB typeDB)
    {
        TypeDB = typeDB;
    }

    protected abstract TContext CreatePtrCallContext(MethodBase owner);

    protected sealed override TContext CreateContext(MethodBase owner)
    {
        var context = CreatePtrCallContext(owner);

        bool needsCleanup = false;

        var parameterMarshallers = new PtrMarshallerWriter[context.Parameters.Count];
        for (int i = 0; i < context.Parameters.Count; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = TypeDB.GetPtrMarshaller(parameter.Type);

            if (marshaller.NeedsCleanup)
            {
                needsCleanup = true;
            }

            parameterMarshallers[i] = marshaller;
        }

        PtrMarshallerWriter? returnTypeMarshaller = null;
        if (context.ReturnType is not null)
        {
            var marshaller = TypeDB.GetPtrMarshaller(context.ReturnType);

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

        context.ParametersWithPreSetup = new bool[context.Parameters.Count];

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

        for (int i = 0; i < context.Parameters.Count; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = context.ParameterMarshallers[i];

            string parameterName = SourceCodeWriter.EscapeIdentifier(parameter.Name);

            if (marshaller.WriteSetupToUnmanaged(writer, parameter.Type, parameterName, $"{parameter.Name}Native"))
            {
                context.ParametersWithPreSetup[i] = true;
            }
        }

        if (context.Parameters.Count > 0)
        {
            writer.WriteLine($"void** {context.ArgsVariableName} = stackalloc void*[{context.Parameters.Count}];");
        }

        if (context.ReturnType is not null)
        {
            writer.WriteLine($"{context.ReturnType.FullNameWithGlobal} {context.ReturnVariableName};");
            writer.WriteLine($"global::System.Runtime.CompilerServices.Unsafe.SkipInit(out {context.ReturnVariableName});");

            var marshaller = context.ReturnTypeMarshaller!;
            if (marshaller.WriteSetupToUnmanagedUninitialized(writer, context.ReturnType, $"{context.ReturnVariableName}Native"))
            {
                context.ReturnTypeWithPreSetup = true;
            }

            writer.WriteLine($"{marshaller.UnmanagedPointerType.FullNameWithGlobal} {context.ReturnVariableName}Ptr = default;");
        }
    }

    protected sealed override void MarshalParameters(TContext context, IndentedTextWriter writer)
    {
        for (int i = 0; i < context.Parameters.Count; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = context.ParameterMarshallers[i];

            // If the marshaller initialized an aux variable, use that one instead.
            string parameterName = context.ParametersWithPreSetup[i]
                ? $"{parameter.Name}Native"
                : SourceCodeWriter.EscapeIdentifier(parameter.Name);

            marshaller.WriteConvertToUnmanaged(writer, parameter.Type, parameterName, $"{context.ArgsVariableName}[{i}]");
        }

        if (context.ReturnType is not null)
        {
            var marshaller = context.ReturnTypeMarshaller!;

            // If the marshaller initialized an aux variable, use that one instead.
            string returnName = context.ReturnTypeWithPreSetup
                ? $"{context.ReturnVariableName}Native"
                : context.ReturnVariableName;

            marshaller.WriteConvertToUnmanaged(writer, context.ReturnType, returnName, $"{context.ReturnVariableName}Ptr");
        }
    }

    protected sealed override void UnmarshalParameters(TContext context, IndentedTextWriter writer)
    {
        if (context.ReturnType is null)
        {
            return;
        }

        var marshaller = context.ReturnTypeMarshaller!;

        if (marshaller.UnmanagedPointerType.PointedAtType == context.ReturnType)
        {
            // When the pointed at type is the same as the return type (e.g.: 'Color*')
            // then we don't need to unmarshal because we already have the value.
            return;
        }

        marshaller.WriteConvertFromUnmanaged(writer, context.ReturnType, $"{context.ReturnVariableName}Ptr", context.ReturnVariableName);
    }

    protected override void Return(TContext context, IndentedTextWriter writer)
    {
        writer.WriteLine($"return {context.ReturnVariableName};");
    }

    protected override void Cleanup(TContext context, IndentedTextWriter writer)
    {
        for (int i = 0; i < context.Parameters.Count; i++)
        {
            var parameter = context.Parameters[i];
            var marshaller = context.ParameterMarshallers[i];

            // Pointer args always need to be casted because they come from a 'void**' variable.
            string source = $"({marshaller.UnmanagedPointerType.FullNameWithGlobal}){context.ArgsVariableName}[{i}]";

            marshaller.WriteFree(writer, parameter.Type, source);
        }

        if (context.ReturnType is not null)
        {
            var marshaller = context.ReturnTypeMarshaller!;
            marshaller.WriteFree(writer, context.ReturnType, $"{context.ReturnVariableName}Ptr");
        }
    }

    protected sealed override void End(TContext context, IndentedTextWriter writer) { }
}
