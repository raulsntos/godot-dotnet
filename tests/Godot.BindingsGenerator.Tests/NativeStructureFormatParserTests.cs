using System;
using System.Collections.Generic;
using Godot.BindingsGenerator;

namespace Godot.Common.Tests;

public class NativeStructureFormatParserTests
{
    /// <summary>
    /// Represents the values of a field parsed from a format string.
    /// </summary>
    public sealed record FieldInfo
    {
        /// <summary>
        /// Type name (including pointer suffix like "Object *").
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Field name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Default value, or null if none.
        /// </summary>
        public string? DefaultValue { get; init; }

        /// <summary>
        /// Array length, or -1 if not array.
        /// </summary>
        public int ArrayLength { get; init; } = -1;

        public FieldInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public FieldInfo(ReadOnlySpan<char> type, ReadOnlySpan<char> name) : this(type.ToString(), name.ToString()) { }
    }

    [Theory]
    [InlineData("int32_tfield_one", "Missing space after type")]
    [InlineData("int32_t field_one[]", "Empty array brackets")]
    [InlineData("int32_t field_one =", "Missing default value after =")]
    public void InvalidFormatThrowsException(string format, string description)
    {
        _ = description; // Used for test display name only.

        Assert.Throws<FormatException>(() =>
        {
            foreach (var _ in NativeStructureFormatParser.EnumerateFields(format)) { }
        });
    }

    [Theory]
    [MemberData(nameof(GetParseFormatTestData))]
    public void ParseFormat(string structureName, string format, FieldInfo[] expectedFields)
    {
        // Used for test display name only.
        _ = structureName;

        var actualFields = new List<FieldInfo>();
        foreach (var field in NativeStructureFormatParser.EnumerateFields(format))
        {
            actualFields.Add(new FieldInfo(field.Type, field.Name)
            {
                DefaultValue = field.HasDefaultValue ? field.DefaultValue.ToString() : null,
                ArrayLength = field.ArrayLength,
            });
        }

        Assert.Equal(expectedFields.Length, actualFields.Count);
        for (int i = 0; i < expectedFields.Length; i++)
        {
            Assert.Equal(expectedFields[i], actualFields[i]);
        }
    }

    public static TheoryData<string, string, FieldInfo[]> GetParseFormatTestData()
    {
        return new TheoryData<string, string, FieldInfo[]>
        {
            // Simple structure with two fields.
            {
                "AudioFrame",
                "float left;float right",
                [
                    new("float", "left"),
                    new("float", "right"),
                ]
            },
            // Structure with namespaced types.
            {
                "CaretInfo",
                "Rect2 leading_caret;Rect2 trailing_caret;TextServer::Direction leading_direction;TextServer::Direction trailing_direction",
                [
                    new("Rect2", "leading_caret"),
                    new("Rect2", "trailing_caret"),
                    new("TextServer::Direction", "leading_direction"),
                    new("TextServer::Direction", "trailing_direction"),
                ]
            },
            // Structure with many default values including negative and float literals.
            {
                "Glyph",
                "int start = -1;int end = -1;uint8_t count = 0;uint8_t repeat = 1;uint16_t flags = 0;float x_off = 0.f;float y_off = 0.f;float advance = 0.f;RID font_rid;int font_size = 0;int32_t index = 0",
                [
                    new("int", "start") { DefaultValue = "-1" },
                    new("int", "end") { DefaultValue = "-1" },
                    new("uint8_t", "count") { DefaultValue = "0" },
                    new("uint8_t", "repeat") { DefaultValue = "1" },
                    new("uint16_t", "flags") { DefaultValue = "0" },
                    new("float", "x_off") { DefaultValue = "0.f" },
                    new("float", "y_off") { DefaultValue = "0.f" },
                    new("float", "advance") { DefaultValue = "0.f" },
                    new("RID", "font_rid"),
                    new("int", "font_size") { DefaultValue = "0" },
                    new("int32_t", "index") { DefaultValue = "0" },
                ]
            },
            // Structure with 12 fields, no defaults or arrays.
            {
                "PhysicsServer2DExtensionMotionResult",
                "Vector2 travel;Vector2 remainder;Vector2 collision_point;Vector2 collision_normal;Vector2 collider_velocity;real_t collision_depth;real_t collision_safe_fraction;real_t collision_unsafe_fraction;int collision_local_shape;ObjectID collider_id;RID collider;int collider_shape",
                [
                    new("Vector2", "travel"),
                    new("Vector2", "remainder"),
                    new("Vector2", "collision_point"),
                    new("Vector2", "collision_normal"),
                    new("Vector2", "collider_velocity"),
                    new("real_t", "collision_depth"),
                    new("real_t", "collision_safe_fraction"),
                    new("real_t", "collision_unsafe_fraction"),
                    new("int", "collision_local_shape"),
                    new("ObjectID", "collider_id"),
                    new("RID", "collider"),
                    new("int", "collider_shape"),
                ]
            },
            // Structure with pointer type.
            {
                "PhysicsServer2DExtensionRayResult",
                "Vector2 position;Vector2 normal;RID rid;ObjectID collider_id;Object *collider;int shape",
                [
                    new("Vector2", "position"),
                    new("Vector2", "normal"),
                    new("RID", "rid"),
                    new("ObjectID", "collider_id"),
                    new("Object *", "collider"),
                    new("int", "shape"),
                ]
            },
            // Structure with fixed-size array.
            {
                "PhysicsServer3DExtensionMotionResult",
                "Vector3 travel;Vector3 remainder;real_t collision_depth;real_t collision_safe_fraction;real_t collision_unsafe_fraction;PhysicsServer3DExtensionMotionCollision collisions[32];int collision_count",
                [
                    new("Vector3", "travel"),
                    new("Vector3", "remainder"),
                    new("real_t", "collision_depth"),
                    new("real_t", "collision_safe_fraction"),
                    new("real_t", "collision_unsafe_fraction"),
                    new("PhysicsServer3DExtensionMotionCollision", "collisions") { ArrayLength = 32 },
                    new("int", "collision_count"),
                ]
            },
        };
    }
}
