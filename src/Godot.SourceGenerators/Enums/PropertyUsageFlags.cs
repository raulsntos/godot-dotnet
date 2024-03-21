using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Godot.SourceGenerators;

// IMPORTANT: Must match the type defined in GodotBindings/Generated/GlobalEnums/PropertyUsageFlags.cs

[Flags]
[SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Enum members are copied directly from the generated enum.")]
internal enum PropertyUsageFlags
{
    None = 0,
    Storage = 2,
    Editor = 4,
    Internal = 8,
    Checkable = 16,
    Checked = 32,
    Group = 64,
    Category = 128,
    Subgroup = 256,
    ClassIsBitfield = 512,
    NoInstanceState = 1024,
    RestartIfChanged = 2048,
    ScriptVariable = 4096,
    StoreIfNull = 8192,
    UpdateAllIfModified = 16384,
    ScriptDefaultValue = 32768,
    ClassIsEnum = 65536,
    NilIsVariant = 131072,
    Array = 262144,
    AlwaysDuplicate = 524288,
    NeverDuplicate = 1048576,
    HighEndGfx = 2097152,
    NodePathFromSceneRoot = 4194304,
    ResourceNotPersistent = 8388608,
    KeyingIncrements = 16777216,
    DeferredSetResource = 33554432,
    EditorInstantiateObject = 67108864,
    EditorBasicSetting = 134217728,
    ReadOnly = 268435456,
    Secret = 536870912,
    Default = 6,
    NoEditor = 2,
}

internal static class PropertyUsageFlagsExtensions
{
    public static string FullNameWithGlobal(this PropertyUsageFlags propertyUsageFlags)
    {
        PropertyUsageFlags[] values = (PropertyUsageFlags[])Enum.GetValues(typeof(PropertyUsageFlags));

        if (TryGetSingleFlagName(propertyUsageFlags, values, out string? name))
        {
            return GetFlagFullNameWithGlobal(name);
        }

        string[] names = Enum.GetNames(typeof(PropertyUsageFlags));

        // With a ulong result value, regardless of the enum's base type, the maximum
        // possible number of consistent name/values we could have is 64, since every
        // value is made up of one or more bits, and when we see values and incorporate
        // their names, we effectively switch off those bits.
        Span<int> foundItems = stackalloc int[64];

        if (TryGetMultipleFlagNames(propertyUsageFlags, values, foundItems, out int foundItemCount))
        {
            var sb = new StringBuilder();

            for (int i = foundItemCount - 1; i >= 0; i--)
            {
                int nameIndex = foundItems[i];
                sb.Append(GetFlagFullNameWithGlobal(names[nameIndex]));
                if (i > 0)
                {
                    sb.Append(" | ");
                }
            }

            return sb.ToString();
        }

        // We could not construct the full name string, fallback to a numeric value.
        return $"(global::Godot.PropertyUsageFlags)({propertyUsageFlags:D})";

        static string GetFlagFullNameWithGlobal(string name)
        {
            return $"global::Godot.PropertyUsageFlags.{name}";
        }

        static bool TryGetSingleFlagName(PropertyUsageFlags value, PropertyUsageFlags[] values, [NotNullWhen(true)] out string? name)
        {
            // Shortcut for default value.
            if (value == PropertyUsageFlags.None)
            {
                name = Enum.GetName(typeof(PropertyUsageFlags), PropertyUsageFlags.None);
                return true;
            }

            // Iterate backwards because the values are sorted from lowest to highest.
            for (int i = values.Length - 1; i < values.Length; i--)
            {
                if (values[i] <= value)
                {
                    if (values[i] == value)
                    {
                        // The value matches exactly, so there is only one flag and we found it.
                        name = Enum.GetName(typeof(PropertyUsageFlags), value);
                        return true;
                    }

                    // The value doesn't match exactly, and since the values are sorted
                    // we know we won't be able to find an exact match so we break the loop.
                    break;
                }
            }

            name = null;
            return false;
        }

        static bool TryGetMultipleFlagNames(PropertyUsageFlags value, PropertyUsageFlags[] values, Span<int> foundItems, out int foundItemCount)
        {
            foundItemCount = 0;

            for (int i = values.Length - 1; ; i--)
            {
                PropertyUsageFlags currentValue = values[i];
                if (i == 0 && currentValue == 0)
                {
                    // We reached the end of the flags values.
                    break;
                }

                if ((value & currentValue) == currentValue)
                {
                    // If the flag exists in the value, remove it from the value
                    // and add the foundItem index to the destination span.
                    value &= ~currentValue;
                    foundItems[foundItemCount++] = i;
                    if (value == 0)
                    {
                        // We've exhausted the flags in the value, so we're done.
                        break;
                    }
                }
            }

            // If we exhausted looking through all the values and we still have
            // a non-zero result, we couldn't match the result to only named values.
            return value == 0;
        }
    }
}
