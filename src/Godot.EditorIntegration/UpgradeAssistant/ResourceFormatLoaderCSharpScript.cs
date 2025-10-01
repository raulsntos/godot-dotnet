using System;
using System.Collections.Generic;
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

    private bool _disposed;

    // Holds the variants and scripts created by the loader so they can be disposed when the loader is disposed.
    private readonly List<Variant> _returnedVariants = [];
    private readonly List<CSharpScript> _createdScripts = [];

    protected override Variant _Load(string path, string originalPath, bool useSubThreads, int cacheMode)
    {
        if (!path.StartsWith("res://", StringComparison.Ordinal))
        {
            GD.PushError(SR.UpgradeAssistant_CSharpScriptPathMustBePrefixedWithRes);
            return default;
        }

        var script = new CSharpScript();
        _createdScripts.Add(script);

        Variant variant = script;
        _returnedVariants.Add(variant);

        return variant;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            foreach (var variant in _returnedVariants)
            {
                variant.Dispose();
            }

            foreach (var script in _createdScripts)
            {
                script.Dispose();
            }

            _returnedVariants.Clear();
            _createdScripts.Clear();
        }

        base.Dispose(disposing);
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
