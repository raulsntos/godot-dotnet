using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class GlobalConstantsBindingsDataCollector : BindingsDataCollector
{
    public override void Populate(BindingsData.CollectionContext context)
    {
        var globals = new TypeInfo("GD", "Godot")
        {
            VisibilityAttributes = VisibilityAttributes.Public,
            TypeAttributes = TypeAttributes.ReferenceType,
            IsStatic = true,
            IsPartial = true,
        };

        foreach (var engineConstant in context.Api.GlobalConstants)
        {
            string fieldName = NamingUtils.SnakeToPascalCase(engineConstant.Name);
            var fieldType = context.TypeDB.GetTypeFromEngineName(engineConstant.Type);
            var field = new FieldInfo(fieldName, fieldType)
            {
                VisibilityAttributes = VisibilityAttributes.Public,
            };
            globals.DeclaredFields.Add(field);
        }

        context.AddGeneratedType($"GD.GlobalConstants.cs", globals);
    }
}
