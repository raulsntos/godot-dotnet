using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Godot.BindingsGenerator.Reflection;

/// <summary>
/// Defines the body of a C# method.
/// </summary>
public abstract class MethodBody
{
    /// <summary>
    /// Cached empty <see cref="MethodBody"/> that can be used for methods with an empty body.
    /// If the method has a return type or out parameters it will assign <see langword="default"/>
    /// to them before exiting.
    /// </summary>
    public static MethodBody Empty { get; } = new EmptyMethodBody();

    /// <summary>
    /// Create a <see cref="MethodBody"/> from a delegate.
    /// If the method requires unsafe code, use
    /// <see cref="CreateUnsafe(Action{IndentedTextWriter})"/> instead.
    /// </summary>
    /// <param name="bodyWriter">
    /// The delegate that contains the implementation that writes the body.
    /// </param>
    /// <returns>The constructed <see cref="MethodBody"/>.</returns>
    public static MethodBody Create(Action<IndentedTextWriter> bodyWriter) => new ActionMethodBody(bodyWriter);

    /// <summary>
    /// Create an <b>unsafe</b> <see cref="MethodBody"/> from a delegate.
    /// If the method doesn't require unsafe code, consider using
    /// <see cref="Create(Action{IndentedTextWriter})"/> instead.
    /// </summary>
    /// <param name="bodyWriter">
    /// The delegate that contains the implementation that writes the body.
    /// </param>
    /// <returns>The constructed <see cref="MethodBody"/>.</returns>
    public static MethodBody CreateUnsafe(Action<IndentedTextWriter> bodyWriter) => new ActionMethodBody(bodyWriter, isUnsafe: true);

    /// <summary>
    /// Indicates whether the method signature should contain the <c>unsafe</c>
    /// modifier. This allows the method body to use unsafe code without having
    /// to wrap it in an unsafe block.
    /// </summary>
    public virtual bool RequiresUnsafeCode => false;

    /// <summary>
    /// Write the method's body.
    /// </summary>
    /// <param name="owner">The <see cref="MethodBase"/> to contains the body.</param>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    public abstract void Write(MethodBase owner, IndentedTextWriter writer);

    private sealed class EmptyMethodBody : MethodBody
    {
        public override void Write(MethodBase owner, IndentedTextWriter writer)
        {
            foreach (var parameter in owner.Parameters)
            {
                if (parameter.IsOut)
                {
                    writer.WriteLine($"{parameter.Name} = default;");
                }
            }

            if (owner is MethodInfo mi && mi.ReturnParameter is not null)
            {
                writer.WriteLine("return default;");
            }
        }
    }

    private sealed class ActionMethodBody : MethodBody
    {
        private Action<IndentedTextWriter> _action;

        public override bool RequiresUnsafeCode { get; }

        public ActionMethodBody(Action<IndentedTextWriter> action, bool isUnsafe = false)
        {
            _action = action;
            RequiresUnsafeCode = isUnsafe;
        }

        public override void Write(MethodBase owner, IndentedTextWriter writer)
        {
            _action(writer);
        }
    }
}
