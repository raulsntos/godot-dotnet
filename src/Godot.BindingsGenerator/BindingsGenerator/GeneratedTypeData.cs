using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

/// <summary>
/// Describes one of the generated types that will be written by <see cref="BindingsGenerator"/>
/// and the path that it should be written to.
/// </summary>
internal sealed class GeneratedTypeData
{
    /// <summary>
    /// Type information that describes the type that will be written.
    /// </summary>
    public TypeInfo Type { get; }

    /// <summary>
    /// Path hint to the file that the type will be written to.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Whether the nullable context should be enabled in the generated file.
    /// </summary>
    public bool Nullable { get; set; }

    /// <summary>
    /// Construct a <see cref="GeneratedTypeData"/>.
    /// </summary>
    /// <param name="path">Path hint to the file that the type will be written to.</param>
    /// <param name="type">Type information that describes the type that will be written.</param>
    public GeneratedTypeData(string path, TypeInfo type)
    {
        Type = type;
        Path = path;
    }

    /// <summary>
    /// Deconstructs the generated type data.
    /// </summary>
    /// <param name="type">Type information that describes the type that will be written.</param>
    /// <param name="path">Path hint to the file that the type will be written to.</param>
    public void Deconstruct(out TypeInfo type, out string path)
    {
        type = Type;
        path = Path;
    }
}
