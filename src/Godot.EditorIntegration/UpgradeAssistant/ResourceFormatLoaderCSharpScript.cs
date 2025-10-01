using System;
using System.IO;
using Godot.Collections;

namespace Godot.EditorIntegration.UpgradeAssistant;

[GodotClass]
internal sealed partial class ResourceFormatLoaderCSharpScript : ResourceFormatLoader
{
    private static class Constants
    {
        public static StringName ScriptStringName { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Script"u8);
        public static StringName CSharpScriptStringName { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("CSharpScript"u8);
        public const string CSharpScriptName = "CSharpScript";
        public const string CSharpScriptExtension = "cs";
    }

    protected override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
    {
        if (!path.StartsWith("res://", StringComparison.Ordinal))
        {
            GD.PushError(SR.UpgradeAssistant_CSharpScriptPathMustBePrefixedWithRes);
            return default;
        }

        var script = new CSharpScript();
        script.TakeOverPath(originalPath);
        return script;
    }

    protected override bool _RecognizePath(string path, StringName type)
    {
        var extension = Path.GetExtension(path.AsSpan());
        return extension.Equals($".{Constants.CSharpScriptExtension}", StringComparison.OrdinalIgnoreCase);
    }

    protected override PackedStringArray _GetRecognizedExtensions()
    {
        return [Constants.CSharpScriptExtension];
    }

    protected override bool _HandlesType(StringName type)
    {
        return type == Constants.ScriptStringName || type == Constants.CSharpScriptStringName;
    }

    protected override string _GetResourceType(string path)
    {
        var extension = Path.GetExtension(path.AsSpan());
        if (extension.Equals(Constants.CSharpScriptExtension, StringComparison.OrdinalIgnoreCase))
        {
            return Constants.CSharpScriptName;
        }

        return "";
    }
}
