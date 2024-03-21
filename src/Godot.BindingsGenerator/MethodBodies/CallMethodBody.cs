using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal abstract class CallMethodBody<TContext> : MethodBody where TContext : CallMethodBodyContext
{
    public sealed override void Write(MethodBase owner, IndentedTextWriter writer)
    {
        var context = CreateContext(owner);

        writer.WriteLine("// Setup - Perform required setup.");
        Setup(context, writer);
        writer.WriteLineNoTabs("");

        if (context.NeedsCleanup)
        {
            writer.WriteLine("try");
            writer.OpenBlock();
        }

        writer.WriteLine("// Marshalling - Convert managed data to native data.");
        MarshalParameters(context, writer);
        writer.WriteLineNoTabs("");

        writer.WriteLine("// Calling the method.");
        InvokeMethodBind(context, writer);

        if (context.ReturnType is not null)
        {
            writer.WriteLineNoTabs("");
            writer.WriteLine("// Unmarshalling - Convert native data to managed data.");
            UnmarshalParameters(context, writer);

            Return(context, writer);
        }

        if (context.NeedsCleanup)
        {
            writer.CloseBlock();
            writer.WriteLine("finally");
            writer.OpenBlock();

            writer.WriteLine("// Cleanup allocated resources.");
            Cleanup(context, writer);

            writer.CloseBlock();
        }

        End(context, writer);
    }

    /// <summary>
    /// Creates the context that will contain the necessary information to
    /// perform the following steps.
    /// </summary>
    protected abstract TContext CreateContext(MethodBase owner);

    /// <summary>
    /// Invokes the method.
    /// </summary>
    protected abstract void InvokeMethodBind(TContext context, IndentedTextWriter writer);

    /// <summary>
    /// Setup instance parameter, method parameters, and return parameters.
    /// Creates the local variables needed to marshal and invoke the method later,
    /// and assigns the default values to parameters if needed.
    /// </summary>
    protected abstract void Setup(TContext context, IndentedTextWriter writer);

    /// <summary>
    /// Marshalls method parameters and return parameters to prepare them
    /// to invoke the method later.
    /// </summary>
    protected abstract void MarshalParameters(TContext context, IndentedTextWriter writer);

    /// <summary>
    /// Unmarshalls the return parameters to prepare them to return them later.
    /// </summary>
    protected abstract void UnmarshalParameters(TContext context, IndentedTextWriter writer);

    /// <summary>
    /// Returns the unmarshalled value. Only executes if <see cref="TContext.ReturnType"/>
    /// is not <see langword="null"/>.
    /// </summary>
    protected abstract void Return(TContext context, IndentedTextWriter writer);

    /// <summary>
    /// Performs cleanup of the allocated memory, and other local variables that
    /// were created during <see cref="Setup(TContext, IndentedTextWriter)"/>.
    /// </summary>
    protected abstract void Cleanup(TContext context, IndentedTextWriter writer);

    /// <summary>
    /// End of the method. Perform any remaining closing operation that needs to
    /// always execute.
    /// </summary>
    protected virtual void End(TContext context, IndentedTextWriter writer) { }
}
