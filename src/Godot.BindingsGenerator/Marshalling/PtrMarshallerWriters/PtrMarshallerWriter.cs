using System.CodeDom.Compiler;
using System.Diagnostics;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

/// <summary>
/// Writes the marshalling code to convert between a managed type and
/// an unmanaged pointer type.
/// </summary>
internal abstract class PtrMarshallerWriter
{
    public TypeInfo UnmanagedPointerType
    {
        get
        {
            var type = GetUnmanagedPointerTypeCore();
            Debug.Assert(type.IsPointerType);
            return type;
        }
    }

    public TypeInfo MarshallableType => GetMarshallableType();

    protected abstract TypeInfo GetUnmanagedPointerTypeCore();

    protected abstract TypeInfo GetMarshallableType();

    /// <summary>
    /// Indicates whether the marshaller needs to cleanup afterwards.
    /// This is sometimes needed for marshallers that may allocate during
    /// <see cref="WriteSetupToUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>
    /// or <see cref="WriteConvertToUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>.
    /// </summary>
    public virtual bool NeedsCleanup => true;

    /// <summary>
    /// Write the necessary lines to setup the conversion to unmanaged.
    /// The marshaller may create an aux variable with the name
    /// <paramref name="destination"/> if it needs it for marshalling and returns
    /// <see langword="true"/> to indicate that this variable should be used for
    /// marshalling instead of <paramref name="source"/>.
    /// </summary>
    /// <param name="writer">Writer to write the lines to.</param>
    /// <param name="type">Type that needs to be converted.</param>
    /// <param name="source">Variable name of the source.</param>
    /// <param name="destination">Variable name of the destination.</param>
    /// <returns>Whether the <paramref name="destination"/> variable was used.</returns>
    public bool WriteSetupToUnmanaged(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        return WriteSetupToUnmanagedCore(writer, type, source, destination);
    }

    /// <inheritdoc cref="WriteSetupToUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>
    protected virtual bool WriteSetupToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        return false;
    }

    /// <summary>
    /// Write the necessary lines to setup the conversion to unmanaged.
    /// The marshaller may create an uninitialized aux variable with the name
    /// <paramref name="destination"/> if it needs it for marshalling and returns
    /// <see langword="true"/> to indicate that this variable should be used for
    /// marshalling.
    /// </summary>
    /// <param name="writer">Writer to write the lines to.</param>
    /// <param name="type">Type that needs to be converted.</param>
    /// <param name="destination">Variable name of the destination.</param>
    /// <returns>Whether the <paramref name="destination"/> variable was used.</returns>
    public bool WriteSetupToUnmanagedUninitialized(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        return WriteSetupToUnmanagedUninitializedCore(writer, type, destination);
    }

    /// <inheritdoc cref="WriteSetupToUnmanagedUninitialized(IndentedTextWriter, TypeInfo, string)"/>
    protected virtual bool WriteSetupToUnmanagedUninitializedCore(IndentedTextWriter writer, TypeInfo type, string destination)
    {
        return false;
    }

    /// <summary>
    /// Write the necessary lines to convert the value stored in a variable
    /// with the name <paramref name="source"/> and store the unmanaged pointer
    /// in a variable with the name <paramref name="destination"/>.
    /// </summary>
    /// <param name="writer">Writer to write the lines to.</param>
    /// <param name="type">Type that needs to be converted.</param>
    /// <param name="source">Variable name of the source.</param>
    /// <param name="destination">Variable name of the destination.</param>
    public void WriteConvertToUnmanaged(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        WriteConvertToUnmanagedCore(writer, type, source, destination);
    }

    /// <inheritdoc cref="WriteConvertToUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>
    protected abstract void WriteConvertToUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination);

    /// <summary>
    /// Write the necessary lines to convert the unmanaged pointer stored in a
    /// variable with the name <paramref name="source"/> and store the pointed at
    /// value in a variable with the name <paramref name="destination"/>.
    /// </summary>
    /// <param name="writer">Writer to write the lines to.</param>
    /// <param name="type">Type that needs to be converted.</param>
    /// <param name="source">Variable name of the source.</param>
    /// <param name="destination">Variable name of the destination.</param>
    public void WriteConvertFromUnmanaged(IndentedTextWriter writer, TypeInfo type, string source, string destination)
    {
        WriteConvertFromUnmanagedCore(writer, type, source, destination);
    }

    /// <inheritdoc cref="WriteConvertFromUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>
    protected abstract void WriteConvertFromUnmanagedCore(IndentedTextWriter writer, TypeInfo type, string source, string destination);

    /// <summary>
    /// Write the necessary lines to free the unmanaged pointer stored in a
    /// variable with the name <paramref name="source"/>.
    /// The pointer to free should be the one allocated by
    /// <see cref="WriteConvertToUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>.
    /// </summary>
    /// <remarks>
    /// If this writer doesn't allocate memory in
    /// <see cref="WriteConvertToUnmanaged(IndentedTextWriter, TypeInfo, string, string)"/>
    /// it doesn't have to free anything here and can be a no-op.
    /// </remarks>
    /// <param name="writer">Writer to write the lines to.</param>
    /// <param name="type">Type that was converted.</param>
    /// <param name="source">Variable name of the source.</param>
    public void WriteFree(IndentedTextWriter writer, TypeInfo type, string source)
    {
        WriteFreeCore(writer, type, source);
    }

    /// <inheritdoc cref="WriteFree(IndentedTextWriter, TypeInfo, string)"/>
    protected abstract void WriteFreeCore(IndentedTextWriter writer, TypeInfo type, string source);
}
