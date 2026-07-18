using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot.Common.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGeneration;

internal static class BindMembersWriter
{
    public static void Write(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(spec.FullyQualifiedNamespace))
        {
            sb.AppendLine($"namespace {spec.FullyQualifiedNamespace};");
            sb.AppendLine();
        }

        for (int i = spec.ContainingTypeSymbols.Count - 1; i >= 0; i--)
        {
            var containingSymbol = spec.ContainingTypeSymbols[i];
            sb.Append("partial ");
            sb.Append(containingSymbol.SymbolKind switch
            {
                ContainingSymbol.Kind.Interface => "interface ",
                ContainingSymbol.Kind.Class => "class ",
                ContainingSymbol.Kind.Struct => "struct ",
                ContainingSymbol.Kind.RecordClass => "record class ",
                ContainingSymbol.Kind.RecordStruct => "record struct ",
                var kind => throw new InvalidOperationException($"Invalid symbol kind '{kind}'."),
            });
            sb.Append('@');
            sb.AppendLine(containingSymbol.SymbolName);
            sb.OpenBlock();
        }

        sb.AppendLine($"partial class @{spec.SymbolName}");
        sb.OpenBlock();

        WriteCachedStringNames(sb, spec);

        WriteSignalMembers(sb, spec);

        sb.AppendLineNoTabs("#pragma warning disable CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.");
        sb.AppendLine("internal static void BindMembers(global::Godot.Bridge.ClassRegistrationContext context)");
        sb.AppendLineNoTabs("#pragma warning restore CS0108 // Method might already be defined higher in the hierarchy, that's not an issue.");
        sb.OpenBlock();

        WriteForceCollectionMarshallingRegistration(sb, spec);

        WriteSetIcon(sb, spec);

        WriteBindConstructor(sb, spec);

        WriteBindMembers(sb, spec);

        WriteBindConstants(sb, spec);

        WriteBindProperties(sb, spec);

        WriteBindSignals(sb, spec);

        WriteSetRpcConfigs(sb, spec);

        sb.CloseBlock();

        sb.CloseBlock();

