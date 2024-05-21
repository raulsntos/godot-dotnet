using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.BindingsGenerator;

internal static class SourceCodeWriter
{
    /// <summary>
    /// Write a curly bracket to open a block and increase the indentation.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    public static void OpenBlock(this IndentedTextWriter writer)
    {
        writer.WriteLine('{');
        writer.Indent++;
    }

    /// <summary>
    /// Write a curly bracket to close a block and decrease the indentation.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    public static void CloseBlock(this IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine('}');
    }

    /// <summary>
    /// Write the member's Comment.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="member">The <see cref="MemberInfo"/> to get the comment from.</param>
    public static void WriteXMLComment(this IndentedTextWriter writer, MemberInfo member)
    {
        WriteXMLComment(writer, member?.XMLComment);
    }

    /// <summary>
    /// Write the member's Comment.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="comment">The to get the comment from.</param>
    public static void WriteXMLComment(this IndentedTextWriter writer, string? comment)
    {
        if (comment is null)
        {
            return;
        }
        foreach (var line in comment.Split('\n'))
        {
            writer.WriteLine("/// " + line);
        }
    }

    /// <summary>
    /// Write the member's attributes.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="member">The <see cref="MemberInfo"/> to get the attributes from.</param>
    public static void WriteAttributes(this IndentedTextWriter writer, MemberInfo member)
    {
        if (member is EnumInfo { HasFlagsAttribute: true })
        {
            writer.WriteLine("[global::System.Flags]");
        }

        foreach (var attribute in member.Attributes)
        {
            writer.WriteLine(attribute);
        }
    }

    /// <summary>
    /// Write the type's declaration without the contained members.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="type">The <see cref="TypeInfo"/> to get the declaration from.</param>
    public static void WriteTypeDeclaration(this IndentedTextWriter writer, TypeInfo type)
    {
        string? visibility = type.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        if (type.IsStatic)
        {
            writer.Write("static ");
        }

        if (type.IsNew)
        {
            writer.Write("new ");
        }

        if (type.IsFinal)
        {
            writer.Write("sealed ");
        }

        if (type.IsAbstract)
        {
            writer.Write("abstract ");
        }

        if (type.IsByRefLike)
        {
            writer.Write("ref ");
        }

        if (type.IsPartial)
        {
            writer.Write("partial ");
        }

        if (type.IsReferenceType)
        {
            writer.Write("class ");
        }

        if (type.IsValueType)
        {
            if (type.IsEnum)
            {
                writer.Write("enum ");
            }
            else
            {
                writer.Write("struct ");
            }
        }

        writer.Write(type.Name);

        if (type.IsGenericType)
        {
            Debug.Assert(type.IsGenericTypeDefinition, "Can't generate a constructed generic type.");
            if (type.GenericTypeArgumentCount == 1)
            {
                writer.Write("<T>");
            }
            else
            {
                writer.Write('<');
                for (int i = 0; i < type.GenericTypeArgumentCount; i++)
                {
                    writer.Write($"T{i}");
                    if (i < type.ImplementedInterfaces.Count - 1)
                    {
                        writer.Write(", ");
                    }
                }
                writer.Write('>');
            }
        }

        if (type is EnumInfo enumType)
        {
            var underlyingType = enumType.UnderlyingType;
            if (underlyingType is not null)
            {
                writer.Write($" : {underlyingType.FullNameWithGlobal}");
            }
        }
        else if (type.IsReferenceType)
        {
            if (type.BaseType is not null)
            {
                writer.Write($" : {type.BaseType.FullNameWithGlobal}");
            }
        }

        if (type.ImplementedInterfaces.Count > 0)
        {
            if (type.BaseType is not null)
            {
                writer.Write(", ");
            }
            else
            {
                writer.Write(" : ");
            }

            for (int i = 0; i < type.ImplementedInterfaces.Count; i++)
            {
                var @interface = type.ImplementedInterfaces[i];
                writer.Write(@interface.Name);
                if (i < type.ImplementedInterfaces.Count - 1)
                {
                    writer.Write(", ");
                }
            }
        }
    }

    /// <summary>
    /// Write the delegate's declaration without a closing semi-colon.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="delegate">The <see cref="DelegateInfo"/> to get the declaration from.</param>
    public static void WriteDelegateDeclaration(this IndentedTextWriter writer, DelegateInfo @delegate)
    {
        string? visibility = @delegate.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        writer.Write("delegate ");

        if (@delegate.ReturnParameter is not null)
        {
            writer.Write(@delegate.ReturnParameter.Type.FullNameWithGlobal);
            writer.Write(' ');
        }
        else
        {
            writer.Write("void ");
        }

        writer.Write(@delegate.Name);

        writer.Write('(');
        writer.WriteParameters(@delegate.Parameters);
        writer.Write(')');
    }

    /// <summary>
    /// Write the event's declaration without the add/remove accessors or a closing semi-colon.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="event">The <see cref="EventInfo"/> to get the declaration from.</param>
    public static void WriteEventDeclaration(this IndentedTextWriter writer, EventInfo @event)
    {
        string? visibility = @event.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        if ((@event.AddAccessor?.Body.RequiresUnsafeCode ?? false)
        || (@event.RemoveAccessor?.Body.RequiresUnsafeCode ?? false))
        {
            writer.Write("unsafe ");
        }

        if (@event.IsStatic)
        {
            writer.Write("static ");
        }

        writer.Write("event ");

        writer.Write($"{@event.EventHandlerType.FullNameWithGlobal} {@event.Name}");
    }

    /// <summary>
    /// Write the field's declaration without a closing semi-colon.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="method">The <see cref="FieldInfo"/> to get the declaration from.</param>
    public static void WriteFieldDeclaration(this IndentedTextWriter writer, FieldInfo field)
    {
#if DEBUG
        if (field.IsLiteral)
        {
            Debug.Assert(!field.IsStatic && !field.IsInitOnly, $"Constant field '{field.Name}' can't be marked static or read-only.");
        }
#endif

        string? visibility = field.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        if (field.RequiresUnsafeCode)
        {
            writer.Write("unsafe ");
        }

        if (field.IsStatic)
        {
            writer.Write("static ");
        }

        if (field.IsNew)
        {
            writer.Write("new ");
        }

        if (field.IsInitOnly)
        {
            writer.Write("readonly ");
        }

        if (field.IsLiteral)
        {
            writer.Write("const ");
        }

        writer.Write($"{field.Type.FullNameWithGlobal} {EscapeIdentifier(field.Name)}");

        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            writer.Write($" = {field.DefaultValue}");
        }
    }

    /// <summary>
    /// Write the property's declaration without the get/set accessors or a closing semi-colon.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="method">The <see cref="PropertyInfo"/> to get the declaration from.</param>
    public static void WritePropertyDeclaration(this IndentedTextWriter writer, PropertyInfo property)
    {
        string? visibility = property.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        if ((property.CanRead && property.Getter.Body.RequiresUnsafeCode)
        || (property.CanWrite && property.Setter.Body.RequiresUnsafeCode))
        {
            writer.Write("unsafe ");
        }

        if (property.IsStatic)
        {
            writer.Write("static ");
        }

        if (property.IsNew)
        {
            writer.Write("new ");
        }

        if (property.IsReadOnly)
        {
            writer.Write("readonly ");
        }

        if (property.CanRead && property.Getter.ReturnParameter!.IsRef)
        {
            // Property returns by-ref.
            writer.Write("ref ");
        }

        writer.Write($"{property.Type.FullNameWithGlobal} {EscapeIdentifier(property.Name)}");
    }

    /// <summary>
    /// Write the constructor's signature.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="method">The <see cref="ConstructorInfo"/> to get the signature from.</param>
    public static void WriteConstructorSignature(this IndentedTextWriter writer, ConstructorInfo constructor, TypeInfo containingType)
    {
        string? visibility = constructor.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        writer.Write(containingType.Name);

        writer.Write('(');
        writer.WriteParameters(constructor.Parameters);
        writer.Write(')');

        if (!string.IsNullOrEmpty(constructor.Initializer))
        {
            writer.Write($" : {constructor.Initializer}");
        }
    }

    /// <summary>
    /// Write the method's signature.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="method">The <see cref="MethodInfo"/> to get the signature from.</param>
    public static void WriteMethodSignature(this IndentedTextWriter writer, MethodInfo method)
    {
        string? visibility = method.GetVisibilityModifierString();
        if (!string.IsNullOrEmpty(visibility))
        {
            writer.Write(visibility);
            writer.Write(' ');
        }

        if (method.Body.RequiresUnsafeCode || (method.ReturnType?.IsPointerType ?? false) || method.Parameters.Any(p => p.Type.IsPointerType))
        {
            writer.Write("unsafe ");
        }

        if (method.IsStatic)
        {
            writer.Write("static ");
        }

        if (method.IsOverridden)
        {
            if (method.IsFinal)
            {
                writer.Write("sealed ");
            }
            else
            {
                writer.Write("override ");
            }
        }
        else
        {
            if (method.IsVirtual)
            {
                writer.Write("virtual ");
            }

            if (method.IsAbstract)
            {
                writer.Write("abstract ");
            }
        }

        if (method.IsNew)
        {
            writer.Write("new ");
        }

        if (method.IsReadOnly)
        {
            writer.Write("readonly ");
        }

        if (method.IsPartial)
        {
            writer.Write("partial ");
        }

        if (method.ReturnParameter is not null)
        {
            writer.Write(method.ReturnParameter.Type.FullNameWithGlobal);
            writer.Write(' ');
        }
        else
        {
            writer.Write("void ");
        }

        writer.Write(method.Name);

        if (method.IsGenericMethod)
        {
            writer.Write('<');
            for (int i = 0; i < method.TypeParameters.Count; i++)
            {
                var typeParameter = method.TypeParameters[i];

                foreach (var attribute in typeParameter.Attributes)
                {
                    writer.Write(attribute);
                    writer.Write(' ');
                }

                writer.Write(typeParameter.Name);
                if (i < method.TypeParameters.Count - 1)
                {
                    writer.Write(", ");
                }
            }
            writer.Write('>');
        }

        writer.Write('(');
        writer.WriteParameters(method.Parameters);
        writer.Write(')');


        foreach (var typeParameter in method.TypeParameters)
        {
            if (typeParameter.ConstraintKind is TypeParameterConstraintKind.None
             && typeParameter.ConstraintTypes.Count == 0)
            {
                // No constraints to specify on this type parameter.
                continue;
            }

            writer.Write($" where {typeParameter.Name} : ");

            if (typeParameter.ConstraintKind is not TypeParameterConstraintKind.None)
            {
                if (typeParameter.HasConstructorConstraint)
                {
                    writer.Write("new()");
                }
                if (typeParameter.HasNotNullConstraint)
                {
                    writer.Write("notnull");
                }
                if (typeParameter.HasReferenceTypeConstraint)
                {
                    writer.Write("class");
                }
                if (typeParameter.HasValueTypeTypeConstraint)
                {
                    writer.Write("struct");
                }
                if (typeParameter.HasUnmanagedTypeConstraint)
                {
                    writer.Write("unmanaged");
                }

                if (typeParameter.ConstraintTypes.Count > 0)
                {
                    writer.Write(", ");
                }
            }

            // Class constraints must be specified before every other constraint type.
            var orderedConstraintTypes = typeParameter.ConstraintTypes
                .OrderBy(type => type.IsReferenceType);

            int i = 0;
            foreach (var type in orderedConstraintTypes)
            {
                writer.Write(type.FullNameWithGlobal);

                if (i < typeParameter.ConstraintTypes.Count - 1)
                {
                    writer.Write(", ");
                }

                i++;
            }
        }
    }

    /// <summary>
    /// Write the method's body.
    /// </summary>
    /// <param name="writer">The <see cref="IndentedTextWriter"/> to write to.</param>
    /// <param name="method">The <see cref="MethodBase"/> to get the body contents from.</param>
    public static void WriteMethodBody(this IndentedTextWriter writer, MethodBase method)
    {
        method.Body.Write(method, writer);
    }

    private static void WriteParameters(this IndentedTextWriter writer, List<ParameterInfo> parameters)
    {
        for (int i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            writer.WriteParameter(parameter);
            if (i < parameters.Count - 1)
            {
                writer.Write(", ");
            }
        }
    }

    private static void WriteParameter(this IndentedTextWriter writer, ParameterInfo parameter)
    {
        foreach (var attribute in parameter.Attributes)
        {
            writer.Write(attribute);
            writer.Write(' ');
        }

        if (parameter.IsParams)
        {
            writer.Write("params ");
        }

        if (parameter.IsScoped)
        {
            writer.Write("scoped ");
        }

        string? access = parameter.GetParameterAccessModifierString();
        if (!string.IsNullOrEmpty(access))
        {
            writer.Write(access);
            writer.Write(' ');
        }

        writer.Write($"{parameter.Type.FullNameWithGlobal} {EscapeIdentifier(parameter.Name)}");

        if (!string.IsNullOrEmpty(parameter.DefaultValue))
        {
            writer.Write(" = ");
            if (parameter.Type.IsEnum)
            {
                // Enum default values may be integers so they need to be converted to the enum type.
                writer.Write($"({parameter.Type.FullNameWithGlobal})({parameter.DefaultValue})");
            }
            else
            {
                writer.Write(parameter.DefaultValue);
            }
        }
    }

    public static void WriteDefaultParameterValues(this IndentedTextWriter writer, IList<ParameterInfo> parameters, IList<GodotArgumentInfo> engineArguments, TypeDB typeDB)
    {
        Debug.Assert(parameters.Count == engineArguments.Count);

        for (int i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var engineArgument = engineArguments[i];

            if (string.IsNullOrEmpty(engineArgument.DefaultValue))
            {
                // This parameter does not have a default value.
                continue;
            }

            string defaultValueExpression = typeDB.GetDefaultValueExpression(parameter.Type, engineArgument.DefaultValue);

            if (defaultValueExpression == "default")
            {
                // If the default value is `default`, then we don't need to reassign the parameter
                // because this is the value it should already have if it wasn't assigned.
                continue;
            }

            // Types that can be constant will have their value assigned in the parameter,
            // so we only need to re-assign to parameters when their type can't be constant.
            if (!TypeDB.CanTypeBeConstant(parameter.Type))
            {
                string escapedParameterName = EscapeIdentifier(parameter.Name);

                if (parameter.Type.IsReferenceType || parameter.Type.GenericTypeDefinition == KnownTypes.Nullable)
                {
                    // The `??=` syntax is only supported for reference types and Nullable<T>.
                    writer.WriteLine($"{escapedParameterName} ??= {defaultValueExpression};");
                }
                else if (parameter.Type == KnownTypes.GodotVariant)
                {
                    // For Variant use more explicit syntax.
                    writer.WriteLine($"if ({escapedParameterName}.VariantType == global::Godot.VariantType.Nil)");
                    writer.OpenBlock();
                    writer.WriteLine($"{escapedParameterName} = {defaultValueExpression};");
                    writer.CloseBlock();
                }
                else
                {
                    // Assume that the type defines an `IsAllocated` property
                    // that can be used to check if it's the default value.
                    writer.WriteLine($"if (!{escapedParameterName}.IsAllocated)");
                    writer.OpenBlock();
                    writer.WriteLine($"{escapedParameterName} = {defaultValueExpression};");
                    writer.CloseBlock();
                }
            }
        }
    }

    /// <summary>
    /// Get the member's visibility modifier as a string.
    /// </summary>
    private static string? GetVisibilityModifierString(this VisibleMemberInfo member)
    {
        if (member.IsPublic)
        {
            return "public";
        }

        if (member.IsPrivate)
        {
            return "private";
        }

        if (member.IsFamily)
        {
            return "protected";
        }

        if (member.IsAssembly)
        {
            return "internal";
        }

        if (member.IsFamilyOrAssembly)
        {
            return "protected internal";
        }

        if (member.IsFamilyAndAssembly)
        {
            return "private protected";
        }

        return null;
    }

    /// <summary>
    /// Get the parameter's access modifier as a string.
    /// </summary>
    private static string? GetParameterAccessModifierString(this ParameterInfo parameter)
    {
        if (parameter.IsRef)
        {
            if (parameter.IsRefReadOnly)
            {
                return "ref readonly";
            }
            else
            {
                return "ref";
            }
        }

        if (parameter.IsIn)
        {
            return "in";
        }

        if (parameter.IsOut)
        {
            return "out";
        }

        return null;
    }

    public static string EscapeIdentifier(string identifier) =>
        IsCSharpKeyword(identifier) ? $"@{identifier}" : identifier;

    private static bool IsCSharpKeyword(string value) =>
        value is "abstract" or "as" or "base" or "bool"
            or "break" or "byte" or "case" or "catch"
            or "char" or "checked" or "class" or "const"
            or "continue" or "decimal" or "default" or "delegate"
            or "do" or "double" or "else" or "enum"
            or "event" or "explicit" or "extern" or "false"
            or "finally" or "fixed" or "float" or "for"
            or "foreach" or "goto" or "if" or "implicit"
            or "in" or "int" or "interface" or "internal"
            or "is" or "lock" or "long" or "namespace"
            or "new" or "null" or "object" or "operator"
            or "out" or "override" or "params" or "private"
            or "protected" or "public" or "readonly" or "ref"
            or "return" or "sbyte" or "sealed" or "short"
            or "sizeof" or "stackalloc" or "static" or "string"
            or "struct" or "switch" or "this" or "throw"
            or "true" or "try" or "typeof" or "uint" or "ulong"
            or "unchecked" or "unsafe" or "ushort" or "using"
            or "virtual" or "volatile" or "void" or "while";
}
