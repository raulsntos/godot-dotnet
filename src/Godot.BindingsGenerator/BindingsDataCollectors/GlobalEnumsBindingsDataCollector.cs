using System;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal sealed class GlobalEnumsBindingsDataCollector : BindingsDataCollector
{
    public override void Initialize(BindingsData.CollectionContext context)
    {
        foreach (var globalEnum in context.Api.GlobalEnums)
        {
            var @enum = new EnumInfo(NamingUtils.PascalToPascalCase(globalEnum.Name), "Godot")
            {
                VisibilityAttributes = VisibilityAttributes.Public,
                HasFlagsAttribute = globalEnum.IsBitField,
                UnderlyingType = KnownTypes.SystemInt64,
            };

            context.AddGeneratedType($"GlobalEnums/{@enum.Name}.cs", @enum);
            context.TypeDB.RegisterTypeName(globalEnum.Name, @enum);
        }
    }

    public override void Populate(BindingsData.CollectionContext context)
    {
        foreach (var globalEnum in context.Api.GlobalEnums)
        {
            var type = context.TypeDB.GetTypeFromEngineName(globalEnum.Name);

            if (type is not EnumInfo enumType)
            {
                throw new InvalidOperationException($"Type found for '{globalEnum.Name}' is not an enum.");
            }

            foreach (var (name, value) in globalEnum.Values)
            {
                enumType.Values.Add((NamingUtils.SnakeToPascalCase(name), value));
            }

            int enumPrefix = NamingUtils.DetermineEnumPrefix(globalEnum);

            // HARDCODED: The Error enum have the prefix 'ERR_' for everything except 'OK' and 'FAILED'.
            if (type.ContainingType is null && type.Name == "Error")
            {
                if (enumPrefix > 0)
                {
                    // Just in case it ever changes.
                    throw new InvalidOperationException($"Prefix for enum 'Error' is not empty.");
                }

                enumPrefix = 1; // 'ERR_'
            }

            NamingUtils.ApplyPrefixToEnumConstants(globalEnum, enumType, enumPrefix);
            NamingUtils.RemoveMaxConstant(globalEnum, enumType);
        }
    }
}
