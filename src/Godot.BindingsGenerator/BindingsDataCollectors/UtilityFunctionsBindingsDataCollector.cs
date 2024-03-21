using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class UtilityFunctionsBindingsDataCollector : BindingsDataCollector
{
    public override void Populate(BindingsData.CollectionContext context)
    {
        var utilityFunctionsType = new TypeInfo("UtilityFunctions", "Godot")
        {
            VisibilityAttributes = VisibilityAttributes.Assembly,
            TypeAttributes = TypeAttributes.ReferenceType,
            IsStatic = true,
            IsPartial = true,
        };

        foreach (var engineMethod in context.Api.UtilityFunctions)
        {
            var method = new MethodInfo(NamingUtils.SnakeToPascalCase(engineMethod.Name))
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                IsStatic = true,
            };

            if (!string.IsNullOrEmpty(engineMethod.ReturnType))
            {
                var returnType = context.TypeDB.GetTypeFromEngineName(engineMethod.ReturnType);
                var returnTypeUnmanaged = context.TypeDB.GetUnmanagedType(returnType);
                method.ReturnParameter = ReturnInfo.FromType(returnTypeUnmanaged);
            }

            method.Body = new CallUtilityFunction(method, engineMethod, context.TypeDB);

            foreach (var arg in engineMethod.Arguments)
            {
                string argName = NamingUtils.SnakeToCamelCase(arg.Name);
                var argType = context.TypeDB.GetTypeFromEngineName(arg.Type, arg.Meta);
                var argTypeUnmanaged = context.TypeDB.GetUnmanagedType(argType);
                var parameter = new ParameterInfo(argName, argTypeUnmanaged);
                context.ApplyDefaultValue(arg, parameter);
                method.Parameters.Add(parameter);
            }

            utilityFunctionsType.DeclaredMethods.Add(method);

            utilityFunctionsType.DeclaredFields.Add(new FieldInfo($"_{method.Name}_MethodBind", new TypeInfo("delegate* unmanaged[Cdecl]<void*, void**, int, void>"))
            {
                VisibilityAttributes = VisibilityAttributes.Private,
                IsStatic = true,
                RequiresUnsafeCode = true,
            });
        }

        context.AddGeneratedType($"{utilityFunctionsType.Name}.cs", utilityFunctionsType);
    }
}
