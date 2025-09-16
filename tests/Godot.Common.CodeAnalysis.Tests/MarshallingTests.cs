using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Godot.Common.CodeAnalysis.Tests;

public class MarshallingTests
{
    [Theory]
    [InlineData(KnownTypeNames.SystemDecimal)]
    [InlineData(KnownTypeNames.SystemIntPtr)]
    [InlineData(KnownTypeNames.SystemUIntPtr)]
    internal async Task TryGetMarshallingInformationWithInvalidTypes(string typeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        bool result = Marshalling.TryGetMarshallingInformation(compilation, typeSymbol, out var info);
        Assert.False(result);
    }

    [Theory]
    [InlineData(KnownTypeNames.SystemBoolean, VariantType.Bool, VariantTypeMetadata.None, "bool")]
    [InlineData(KnownTypeNames.SystemByte, VariantType.Int, VariantTypeMetadata.Byte, "byte")]
    [InlineData(KnownTypeNames.SystemSByte, VariantType.Int, VariantTypeMetadata.SByte, "sbyte")]
    [InlineData(KnownTypeNames.SystemChar, VariantType.Int, VariantTypeMetadata.Char16, "char")]
    [InlineData(KnownTypeNames.SystemTextRune, VariantType.Int, VariantTypeMetadata.Char32, $"global::{KnownTypeNames.SystemTextRune}")]
    [InlineData(KnownTypeNames.SystemInt16, VariantType.Int, VariantTypeMetadata.Int16, "short")]
    [InlineData(KnownTypeNames.SystemInt32, VariantType.Int, VariantTypeMetadata.Int32, "int")]
    [InlineData(KnownTypeNames.SystemInt64, VariantType.Int, VariantTypeMetadata.Int64, "long")]
    [InlineData(KnownTypeNames.SystemUInt16, VariantType.Int, VariantTypeMetadata.UInt16, "ushort")]
    [InlineData(KnownTypeNames.SystemUInt32, VariantType.Int, VariantTypeMetadata.UInt32, "uint")]
    [InlineData(KnownTypeNames.SystemUInt64, VariantType.Int, VariantTypeMetadata.UInt64, "ulong")]
    [InlineData(KnownTypeNames.SystemHalf, VariantType.Float, VariantTypeMetadata.None, $"global::{KnownTypeNames.SystemHalf}")]
    [InlineData(KnownTypeNames.SystemSingle, VariantType.Float, VariantTypeMetadata.Single, "float")]
    [InlineData(KnownTypeNames.SystemDouble, VariantType.Float, VariantTypeMetadata.Double, "double")]
    [InlineData(KnownTypeNames.SystemString, VariantType.String, VariantTypeMetadata.None, "string")]
    internal async Task TryGetMarshallingInformationWithPrimitiveTypes(string typeName, VariantType expectedVariantType, VariantTypeMetadata expectedVariantTypeMetadata, string? expectedTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        bool result = Marshalling.TryGetMarshallingInformation(compilation, typeSymbol, out var info);
        Assert.True(result);
        Assert.Equal(expectedVariantType, info.VariantType);
        Assert.Equal(expectedTypeName, info.FullyQualifiedTypeName);
        Assert.Equal(expectedVariantTypeMetadata, info.VariantTypeMetadata);
    }

