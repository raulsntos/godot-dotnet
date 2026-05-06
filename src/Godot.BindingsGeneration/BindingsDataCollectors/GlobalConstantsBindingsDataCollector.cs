using Godot.BindingsGeneration.Reflection;
using Godot.Common;

namespace Godot.BindingsGeneration;

internal sealed class GlobalConstantsBindingsDataCollector : BindingsDataCollector
{
    public override void Populate(BindingsData.CollectionContext context)
    {
        if (context.IsExtension)
        {
            // Global constants are only generated for the core API, not for GDExtensions.
            return;
        }

        var globals = new TypeInfo("GlobalConstants", context.Options.Namespace)
        {
            VisibilityAttributes = VisibilityAttributes.Assembly,
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
                IsLiteral = true,
                DefaultValue = engineConstant.Value,
                Documentation = context.Options.IncludeDocumentation ? engineConstant.Description : null,
            };
            globals.DeclaredFields.Add(field);
        }

        context.AddGeneratedType($"GlobalConstants.cs", globals);
    }
}