        for (int i = 0; i < spec.ContainingTypeSymbols.Count; i++)
        {
            sb.CloseBlock();
        }
    }

    private static void WriteCachedStringNames(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        string baseTypeFullName = spec.FullyQualifiedBaseTypeName;

        {
            HashSet<string> usedNames = [];

            sb.AppendLine($"public new partial class MethodName : {baseTypeFullName}.MethodName");
            sb.OpenBlock();
            foreach (var method in spec.Methods)
            {
                if (usedNames.Add(method.SymbolName))
                {
                    string value = !string.IsNullOrEmpty(method.NameOverride)
                        ? method.NameOverride!
                        : method.SymbolName;
                    AddCachedStringName(method.SymbolName, value);
                }
            }
            sb.CloseBlock();
        }

        {
            HashSet<string> usedNames = [];

            sb.AppendLine($"public new partial class ConstantName : {baseTypeFullName}.ConstantName");
            sb.OpenBlock();
            foreach (var constant in spec.Constants)
            {
                if (!string.IsNullOrEmpty(constant.EnumSymbolName))
                {
                    if (usedNames.Add(constant.EnumSymbolName!))
                    {
                        string value = !string.IsNullOrEmpty(constant.EnumNameOverride)
                            ? constant.EnumNameOverride!
                            : constant.EnumSymbolName!;
                        AddCachedStringName(constant.EnumSymbolName!, value);
                    }
                }

                {
                    string symbolName = $"{constant.EnumSymbolName}{constant.SymbolName}";
                    if (usedNames.Add(symbolName))
                    {
                        string value = !string.IsNullOrEmpty(constant.NameOverride)
                            ? constant.NameOverride!
                            : constant.SymbolName;
                        AddCachedStringName(symbolName, value);
                    }
                }
            }
            sb.CloseBlock();
        }

        {
            HashSet<string> usedNames = [];

            sb.AppendLine($"public new partial class PropertyName : {baseTypeFullName}.PropertyName");
            sb.OpenBlock();
            foreach (var property in spec.Properties)
            {
                if (usedNames.Add(property.SymbolName))
                {
                    string value = !string.IsNullOrEmpty(property.NameOverride)
                        ? property.NameOverride!
                        : property.SymbolName;
                    AddCachedStringName(property.SymbolName, value);
                }
            }
            sb.CloseBlock();
        }

        {
            HashSet<string> usedNames = [];

            sb.AppendLine($"public new partial class SignalName : {baseTypeFullName}.SignalName");
            sb.OpenBlock();
            foreach (var signal in spec.Signals)
            {
                if (usedNames.Add(signal.EventSymbolName))
                {
                    string value = !string.IsNullOrEmpty(signal.NameOverride)
                        ? signal.NameOverride!
                        : signal.EventSymbolName;
                    AddCachedStringName(signal.EventSymbolName, value);
                }
            }
            sb.CloseBlock();
        }

        void AddCachedStringName(string symbolName, string value)
        {
            if (value.IsAscii())
            {
                sb.AppendLine($$"""public static global::Godot.StringName @{{symbolName}} { get; } = global::Godot.StringName.CreateStaticFromAscii("{{value}}"u8);""");
            }
            else
            {
                sb.AppendLine($$"""public static global::Godot.StringName @{{symbolName}} { get; } = global::Godot.StringName.CreateFromUtf8("{{value}}"u8);""");
            }
        }
    }

    private static void WriteSignalMembers(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        foreach (var signal in spec.Signals)
        {
            WriteSignalEvent(sb, signal);
            WriteEmitSignalMethod(sb, signal);
        }
    }

    private static void WriteSignalEvent(IndentedStringBuilder sb, GodotSignalSpec signal)
    {
        string? accessibility = AccessibilityToKeyword(signal.SymbolDeclaredAccessibility);
        if (!string.IsNullOrEmpty(accessibility))
        {
            sb.Append($"{accessibility} ");
        }

        sb.AppendLine($"event {signal.SymbolName} @{signal.EventSymbolName}");
        sb.OpenBlock();

        sb.Append($"add => Connect(SignalName.@{signal.EventSymbolName}, ");
        AppendCallableFromDelegate(sb, signal);
        sb.AppendLine(");");

        sb.Append($"remove => Disconnect(SignalName.@{signal.EventSymbolName}, ");
        AppendCallableFromDelegate(sb, signal);
        sb.AppendLine(");");

        sb.CloseBlock();

        static void AppendCallableFromDelegate(IndentedStringBuilder sb, GodotSignalSpec signal)
        {
            sb.Append("global::Godot.Callable.From");
            if (signal.Parameters.Count != 0)
            {
                sb.Append('<');
                for (int i = 0; i < signal.Parameters.Count; i++)
                {
                    var parameter = signal.Parameters[i];
                    sb.Append(parameter.FullyQualifiedTypeName);
                    if (i < signal.Parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append('>');
            }
            sb.Append("(value.Invoke)");
        }
    }

    private static void WriteEmitSignalMethod(IndentedStringBuilder sb, GodotSignalSpec spec)
    {
        string? accessibility = AccessibilityToKeyword(spec.EmitSignalMethodAccessibility);
        if (!string.IsNullOrEmpty(accessibility))
        {
            sb.Append($"{accessibility} ");
        }

        sb.Append($"void EmitSignal{spec.EventSymbolName}(");
        for (int i = 0; i < spec.Parameters.Count; i++)
        {
            var parameter = spec.Parameters[i];
            sb.Append($"{parameter.FullyQualifiedTypeName} @{parameter.SymbolName}");
            if (parameter.HasExplicitDefaultValue)
            {
                sb.Append($" = {parameter.ExplicitDefaultValue}");
            }
            if (i < spec.Parameters.Count - 1)
            {
                sb.Append(", ");
            }
        }
        sb.AppendLine(")");
        sb.OpenBlock();

        sb.Append($"EmitSignal(SignalName.@{spec.EventSymbolName}, [");
        for (int i = 0; i < spec.Parameters.Count; i++)
        {
            var parameter = spec.Parameters[i];
            if (parameter.TypeKind == TypeKind.Enum)
            {
                // Enums need to be cast to long because they are not implicitly convertible to Variant.
                sb.Append("(long)");
            }
            sb.Append($"@{parameter.SymbolName}");
            if (i < spec.Parameters.Count - 1)
            {
                sb.Append(", ");
            }
        }
        sb.AppendLine("]);");

        sb.CloseBlock();
    }

    private static void WriteSetIcon(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        if (spec.IconPath is null)
        {
            // Class did not specify an icon.
            return;
        }

        sb.AppendLine($"""context.SetIcon("{spec.IconPath}");""");
    }

    private static void WriteBindConstructor(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        if (spec.Constructor is null)
        {
            // Class is not instantiable, don't bind a constructor.
            return;
        }

        var constructor = spec.Constructor.Value;

        if (constructor.IsConstructor)
        {
            // The spec refers to a type constructor.
            sb.AppendLine($"context.BindConstructor(() => new {constructor.FullyQualifiedBuilderTypeName}());");
        }
        else
        {
            sb.AppendLine($"context.BindConstructor(() => {constructor.FullyQualifiedBuilderTypeName}.@{constructor.MethodSymbolName}());");
        }
    }

    private static void WriteForceCollectionMarshallingRegistration(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        // GodotArray<T> and GodotDictionary<TKey, TValue> register their Variant marshalling
        // callbacks lazily, from their static constructors. Under trimming/NativeAOT nothing
        // guarantees those static constructors run before the members that use them are marshalled,
        // which would otherwise result in an 'InvalidOperationException' (marshalling not supported).
        // Force them to run here using a concrete (trim-safe) type literal, once per distinct
        // collection type used by a bound member (property, method parameter, method return,
        // or signal parameter).

        HashSet<string> seenTypeNames = [];
        List<string> orderedTypeNames = [];

        void Collect(MarshalInfo marshalInfo)
        {
            string? collectionTypeName = GetLazilyRegisteredCollectionTypeName(marshalInfo);
            if (collectionTypeName is not null && seenTypeNames.Add(collectionTypeName))
            {
                orderedTypeNames.Add(collectionTypeName);
            }
        }

        foreach (var property in spec.Properties)
        {
            Collect(property.MarshalInfo);
        }
        foreach (var method in spec.Methods)
        {
            foreach (var parameter in method.Parameters)
            {
                Collect(parameter.MarshalInfo);
            }
            if (method.ReturnParameter is not null)
            {
                Collect(method.ReturnParameter.Value.MarshalInfo);
            }
        }
        foreach (var signal in spec.Signals)
        {
            foreach (var parameter in signal.Parameters)
            {
                Collect(parameter.MarshalInfo);
            }
        }

        foreach (string collectionTypeName in orderedTypeNames)
        {
            sb.AppendLine($"global::System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof({collectionTypeName}).TypeHandle);");
        }
    }

    /// <summary>
    /// If <paramref name="marshalInfo"/> is marshalled as a generic <c>GodotArray&lt;...&gt;</c> or
    /// <c>GodotDictionary&lt;...&gt;</c> (the only collection types that register their marshalling
    /// callbacks lazily), returns a type name usable in a <c>typeof</c> expression; otherwise
    /// returns <see langword="null"/>.
    /// </summary>
    private static string? GetLazilyRegisteredCollectionTypeName(MarshalInfo marshalInfo)
    {
        if (marshalInfo.VariantType is not (VariantType.Array or VariantType.Dictionary))
        {
            // Only Array/Dictionary Variant types are marshalled as GodotArray<...>/GodotDictionary<...>.
            return null;
        }

        // The non-generic GodotArray/GodotDictionary are core Variant types that register their
        // marshalling eagerly, so only the generic variants (with type arguments) need to be forced.
        const string GodotArrayPrefix = "global::" + KnownTypeNames.GodotArray + "<";
        const string GodotDictionaryPrefix = "global::" + KnownTypeNames.GodotDictionary + "<";

        string marshalAsTypeName = marshalInfo.FullyQualifiedMarshalAsTypeName;
        if (!marshalAsTypeName.StartsWith(GodotArrayPrefix, StringComparison.Ordinal)
         && !marshalAsTypeName.StartsWith(GodotDictionaryPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        // Strip nullable reference type annotations ('?'): 'typeof' rejects them, and e.g.
        // 'GodotArray<GodotObject?>' and 'GodotArray<GodotObject>' are the same runtime type (so
        // this also dedupes them). Nullable value types (e.g. 'int?') can't be Variant-marshalable
        // collection type arguments, so removing every '?' is safe for these type names.
        return marshalAsTypeName.Replace("?", "");
    }

    private static void WriteBindMembers(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        foreach (var method in spec.Methods)
        {
            if (method.IsStatic)
            {
                Debug.Assert(!method.IsVirtual, $"Static methods can't be virtual. Method: '{method.SymbolName}'.");
            }

            sb.Append($"context.{method switch
            {
                _ when method.IsStatic => "BindStaticMethod",
                _ when method.IsVirtual => "BindVirtualMethod",
                _ => "BindMethod"
            }}");

            if (method.IsVirtual && (method.Parameters.Count != 0 || method.ReturnParameter is not null))
            {
                sb.Append('<');
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    var parameter = method.Parameters[i];
                    sb.Append(parameter.MarshalInfo.FullyQualifiedMarshalAsTypeName);
                    if (method.ReturnParameter is not null || i < method.Parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                if (method.ReturnParameter is not null)
                {
                    sb.Append(method.ReturnParameter.Value.MarshalInfo.FullyQualifiedMarshalAsTypeName);
                }
                sb.Append('>');
            }

            sb.Append($"(MethodName.@{method.SymbolName}");

            if (method.IsVirtual && method.Parameters.Count == 0 && method.ReturnParameter is null)
            {
                sb.AppendLine(");");
                continue;
            }
            else
            {
                sb.AppendLine(',');
                sb.Indent++;
            }

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                sb.AppendParameterDefinition(parameter);
                if (method.ReturnParameter is not null || i < method.Parameters.Count - 1)
                {
                    sb.AppendLine(',');
                }
            }
            if (method.ReturnParameter is not null)
            {
                sb.AppendReturnDefinition(method.ReturnParameter.Value);
            }

            if (method.IsVirtual)
            {
                sb.AppendLine(");");
                sb.Indent--;
                continue;
            }

            if (method.Parameters.Count != 0 || method.ReturnParameter is not null)
            {
                sb.AppendLine(',');
            }

            // Generate the function.

            sb.Append("static (");
            if (!method.IsStatic)
            {
                sb.Append($"{spec.SymbolName} __instance");
            }
            if (method.Parameters.Count != 0)
            {
                if (!method.IsStatic)
                {
                    sb.Append(", ");
                }
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    GodotPropertySpec parameter = method.Parameters[i];
                    sb.Append($"{parameter.MarshalInfo.FullyQualifiedMarshalAsTypeName} @{parameter.SymbolName}");
                    if (i < method.Parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
            }

            sb.AppendLine(") =>");

            sb.AppendLine("{");
            sb.Indent++;

            if (method.ReturnParameter is not null)
            {
                sb.Append("return ");
            }

            var methodInvocation = new IndentedStringBuilder();
            if (method.ExplicitInterfaceFullyQualifiedTypeName is not null)
            {
                methodInvocation.Append($"(({method.ExplicitInterfaceFullyQualifiedTypeName})");
            }
            methodInvocation.Append(method.IsStatic ? spec.SymbolName : "__instance");
            if (method.ExplicitInterfaceFullyQualifiedTypeName is not null)
            {
                methodInvocation.Append(')');
            }
            methodInvocation.Append($".@{method.SymbolName}(");
            if (method.Parameters.Count != 0)
            {
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    GodotPropertySpec parameter = method.Parameters[i];
                    AppendConvertFromGodotType(methodInvocation, parameter, $"@{parameter.SymbolName}");
                    if (i < method.Parameters.Count - 1)
                    {
                        methodInvocation.Append(", ");
                    }
                }
            }
            methodInvocation.Append(")");

            if (method.ReturnParameter is not null)
            {
                AppendConvertToGodotType(sb, method.ReturnParameter.Value, methodInvocation.ToString());
            }
            else
            {
                sb.Append(methodInvocation.ToString());
            }

            sb.AppendLine(';');

            sb.Indent--;
            sb.AppendLine("});");

            sb.Indent--;
        }
    }

    private static void WriteBindConstants(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        foreach (var constant in spec.Constants)
        {
            sb.Append("context.BindConstant(");
            sb.Indent++;

            sb.AppendConstantDefinition(constant);

            sb.AppendLine(");");

            sb.Indent--;
        }
    }

    private static void WriteBindProperties(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        foreach (var property in spec.Properties)
        {
            if (property.GroupDefinition is not null)
            {
                var groupSpec = property.GroupDefinition.Value;

                sb.Append("context.AddPropertyGroup(");
                sb.Append($"\"{groupSpec.Name}\"");
                if (!string.IsNullOrEmpty(groupSpec.Prefix))
                {
                    sb.Append($", \"{groupSpec.Prefix}\"");
                }
                sb.AppendLine(");");
            }

            if (property.SubgroupDefinition is not null)
            {
                var subgroupSpec = property.SubgroupDefinition.Value;

                sb.Append("context.AddPropertySubgroup(");
                sb.Append($"\"{subgroupSpec.Name}\"");
                if (!string.IsNullOrEmpty(subgroupSpec.Prefix))
                {
                    sb.Append($", \"{subgroupSpec.Prefix}\"");
                }
                sb.AppendLine(");");
            }

            sb.Append("context.BindProperty(");
            sb.Indent++;

            sb.AppendPropertyDefinition(property);
            sb.AppendLine(',');

            // Generate getter.
            sb.AppendLine($"static ({spec.SymbolName} __instance) =>");
            sb.AppendLine('{');
            sb.Indent++;
            sb.Append("return ");
            string instanceSource = "__instance";
            if (property.ExplicitInterfaceFullyQualifiedTypeName is not null)
            {
                instanceSource = $"(({property.ExplicitInterfaceFullyQualifiedTypeName})__instance)";
            }
            AppendConvertToGodotType(sb, property, $"{instanceSource}.@{property.SymbolName}");
            sb.AppendLine(';');
            sb.Indent--;
            sb.Append('}');
            sb.AppendLine(',');

            // Generate setter.
            sb.AppendLine($"static ({spec.SymbolName} __instance, {property.MarshalInfo.FullyQualifiedMarshalAsTypeName} value) =>");
            sb.AppendLine('{');
            sb.Indent++;
            if (property.ExplicitInterfaceFullyQualifiedTypeName is not null)
            {
                sb.Append($"(({property.ExplicitInterfaceFullyQualifiedTypeName})");
            }
            sb.Append("__instance");
            if (property.ExplicitInterfaceFullyQualifiedTypeName is not null)
            {
                sb.Append(')');
            }
            sb.Append($".@{property.SymbolName} = ");
            AppendConvertFromGodotType(sb, property, "value");
            sb.AppendLine(';');
            sb.Indent--;
            sb.Append('}');

            sb.AppendLine(");");
            sb.Indent--;
        }
    }

    private static void WriteBindSignals(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        foreach (var signal in spec.Signals)
        {
            sb.Append("context.BindSignal(new global::Godot.Bridge.SignalDefinition(");
            sb.Append($"SignalName.@{signal.EventSymbolName})");
            if (signal.Parameters.Count == 0)
            {
                sb.AppendLine(");");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine('{');
                sb.Indent++;

                sb.AppendLine("Parameters =");
                sb.AppendLine('{');
                sb.Indent++;
                foreach (var parameter in signal.Parameters)
                {
                    sb.AppendParameterDefinition(parameter);
                    sb.AppendLine(',');
                }
                sb.Indent--;
                sb.AppendLine("},");

                sb.Indent--;
                sb.AppendLine("});");
            }
        }
    }

    private static void WriteSetRpcConfigs(IndentedStringBuilder sb, GodotClassSpec spec)
    {
        foreach (var rpcMethod in spec.RpcMethods)
        {
            sb.AppendLine($"context.SetRpcConfig(MethodName.@{rpcMethod.SymbolName}, new global::Godot.NativeInterop.RpcConfig()");
            sb.AppendLine("{");
            sb.Indent++;
            sb.AppendLine($"Mode = {rpcMethod.ModeExpression},");
            sb.AppendLine($"CallLocal = {(rpcMethod.CallLocal ? "true" : "false")},");
            sb.AppendLine($"TransferMode = {rpcMethod.TransferModeExpression},");
            sb.AppendLine($"TransferChannel = {rpcMethod.TransferChannel},");
            sb.Indent--;
            sb.AppendLine("});");
        }
    }

    private static void AppendReturnDefinition(this IndentedStringBuilder sb, GodotPropertySpec returnParameter)
    {
        sb.Append("new global::Godot.Bridge.ReturnDefinition(");
        sb.Append(returnParameter.MarshalInfo.VariantType.FullNameWithGlobal());
        if (returnParameter.MarshalInfo.VariantTypeMetadata != VariantTypeMetadata.None)
        {
            sb.Append(", ");
            sb.Append(returnParameter.MarshalInfo.VariantTypeMetadata.FullNameWithGlobal());
        }
        sb.Append(')');
        AppendPropertyDefinitionObjectInitializer(sb, returnParameter);
    }

    private static void AppendParameterDefinition(this IndentedStringBuilder sb, GodotPropertySpec parameter)
    {
        string nameValue = !string.IsNullOrEmpty(parameter.NameOverride)
            ? parameter.NameOverride!
            : parameter.SymbolName;

        sb.Append("new global::Godot.Bridge.ParameterDefinition(");
        if (nameValue.IsAscii())
        {
            sb.Append($"""global::Godot.StringName.CreateStaticFromAscii("{nameValue}"u8), """);
        }
        else
        {
            sb.Append($"""global::Godot.StringName.CreateFromUtf8("{nameValue}"u8), """);
        }
        sb.Append(parameter.MarshalInfo.VariantType.FullNameWithGlobal());
        if (parameter.MarshalInfo.VariantTypeMetadata != VariantTypeMetadata.None || parameter.HasExplicitDefaultValue)
        {
            sb.Append(", ");
            sb.Append(parameter.MarshalInfo.VariantTypeMetadata.FullNameWithGlobal());
        }
        if (parameter.HasExplicitDefaultValue)
        {
            sb.Append($", {parameter.ExplicitDefaultValue}");
        }
        sb.Append(')');
        AppendPropertyDefinitionObjectInitializer(sb, parameter);
    }

    private static void AppendConstantDefinition(this IndentedStringBuilder sb, GodotConstantSpec constant)
    {
        sb.Append($"new global::Godot.Bridge.ConstantDefinition(");
        sb.Append($"ConstantName.@{constant.EnumSymbolName}{constant.SymbolName}, ");
        sb.Append("(long)(");
        if (!string.IsNullOrEmpty(constant.EnumSymbolName))
        {
            sb.Append($"@{constant.EnumSymbolName}.");
        }
        sb.Append($"@{constant.SymbolName}))");

        if (!string.IsNullOrEmpty(constant.EnumSymbolName))
        {
            sb.AppendLine();
            sb.AppendLine('{');
            sb.Indent++;

            sb.AppendLine($"""EnumName = ConstantName.@{constant.EnumSymbolName},""");
            if (constant.IsFlagsEnum)
            {
                sb.AppendLine($"""IsFlagsEnum = true,""");
            }

            sb.Indent--;
            sb.Append('}');
        }
    }

    private static void AppendPropertyDefinition(this IndentedStringBuilder sb, GodotPropertySpec property)
    {
        sb.Append("new global::Godot.Bridge.PropertyDefinition(");
        sb.Append($"PropertyName.@{property.SymbolName}, ");
        sb.Append(property.MarshalInfo.VariantType.FullNameWithGlobal());
        if (property.MarshalInfo.VariantTypeMetadata != VariantTypeMetadata.None)
        {
            sb.Append(", ");
            sb.Append(property.MarshalInfo.VariantTypeMetadata.FullNameWithGlobal());
        }
        sb.Append(')');
        AppendPropertyDefinitionObjectInitializer(sb, property);
    }

    private static void AppendPropertyDefinitionObjectInitializer(IndentedStringBuilder sb, GodotPropertySpec property)
    {
        var marshalInfo = property.MarshalInfo;

        List<string> lines = [];

        if (marshalInfo.Hint != PropertyHint.None)
        {
            lines.Add($"Hint = {marshalInfo.Hint.FullNameWithGlobal()},");
        }
        if (!string.IsNullOrEmpty(marshalInfo.HintString))
        {
            lines.Add($"""HintString = "{marshalInfo.HintString}",""");
        }
        if (marshalInfo.Usage != PropertyUsageFlags.None)
        {
            lines.Add($"Usage = {marshalInfo.Usage.FullNameWithGlobal()},");
        }
        if (!string.IsNullOrEmpty(marshalInfo.ClassName))
        {
            if (marshalInfo.ClassName!.IsAscii())
            {
                lines.Add($"""ClassName = global::Godot.StringName.CreateStaticFromAscii("{marshalInfo.ClassName}"u8),""");
            }
            else
            {
                lines.Add($"""ClassName = global::Godot.StringName.CreateFromUtf8("{marshalInfo.ClassName}"u8),""");
            }
        }

        if (lines.Count == 0)
        {
            // All the properties have the default value, no need to append anything.
            return;
        }

        sb.AppendLine();
        sb.AppendLine('{');
        sb.Indent++;

        foreach (string line in lines)
        {
            sb.AppendLine(line);
        }

        sb.Indent--;
        sb.Append('}');
    }

    private static void AppendConvertToGodotType(IndentedStringBuilder sb, GodotPropertySpec spec, string source)
    {
        if (spec.MarshalInfo.TypeIsSpeciallyRecognized)
        {
            if (spec.MarshalInfo.VariantType is VariantType.Array or VariantType.Dictionary
             || spec.MarshalInfo.VariantType.IsPackedArray())
            {
                // Specially-recognized types that are marshalled as collection types
                // should support spread expressions to convert them.
                // We also add a cast in case the compiler can't figure out the target type.
                sb.Append($"({spec.MarshalInfo.FullyQualifiedMarshalAsTypeName})");
                sb.Append('(');
                sb.Append($"[.. {source}]");
                sb.Append(')');
                return;
            }
        }
        else if (spec.MarshalInfo.UsesCustomMarshaller)
        {
            sb.Append(spec.MarshalInfo.FullyQualifiedMarshallerTypeName);
            sb.Append($".ConvertToGodotType({source})");
            return;
        }

        // Core Variant types shouldn't need to be converted.
        sb.Append(source);
    }

    private static void AppendConvertFromGodotType(IndentedStringBuilder sb, GodotPropertySpec spec, string source)
    {
        if (spec.MarshalInfo.TypeIsSpeciallyRecognized)
        {
            if (spec.MarshalInfo.VariantType is VariantType.Array or VariantType.Dictionary
             || spec.MarshalInfo.VariantType.IsPackedArray())
            {
                // Specially-recognized types that are marshalled as collection types
                // should support spread expressions to convert them.
                sb.Append($"[.. {source}]");
                return;
            }
        }
        else if (spec.MarshalInfo.UsesCustomMarshaller)
        {
            sb.Append(spec.MarshalInfo.FullyQualifiedMarshallerTypeName);
            sb.Append($".ConvertFromGodotType({source})");
            return;
        }

        // Core Variant types shouldn't need to be converted.
        sb.Append(source);
    }

    private static string? AccessibilityToKeyword(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.NotApplicable or _ => null,
        };
    }

    private static bool IsAscii(this string value)
    {
        return System.Text.Encoding.UTF8.GetByteCount(value) == value.Length;
    }
}