    [Theory]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemInt32, VariantType.PackedInt32Array, VariantTypeMetadata.None, "global::System.Collections.Generic.List<int>")]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemBoolean, VariantType.Array, VariantTypeMetadata.None, "global::System.Collections.Generic.List<bool>")]
    [InlineData("System.Collections.Generic.Dictionary`2", KnownTypeNames.SystemInt32, VariantType.Dictionary, VariantTypeMetadata.None, "global::System.Collections.Generic.Dictionary<int, int>")]
    internal async Task TryGetMarshallingInformationWithSpeciallyRecognizedTypes(string typeName, string elementTypeName, VariantType expectedVariantType, VariantTypeMetadata expectedVariantTypeMetadata, string? expectedTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);

        int genericTypeArgumentSeparatorIndex = typeName.IndexOf('`', StringComparison.Ordinal);
        Assert.NotEqual(-1, genericTypeArgumentSeparatorIndex);
        var genericTypeArgumentCountTextSpan = typeName.AsSpan(genericTypeArgumentSeparatorIndex + 1);
        int genericTypeArgumentCount = int.Parse(genericTypeArgumentCountTextSpan, CultureInfo.InvariantCulture);

        var elementTypeSymbols = Enumerable.Repeat(elementTypeSymbol, genericTypeArgumentCount);
        typeSymbol = typeSymbol.Construct([.. elementTypeSymbols]);

        bool result = Marshalling.TryGetMarshallingInformation(compilation, typeSymbol, out var info);
        Assert.True(result);
        Assert.Equal(expectedVariantType, info.VariantType);
        Assert.Equal(expectedTypeName, info.FullyQualifiedTypeName);
        Assert.Equal(expectedVariantTypeMetadata, info.VariantTypeMetadata);
    }

    [Theory]
    [InlineData("System.Object")]
    public async Task TryGetPackedArrayTypeWithInvalidTypes(string typeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        bool result = Marshalling.TryGetArrayLikeElementType(compilation, typeSymbol, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("System.Boolean")]
    [InlineData("System.Char")]
    [InlineData("System.SByte")]
    [InlineData("System.Int16")]
    [InlineData("System.UInt16")]
    [InlineData("System.UInt32")]
    [InlineData("System.UInt64")]
    [InlineData("System.IntPtr")]
    [InlineData("System.Object")]
    [InlineData("Godot.GodotObject")]
    [InlineData("Godot.Node")]
    [InlineData("Godot.Resource")]
    [InlineData("Godot.Variant")]
    [InlineData("System.DayOfWeek")]
    public async Task TryGetPackedArrayTypeWithInvalidArrayTypes(string elementTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);
        var typeSymbol = compilation.CreateArrayTypeSymbol(elementTypeSymbol);

        bool result = Marshalling.TryGetPackedArrayType(typeSymbol, typeSymbol.ElementType, out var packedType, out string? marshalAsType);
        Assert.False(result);
    }

    [Theory]
    [InlineData("System.Byte", VariantType.PackedByteArray, KnownTypeNames.GodotPackedByteArray)]
    [InlineData("System.Int32", VariantType.PackedInt32Array, KnownTypeNames.GodotPackedInt32Array)]
    [InlineData("System.Int64", VariantType.PackedInt64Array, KnownTypeNames.GodotPackedInt64Array)]
    [InlineData("System.Single", VariantType.PackedFloat32Array, KnownTypeNames.GodotPackedFloat32Array)]
    [InlineData("System.Double", VariantType.PackedFloat64Array, KnownTypeNames.GodotPackedFloat64Array)]
    [InlineData("System.String", VariantType.PackedStringArray, KnownTypeNames.GodotPackedStringArray)]
    [InlineData("Godot.Vector2", VariantType.PackedVector2Array, KnownTypeNames.GodotPackedVector2Array)]
    [InlineData("Godot.Vector3", VariantType.PackedVector3Array, KnownTypeNames.GodotPackedVector3Array)]
    [InlineData("Godot.Color", VariantType.PackedColorArray, KnownTypeNames.GodotPackedColorArray)]
    [InlineData("Godot.Vector4", VariantType.PackedVector4Array, KnownTypeNames.GodotPackedVector4Array)]
    internal async Task TryGetPackedArrayTypeWithArrays(string elementTypeName, VariantType expectedVariantType, string expectedTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);
        var typeSymbol = compilation.CreateArrayTypeSymbol(elementTypeSymbol);

        bool result = Marshalling.TryGetPackedArrayType(typeSymbol, typeSymbol.ElementType, out var packedType, out string? marshalAsType);
        Assert.True(result);
        Assert.Equal(expectedVariantType, packedType);
        Assert.Equal(expectedTypeName, marshalAsType);
    }

    [Theory]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemBoolean)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemChar)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemSByte)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemInt16)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemUInt16)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemUInt32)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemUInt64)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemIntPtr)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotNode)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotResource)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotVariant)]
    [InlineData("System.Collections.Generic.List`1", "System.Object")]
    [InlineData("System.Collections.Generic.List`1", "Godot.GodotObject")]
    [InlineData("System.Collections.Generic.List`1", "System.DayOfWeek")]
    internal async Task TryGetPackedArrayTypeWithInvalidGenericCollections(string typeName, string elementTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);
        Assert.True(typeSymbol.IsGenericType);

        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);

        typeSymbol = typeSymbol.Construct([elementTypeSymbol]);

        bool result = Marshalling.TryGetPackedArrayType(typeSymbol, elementTypeSymbol, out var packedType, out string? marshalAsType);
        Assert.False(result);
    }

    [Theory]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemByte, VariantType.PackedByteArray, KnownTypeNames.GodotPackedByteArray)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemInt32, VariantType.PackedInt32Array, KnownTypeNames.GodotPackedInt32Array)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemInt64, VariantType.PackedInt64Array, KnownTypeNames.GodotPackedInt64Array)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemSingle, VariantType.PackedFloat32Array, KnownTypeNames.GodotPackedFloat32Array)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemDouble, VariantType.PackedFloat64Array, KnownTypeNames.GodotPackedFloat64Array)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemString, VariantType.PackedStringArray, KnownTypeNames.GodotPackedStringArray)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotVector2, VariantType.PackedVector2Array, KnownTypeNames.GodotPackedVector2Array)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotVector3, VariantType.PackedVector3Array, KnownTypeNames.GodotPackedVector3Array)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotColor, VariantType.PackedColorArray, KnownTypeNames.GodotPackedColorArray)]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotVector4, VariantType.PackedVector4Array, KnownTypeNames.GodotPackedVector4Array)]
    internal async Task TryGetPackedArrayTypeWithGenericCollections(string typeName, string elementTypeName, VariantType expectedVariantType, string expectedTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);
        Assert.True(typeSymbol.IsGenericType);

        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);

        typeSymbol = typeSymbol.Construct([elementTypeSymbol]);

        bool result = Marshalling.TryGetPackedArrayType(typeSymbol, elementTypeSymbol, out var packedType, out string? marshalAsType);
        Assert.True(result);
        Assert.Equal(expectedVariantType, packedType);
        Assert.Equal(expectedTypeName, marshalAsType);
    }

    [Theory]
    [InlineData("System.Object")]
    public async Task TryGetArrayLikeElementTypeWithInvalidTypes(string typeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        bool result = Marshalling.TryGetArrayLikeElementType(compilation, typeSymbol, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData("System.Object", "object")]
    [InlineData(KnownTypeNames.SystemInt32, "int")]
    [InlineData(KnownTypeNames.SystemString, "string")]
    [InlineData(KnownTypeNames.GodotObject, KnownTypeNames.GodotObject)]
    public async Task TryGetArrayLikeElementTypeWithArrays(string elementTypeName, string expectedElementTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);
        var typeSymbol = compilation.CreateArrayTypeSymbol(elementTypeSymbol);

        bool result = Marshalling.TryGetArrayLikeElementType(compilation, typeSymbol, out var elementType);
        Assert.True(result);
        Assert.NotNull(elementType);
        Assert.Equal(expectedElementTypeName, elementType.FullQualifiedNameOmitGlobal());
    }

    [Theory]
    [InlineData("System.Collections.Generic.List`1", "System.Object", "object")]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemInt32, "int")]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.SystemString, "string")]
    [InlineData("System.Collections.Generic.List`1", KnownTypeNames.GodotObject, KnownTypeNames.GodotObject)]
    public async Task TryGetArrayLikeElementTypeWithGenericCollections(string typeName, string elementTypeName, string expectedElementTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);
        Assert.True(typeSymbol.IsGenericType);

        var elementTypeSymbol = compilation.GetTypesByMetadataName(elementTypeName).FirstOrDefault();
        Assert.NotNull(elementTypeSymbol);

        typeSymbol = typeSymbol.Construct([elementTypeSymbol]);

        bool result = Marshalling.TryGetArrayLikeElementType(compilation, typeSymbol, out var retrievedElementTypeSymbol);
        Assert.True(result);
        Assert.NotNull(retrievedElementTypeSymbol);
        Assert.Equal(elementTypeSymbol, retrievedElementTypeSymbol);
        Assert.Equal(expectedElementTypeName, retrievedElementTypeSymbol.FullQualifiedNameOmitGlobal());
    }

    [Theory]
    [InlineData("System.Object")]
    public async Task TryGetDictionaryLikeKeyValueTypesWithInvalidTypes(string typeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        bool result = Marshalling.TryGetDictionaryLikeKeyValueTypes(compilation, typeSymbol, out var keyType, out var valueType);
        Assert.False(result);
    }

    [Theory]
    [InlineData("System.Collections.Generic.Dictionary`2", KnownTypeNames.SystemInt32, KnownTypeNames.SystemString, "int", "string")]
    [InlineData("System.Collections.Generic.Dictionary`2", KnownTypeNames.SystemString, KnownTypeNames.GodotObject, "string", KnownTypeNames.GodotObject)]
    public async Task TryGetDictionaryLikeKeyValueTypesWithGenericCollections(string typeName, string keyTypeName, string valueTypeName, string expectedKeyTypeName, string expectedValueTypeName)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);
        Assert.True(typeSymbol.IsGenericType);

        var keyTypeSymbol = compilation.GetTypesByMetadataName(keyTypeName).FirstOrDefault();
        Assert.NotNull(keyTypeSymbol);
        var valueTypeSymbol = compilation.GetTypesByMetadataName(valueTypeName).FirstOrDefault();
        Assert.NotNull(valueTypeSymbol);

        typeSymbol = typeSymbol.Construct([keyTypeSymbol, valueTypeSymbol]);

        bool result = Marshalling.TryGetDictionaryLikeKeyValueTypes(compilation, typeSymbol, out var retrievedKeyTypeSymbol, out var retrievedValueTypeSymbol);
        Assert.True(result);
        Assert.NotNull(retrievedKeyTypeSymbol);
        Assert.NotNull(retrievedValueTypeSymbol);
        Assert.Equal(keyTypeSymbol, retrievedKeyTypeSymbol);
        Assert.Equal(valueTypeSymbol, retrievedValueTypeSymbol);
        Assert.Equal(expectedKeyTypeName, retrievedKeyTypeSymbol.FullQualifiedNameOmitGlobal());
        Assert.Equal(expectedValueTypeName, retrievedValueTypeSymbol.FullQualifiedNameOmitGlobal());
    }

    [Theory]
    [InlineData("System.Object", VariantType.Nil, PropertyHint.None, null)]
    [InlineData(KnownTypeNames.GodotVariant, VariantType.Nil, PropertyHint.None, null)]
    [InlineData(KnownTypeNames.GodotObject, VariantType.Nil, PropertyHint.None, null)]
    [InlineData(KnownTypeNames.GodotVector2, VariantType.Nil, PropertyHint.None, null)]
    [InlineData("System.DayOfWeek", VariantType.Int, PropertyHint.Enum, "Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday")]
    [InlineData("System.DayOfWeek", VariantType.String, PropertyHint.Enum, "Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday")]
    [InlineData("System.DayOfWeek", VariantType.StringName, PropertyHint.Enum, "Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday")]
    [InlineData("System.IO.FileAccess", VariantType.Int, PropertyHint.Flags, "Read:1,Write:2,ReadWrite:3")]
    [InlineData("System.IO.FileAccess", VariantType.String, PropertyHint.Flags, "Read:1,Write:2,ReadWrite:3")]
    [InlineData("System.IO.FileAccess", VariantType.StringName, PropertyHint.Flags, "Read:1,Write:2,ReadWrite:3")]
    [InlineData(KnownTypeNames.GodotNode, VariantType.Object, PropertyHint.NodeType, "Node")]
    [InlineData(KnownTypeNames.GodotResource, VariantType.Object, PropertyHint.ResourceType, "Resource")]
    [InlineData("Godot.PackedScene", VariantType.Object, PropertyHint.ResourceType, "PackedScene")]
    [InlineData(KnownTypeNames.GodotPackedByteArray, VariantType.Array, PropertyHint.TypeString, "2/0:")]
    [InlineData(KnownTypeNames.GodotPackedInt32Array, VariantType.Array, PropertyHint.TypeString, "2/0:")]
    [InlineData(KnownTypeNames.GodotPackedInt64Array, VariantType.Array, PropertyHint.TypeString, "2/0:")]
    [InlineData(KnownTypeNames.GodotPackedFloat32Array, VariantType.Array, PropertyHint.TypeString, "3/0:")]
    [InlineData(KnownTypeNames.GodotPackedFloat64Array, VariantType.Array, PropertyHint.TypeString, "3/0:")]
    [InlineData(KnownTypeNames.GodotPackedStringArray, VariantType.Array, PropertyHint.TypeString, "4/0:")]
    [InlineData(KnownTypeNames.GodotPackedVector2Array, VariantType.Array, PropertyHint.TypeString, "5/0:")]
    [InlineData(KnownTypeNames.GodotPackedVector3Array, VariantType.Array, PropertyHint.TypeString, "9/0:")]
    [InlineData(KnownTypeNames.GodotPackedColorArray, VariantType.Array, PropertyHint.TypeString, "20/0:")]
    [InlineData(KnownTypeNames.GodotPackedVector4Array, VariantType.Array, PropertyHint.TypeString, "12/0:")]
    internal async Task TryGetDefaultPropertyHintReturnsExpectedHint(string typeName, VariantType variantType, PropertyHint expectedPropertyHint, string? expectedHintString)
    {
        var compilation = await CreateCompilationAsync();
        var typeSymbol = compilation.GetTypesByMetadataName(typeName).FirstOrDefault();
        Assert.NotNull(typeSymbol);

        bool result = Marshalling.TryGetDefaultPropertyHint(compilation, typeSymbol, variantType, out var hint, out string? hintString);
        Assert.True(result);
        Assert.Equal(expectedPropertyHint, hint);
        Assert.Equal(expectedHintString ?? "", hintString ?? "");
    }

    private static async Task<CSharpCompilation> CreateCompilationAsync(CancellationToken cancellationToken = default)
    {
        var referenceAssemblies = ReferenceAssemblies.Net.Net90;
        var references = await referenceAssemblies.ResolveAsync(LanguageNames.CSharp, cancellationToken);

        references = references.Add(MetadataReference.CreateFromFile(typeof(GodotObject).Assembly.Location));

        return CSharpCompilation.Create(
            "Dummy",
            [],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }
}
