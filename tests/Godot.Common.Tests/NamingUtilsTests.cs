using System.Collections.Generic;
using Godot.BindingsGenerator.ApiDump;
using Godot.BindingsGenerator.Reflection;

namespace Godot.Common.Tests;

public class NamingUtilsTests
{
    [Theory]
    [InlineData("oneword", "Oneword")]
    [InlineData("two_words", "TwoWords")]
    [InlineData("three_separate_words", "ThreeSeparateWords")]
    [InlineData("ONEWORD", "Oneword")]
    [InlineData("TWO_WORDS", "TwoWords")]
    [InlineData("THREE_SEPARATE_WORDS", "ThreeSeparateWords")]
    public void SnakeToPascalCase(string value, string expected)
    {
        string actual = NamingUtils.SnakeToPascalCase(value);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("oneword", "oneword")]
    [InlineData("two_words", "twoWords")]
    [InlineData("three_separate_words", "threeSeparateWords")]
    [InlineData("ONEWORD", "oneword")]
    [InlineData("TWO_WORDS", "twoWords")]
    [InlineData("THREE_SEPARATE_WORDS", "threeSeparateWords")]
    public void SnakeToCamelCase(string value, string expected)
    {
        string actual = NamingUtils.SnakeToCamelCase(value);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Oneword", "Oneword")]
    [InlineData("TwoWords", "TwoWords")]
    [InlineData("ThreeSeparateWords", "ThreeSeparateWords")]
    [InlineData("AABB", "Aabb")]
    [InlineData("AESContext", "AesContext")]
    [InlineData("AStar3D", "AStar3D")]
    [InlineData("AudioEffectEQ21", "AudioEffectEQ21")]
    [InlineData("AudioStreamWAV", "AudioStreamWav")]
    [InlineData("CharFXTransform", "CharFXTransform")]
    [InlineData("CPUParticles3D", "CpuParticles3D")]
    [InlineData("EditorSceneImporterGLTF", "EditorSceneImporterGltf")]
    [InlineData("GIProbe", "GIProbe")]
    [InlineData("HMACContext", "HmacContext")]
    [InlineData("HSeparator", "HSeparator")]
    [InlineData("IP", "IP")]
    [InlineData("JNISingleton", "JniSingleton")]
    [InlineData("JSON", "Json")]
    [InlineData("JSONParseResult", "JsonParseResult")]
    [InlineData("JSONRPC", "JsonRpc")]
    [InlineData("NetworkedMultiplayerENet", "NetworkedMultiplayerENet")]
    [InlineData("ObjectID", "ObjectId")]
    [InlineData("OpenXRAPIExtension", "OpenXRApiExtension")]
    [InlineData("OpenXRIPBinding", "OpenXRIPBinding")]
    [InlineData("PackedFloat32Array", "PackedFloat32Array")]
    [InlineData("PCKPacker", "PckPacker")]
    [InlineData("PHashTranslation", "PHashTranslation")]
    [InlineData("PhysicsServer2DExtensionRayResult", "PhysicsServer2DExtensionRayResult")]
    [InlineData("Rect2", "Rect2")]
    [InlineData("Rect2i", "Rect2I")]
    [InlineData("RID", "Rid")]
    [InlineData("StreamPeerSSL", "StreamPeerSsl")]
    [InlineData("Transform3D", "Transform3D")]
    [InlineData("ViewportScreenSpaceAA", "ViewportScreenSpaceAA")]
    [InlineData("ViewportSDFScale", "ViewportSdfScale")]
    [InlineData("WebRTCPeerConnectionGDNative", "WebRtcPeerConnectionGDNative")]
    [InlineData("X509Certificate", "X509Certificate")]
    [InlineData("XRServer", "XRServer")]
    [InlineData("YSort", "YSort")]
    public void PascalToPascalCase(string value, string expected)
    {
        string actual = NamingUtils.PascalToPascalCase(value);
        Assert.Equal(expected, actual);
    }

    public static TheoryData<GodotEnumInfo, EnumInfo> GetEnumTestData() => new()
    {
        {
            new GodotEnumInfo()
            {
                Name = "UniformType",
                Values =
                [
                    new() { Name = "UNIFORM_TYPE_SAMPLER", Value = 0 },
                    new() { Name = "UNIFORM_TYPE_SAMPLER_WITH_TEXTURE", Value = 1 },
                    new() { Name = "UNIFORM_TYPE_TEXTURE", Value = 2 },
                    new() { Name = "UNIFORM_TYPE_IMAGE", Value = 3 },
                    new() { Name = "UNIFORM_TYPE_TEXTURE_BUFFER", Value = 4 },
                    new() { Name = "UNIFORM_TYPE_SAMPLER_WITH_TEXTURE_BUFFER", Value = 5 },
                    new() { Name = "UNIFORM_TYPE_IMAGE_BUFFER", Value = 6 },
                    new() { Name = "UNIFORM_TYPE_UNIFORM_BUFFER", Value = 7 },
                    new() { Name = "UNIFORM_TYPE_STORAGE_BUFFER", Value = 8 },
                    new() { Name = "UNIFORM_TYPE_INPUT_ATTACHMENT", Value = 9 },
                    new() { Name = "UNIFORM_TYPE_MAX", Value = 10 },
                ],
            },
            new EnumInfo("UniformType")
            {
                Values =
                [
                    ("Sampler", 0),
                    ("SamplerWithTexture", 1),
                    ("Texture", 2),
                    ("Image", 3),
                    ("TextureBuffer", 4),
                    ("SamplerWithTextureBuffer", 5),
                    ("ImageBuffer", 6),
                    ("UniformBuffer", 7),
                    ("StorageBuffer", 8),
                    ("InputAttachment", 9),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "ParticlesCollisionHeightfieldResolution",
                Values =
                [
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_256", Value = 0 },
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_512", Value = 1 },
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_1024", Value = 2 },
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_2048", Value = 3 },
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_4096", Value = 4 },
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_8192", Value = 5 },
                    new() { Name = "PARTICLES_COLLISION_HEIGHTFIELD_RESOLUTION_MAX", Value = 6 },
                ],
            },
            new EnumInfo("ParticlesCollisionHeightfieldResolution")
            {
                Values =
                [
                    ("Resolution256", 0),
                    ("Resolution512", 1),
                    ("Resolution1024", 2),
                    ("Resolution2048", 3),
                    ("Resolution4096", 4),
                    ("Resolution8192", 5),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "ShaderStage",
                Values =
                [
                    new() { Name = "SHADER_STAGE_VERTEX", Value = 0 },
                    new() { Name = "SHADER_STAGE_FRAGMENT", Value = 1 },
                    new() { Name = "SHADER_STAGE_TESSELATION_CONTROL", Value = 2 },
                    new() { Name = "SHADER_STAGE_TESSELATION_EVALUATION", Value = 3 },
                    new() { Name = "SHADER_STAGE_COMPUTE", Value = 4 },
                    new() { Name = "SHADER_STAGE_MAX", Value = 5 },
                    new() { Name = "SHADER_STAGE_VERTEX_BIT", Value = 1 },
                    new() { Name = "SHADER_STAGE_FRAGMENT_BIT", Value = 2 },
                    new() { Name = "SHADER_STAGE_TESSELATION_CONTROL_BIT", Value = 4 },
                    new() { Name = "SHADER_STAGE_TESSELATION_EVALUATION_BIT", Value = 8 },
                    new() { Name = "SHADER_STAGE_COMPUTE_BIT", Value = 16 },
                ],
            },
            new EnumInfo("ShaderStage")
            {
                Values =
                [
                    ("Vertex", 0),
                    ("Fragment", 1),
                    ("TesselationControl", 2),
                    ("TesselationEvaluation", 3),
                    ("Compute", 4),
                    ("VertexBit", 1),
                    ("FragmentBit", 2),
                    ("TesselationControlBit", 4),
                    ("TesselationEvaluationBit", 8),
                    ("ComputeBit", 16),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "StorageBufferUsage",
                Values =
                [
                    new() { Name = "STORAGE_BUFFER_USAGE_DISPATCH_INDIRECT", Value = 1 },
                ],
            },
            new EnumInfo("StorageBufferUsage")
            {
                Values =
                [
                    ("DispatchIndirect", 1),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "SaverFlags",
                Values =
                [
                    new() { Name = "FLAG_NONE", Value = 0 },
                    new() { Name = "FLAG_RELATIVE_PATHS", Value = 1 },
                    new() { Name = "FLAG_BUNDLE_RESOURCES", Value = 2 },
                    new() { Name = "FLAG_CHANGE_PATH", Value = 4 },
                    new() { Name = "FLAG_OMIT_EDITOR_PROPERTIES", Value = 8 },
                    new() { Name = "FLAG_SAVE_BIG_ENDIAN", Value = 16 },
                    new() { Name = "FLAG_COMPRESS", Value = 32 },
                    new() { Name = "FLAG_REPLACE_SUBRESOURCE_PATHS", Value = 64 },
                ],
            },
            new EnumInfo("SaverFlags")
            {
                Values =
                [
                    ("None", 0),
                    ("RelativePaths", 1),
                    ("BundleResources", 2),
                    ("ChangePath", 4),
                    ("OmitEditorProperties", 8),
                    ("SaveBigEndian", 16),
                    ("Compress", 32),
                    ("ReplaceSubresourcePaths", 64),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "MethodFlags",
                Values =
                [
                    new() { Name = "METHOD_FLAG_NORMAL", Value = 1 },
                    new() { Name = "METHOD_FLAG_EDITOR", Value = 2 },
                    new() { Name = "METHOD_FLAG_CONST", Value = 4 },
                    new() { Name = "METHOD_FLAG_VIRTUAL", Value = 8 },
                    new() { Name = "METHOD_FLAG_VARARG", Value = 16 },
                    new() { Name = "METHOD_FLAG_STATIC", Value = 32 },
                    new() { Name = "METHOD_FLAG_OBJECT_CORE", Value = 64 },
                    new() { Name = "METHOD_FLAGS_DEFAULT", Value = 1 },
                ],
            },
            new EnumInfo("MethodFlags")
            {
                Values =
                [
                    ("Normal", 1),
                    ("Editor", 2),
                    ("Const", 4),
                    ("Virtual", 8),
                    ("Vararg", 16),
                    ("Static", 32),
                    ("ObjectCore", 64),
                    ("Default", 1),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "Variant.Type",
                Values =
                [
                    new() { Name = "TYPE_NIL", Value = 0 },
                    new() { Name = "TYPE_BOOL", Value = 1 },
                    new() { Name = "TYPE_INT", Value = 2 },
                    new() { Name = "TYPE_FLOAT", Value = 3 },
                    new() { Name = "TYPE_STRING", Value = 4 },
                    new() { Name = "TYPE_VECTOR2", Value = 5 },
                    new() { Name = "TYPE_VECTOR2I", Value = 6 },
                    new() { Name = "TYPE_RECT2", Value = 7 },
                    new() { Name = "TYPE_RECT2I", Value = 8 },
                    new() { Name = "TYPE_VECTOR3", Value = 9 },
                    new() { Name = "TYPE_VECTOR3I", Value = 10 },
                    new() { Name = "TYPE_TRANSFORM2D", Value = 11 },
                    new() { Name = "TYPE_VECTOR4", Value = 12 },
                    new() { Name = "TYPE_VECTOR4I", Value = 13 },
                    new() { Name = "TYPE_PLANE", Value = 14 },
                    new() { Name = "TYPE_QUATERNION", Value = 15 },
                    new() { Name = "TYPE_AABB", Value = 16 },
                    new() { Name = "TYPE_BASIS", Value = 17 },
                    new() { Name = "TYPE_TRANSFORM3D", Value = 18 },
                    new() { Name = "TYPE_PROJECTION", Value = 19 },
                    new() { Name = "TYPE_COLOR", Value = 20 },
                    new() { Name = "TYPE_STRING_NAME", Value = 21 },
                    new() { Name = "TYPE_NODE_PATH", Value = 22 },
                    new() { Name = "TYPE_RID", Value = 23 },
                    new() { Name = "TYPE_OBJECT", Value = 24 },
                    new() { Name = "TYPE_CALLABLE", Value = 25 },
                    new() { Name = "TYPE_SIGNAL", Value = 26 },
                    new() { Name = "TYPE_DICTIONARY", Value = 27 },
                    new() { Name = "TYPE_ARRAY", Value = 28 },
                    new() { Name = "TYPE_PACKED_BYTE_ARRAY", Value = 29 },
                    new() { Name = "TYPE_PACKED_INT32_ARRAY", Value = 30 },
                    new() { Name = "TYPE_PACKED_INT64_ARRAY", Value = 31 },
                    new() { Name = "TYPE_PACKED_FLOAT32_ARRAY", Value = 32 },
                    new() { Name = "TYPE_PACKED_FLOAT64_ARRAY", Value = 33 },
                    new() { Name = "TYPE_PACKED_STRING_ARRAY", Value = 34 },
                    new() { Name = "TYPE_PACKED_VECTOR2_ARRAY", Value = 35 },
                    new() { Name = "TYPE_PACKED_VECTOR3_ARRAY", Value = 36 },
                    new() { Name = "TYPE_PACKED_COLOR_ARRAY", Value = 37 },
                    new() { Name = "TYPE_MAX", Value = 38 },
                ],
            },
            new EnumInfo("VariantType")
            {
                Values =
                [
                    ("Nil", 0),
                    ("Bool", 1),
                    ("Int", 2),
                    ("Float", 3),
                    ("String", 4),
                    ("Vector2", 5),
                    ("Vector2I", 6),
                    ("Rect2", 7),
                    ("Rect2I", 8),
                    ("Vector3", 9),
                    ("Vector3I", 10),
                    ("Transform2D", 11),
                    ("Vector4", 12),
                    ("Vector4I", 13),
                    ("Plane", 14),
                    ("Quaternion", 15),
                    ("Aabb", 16),
                    ("Basis", 17),
                    ("Transform3D", 18),
                    ("Projection", 19),
                    ("Color", 20),
                    ("StringName", 21),
                    ("NodePath", 22),
                    ("Rid", 23),
                    ("Object", 24),
                    ("Callable", 25),
                    ("Signal", 26),
                    ("Dictionary", 27),
                    ("Array", 28),
                    ("PackedByteArray", 29),
                    ("PackedInt32Array", 30),
                    ("PackedInt64Array", 31),
                    ("PackedFloat32Array", 32),
                    ("PackedFloat64Array", 33),
                    ("PackedStringArray", 34),
                    ("PackedVector2Array", 35),
                    ("PackedVector3Array", 36),
                    ("PackedColorArray", 37),
                ],
            }
        },
        {
            new GodotEnumInfo()
            {
                Name = "Error",
                Values =
                [
                    new() { Name = "OK", Value = 0 },
                    new() { Name = "FAILED", Value = 1 },
                    new() { Name = "ERR_UNAVAILABLE", Value = 2 },
                    new() { Name = "ERR_UNCONFIGURED", Value = 3 },
                    new() { Name = "ERR_UNAUTHORIZED", Value = 4 },
                    new() { Name = "ERR_PARAMETER_RANGE_ERROR", Value = 5 },
                    new() { Name = "ERR_OUT_OF_MEMORY", Value = 6 },
                    new() { Name = "ERR_FILE_NOT_FOUND", Value = 7 },
                    new() { Name = "ERR_FILE_BAD_DRIVE", Value = 8 },
                    new() { Name = "ERR_FILE_BAD_PATH", Value = 9 },
                    new() { Name = "ERR_FILE_NO_PERMISSION", Value = 10 },
                    new() { Name = "ERR_FILE_ALREADY_IN_USE", Value = 11 },
                    new() { Name = "ERR_FILE_CANT_OPEN", Value = 12 },
                    new() { Name = "ERR_FILE_CANT_WRITE", Value = 13 },
                    new() { Name = "ERR_FILE_CANT_READ", Value = 14 },
                    new() { Name = "ERR_FILE_UNRECOGNIZED", Value = 15 },
                    new() { Name = "ERR_FILE_CORRUPT", Value = 16 },
                    new() { Name = "ERR_FILE_MISSING_DEPENDENCIES", Value = 17 },
                    new() { Name = "ERR_FILE_EOF", Value = 18 },
                    new() { Name = "ERR_CANT_OPEN", Value = 19 },
                    new() { Name = "ERR_CANT_CREATE", Value = 20 },
                    new() { Name = "ERR_QUERY_FAILED", Value = 21 },
                    new() { Name = "ERR_ALREADY_IN_USE", Value = 22 },
                    new() { Name = "ERR_LOCKED", Value = 23 },
                    new() { Name = "ERR_TIMEOUT", Value = 24 },
                    new() { Name = "ERR_CANT_CONNECT", Value = 25 },
                    new() { Name = "ERR_CANT_RESOLVE", Value = 26 },
                    new() { Name = "ERR_CONNECTION_ERROR", Value = 27 },
                    new() { Name = "ERR_CANT_ACQUIRE_RESOURCE", Value = 28 },
                    new() { Name = "ERR_CANT_FORK", Value = 29 },
                    new() { Name = "ERR_INVALID_DATA", Value = 30 },
                    new() { Name = "ERR_INVALID_PARAMETER", Value = 31 },
                    new() { Name = "ERR_ALREADY_EXISTS", Value = 32 },
                    new() { Name = "ERR_DOES_NOT_EXIST", Value = 33 },
                    new() { Name = "ERR_DATABASE_CANT_READ", Value = 34 },
                    new() { Name = "ERR_DATABASE_CANT_WRITE", Value = 35 },
                    new() { Name = "ERR_COMPILATION_FAILED", Value = 36 },
                    new() { Name = "ERR_METHOD_NOT_FOUND", Value = 37 },
                    new() { Name = "ERR_LINK_FAILED", Value = 38 },
                    new() { Name = "ERR_SCRIPT_FAILED", Value = 39 },
                    new() { Name = "ERR_CYCLIC_LINK", Value = 40 },
                    new() { Name = "ERR_INVALID_DECLARATION", Value = 41 },
                    new() { Name = "ERR_DUPLICATE_SYMBOL", Value = 42 },
                    new() { Name = "ERR_PARSE_ERROR", Value = 43 },
                    new() { Name = "ERR_BUSY", Value = 44 },
                    new() { Name = "ERR_SKIP", Value = 45 },
                    new() { Name = "ERR_HELP", Value = 46 },
                    new() { Name = "ERR_BUG", Value = 47 },
                    new() { Name = "ERR_PRINTER_ON_FIRE", Value = 48 },
                ],
            },
            new EnumInfo("Error")
            {
                Values =
                [
                    ("Ok", 0),
                    ("Failed", 1),
                    ("Unavailable", 2),
                    ("Unconfigured", 3),
                    ("Unauthorized", 4),
                    ("ParameterRangeError", 5),
                    ("OutOfMemory", 6),
                    ("FileNotFound", 7),
                    ("FileBadDrive", 8),
                    ("FileBadPath", 9),
                    ("FileNoPermission", 10),
                    ("FileAlreadyInUse", 11),
                    ("FileCantOpen", 12),
                    ("FileCantWrite", 13),
                    ("FileCantRead", 14),
                    ("FileUnrecognized", 15),
                    ("FileCorrupt", 16),
                    ("FileMissingDependencies", 17),
                    ("FileEof", 18),
                    ("CantOpen", 19),
                    ("CantCreate", 20),
                    ("QueryFailed", 21),
                    ("AlreadyInUse", 22),
                    ("Locked", 23),
                    ("Timeout", 24),
                    ("CantConnect", 25),
                    ("CantResolve", 26),
                    ("ConnectionError", 27),
                    ("CantAcquireResource", 28),
                    ("CantFork", 29),
                    ("InvalidData", 30),
                    ("InvalidParameter", 31),
                    ("AlreadyExists", 32),
                    ("DoesNotExist", 33),
                    ("DatabaseCantRead", 34),
                    ("DatabaseCantWrite", 35),
                    ("CompilationFailed", 36),
                    ("MethodNotFound", 37),
                    ("LinkFailed", 38),
                    ("ScriptFailed", 39),
                    ("CyclicLink", 40),
                    ("InvalidDeclaration", 41),
                    ("DuplicateSymbol", 42),
                    ("ParseError", 43),
                    ("Busy", 44),
                    ("Skip", 45),
                    ("Help", 46),
                    ("Bug", 47),
                    ("PrinterOnFire", 48),
                ],
            }
        },
    };

    [Theory]
    [MemberData(nameof(GetEnumTestData))]
    public void GenerateEnumInfo(GodotEnumInfo engineEnum, EnumInfo expected)
    {
        string enumName = NamingUtils.PascalToPascalCase(engineEnum.Name);
        var actual = new EnumInfo(enumName);

        foreach (var (name, value) in engineEnum.Values)
        {
            actual.Values.Add((NamingUtils.SnakeToPascalCase(name), value));
        }

        int enumPrefix = NamingUtils.DetermineEnumPrefix(engineEnum);

        // HARDCODED: The Error enum have the prefix 'ERR_' for everything except 'OK' and 'FAILED'.
        if (engineEnum.Name == "Error")
        {
            enumPrefix = 1; // 'ERR_'
        }

        NamingUtils.ApplyPrefixToEnumConstants(engineEnum, actual, enumPrefix);
        NamingUtils.RemoveMaxConstant(engineEnum, actual);

        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Values, actual.Values);
    }
}
