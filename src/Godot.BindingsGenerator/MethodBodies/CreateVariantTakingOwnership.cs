using System.CodeDom.Compiler;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class CreateVariantTakingOwnership : MethodBody
{
    private readonly string _targetTypeName;

    public override bool RequiresUnsafeCode => true;

    public CreateVariantTakingOwnership(string targetTypeName)
    {
        _targetTypeName = targetTypeName;
    }

    public override void Write(MethodBase owner, IndentedTextWriter writer)
    {
        writer.WriteLine($$"""return new() { {{_targetTypeName}} = value, Type = global::Godot.VariantType.{{_targetTypeName}} };""");
    }
}
