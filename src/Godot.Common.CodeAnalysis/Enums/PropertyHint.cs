using System;

namespace Godot.Common.CodeAnalysis;

// IMPORTANT: Must match the type defined in GodotBindings/Generated/GlobalEnums/PropertyHint.cs

internal enum PropertyHint
{
    None = 0,
    Range = 1,
    Enum = 2,
    EnumSuggestion = 3,
    ExpEasing = 4,
    Link = 5,
    Flags = 6,
    Layers2DRender = 7,
    Layers2DPhysics = 8,
    Layers2DNavigation = 9,
    Layers3DRender = 10,
    Layers3DPhysics = 11,
    Layers3DNavigation = 12,
    LayersAvoidance = 37,
    File = 13,
    Dir = 14,
    GlobalFile = 15,
    GlobalDir = 16,
    ResourceType = 17,
    MultilineText = 18,
    Expression = 19,
    PlaceholderText = 20,
    ColorNoAlpha = 21,
    ObjectId = 22,
    TypeString = 23,
    NodePathToEditedNode = 24,
    ObjectTooBig = 25,
    NodePathValidTypes = 26,
    SaveFile = 27,
    GlobalSaveFile = 28,
    IntIsObjectid = 29,
    IntIsPointer = 30,
    ArrayType = 31,
    LocaleId = 32,
    LocalizableString = 33,
    NodeType = 34,
    HideQuaternionEdit = 35,
    Password = 36,
}

internal static class PropertyHintExtensions
{
    public static string FullNameWithGlobal(this PropertyHint propertyHint)
    {
        if (!Enum.IsDefined(typeof(PropertyHint), propertyHint))
        {
            throw new ArgumentOutOfRangeException(nameof(propertyHint), $"Unrecognized PropertyHint value '{propertyHint}'.");
        }

        return $"global::Godot.PropertyHint.{Enum.GetName(typeof(PropertyHint), propertyHint)}";
    }
}
