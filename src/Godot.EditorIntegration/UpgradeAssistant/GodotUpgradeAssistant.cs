using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Godot.Bridge;
using Godot.Collections;
using Godot.EditorIntegration.Build.Cli;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.UpgradeAssistant;

internal sealed class GodotUpgradeAssistant
{
    private static class PropertyName
    {
        public static StringName Script { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("script"u8);
    }

    private static HashSet<string>? _recognizedExtensions;

    private bool _previewUpgradeEnabled;

    private readonly List<string> _upgradableFilePaths = [];

    private readonly Dictionary<string, Resource> _upgradableResourceInstances = [];
    private readonly Dictionary<string, SceneInfo> _upgradableSceneInstances = [];

    private static bool IsPreviewEnabled(string projectPath)
    {
        var startInfo = DotNetCli.CreateBuildStartInfo(new Build.BuildOptions()
        {
            SlnOrProject = projectPath,
            GetProperty = "EnableGodotDotNetPreview",
        });

        try
        {
            using var process = StartProcess(startInfo);

            process.WaitForExit();
            int exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                // Print the process output because it may contain more details about the error.
                GD.Print(process.StandardOutput.ReadToEnd());
                GD.Print(process.StandardError.ReadToEnd());
                GD.PushError($".NET failed to check Godot .NET preview status for '{projectPath}'. See output above for more details.");
                return false;
            }

            return process.StandardOutput.ReadToEnd().Trim() == "true";
        }
        catch (Exception e)
        {
            GD.PushError($".NET failed to check Godot .NET preview status for '{projectPath}'. Exception: {e}");
            return false;
        }
    }

    private static bool IsCSharpScript(Script? script)
    {
        if (script is null)
        {
            return false;
        }

        var scriptPath = script.GetPath().AsSpan();
        var scriptExtension = Path.GetExtension(scriptPath);
        if (!scriptExtension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static bool IsCSharpScript(Script? script, [NotNullWhen(true)] out string? className)
    {
        if (script is null)
        {
            className = null;
            return false;
        }

        var scriptPath = script.GetPath().AsSpan();
        var scriptExtension = Path.GetExtension(scriptPath);
        if (!scriptExtension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            className = null;
            return false;
        }

        // In GodotSharp, the name of the C# class must match the filename
        // so we can get the class name from the path.
        className = Path.GetFileNameWithoutExtension(scriptPath).ToString();
        return true;
    }

    private static void CollectUpgradableFiles(string rootPath, List<string> upgradableFilePaths)
    {
        _recognizedExtensions ??=
        [
            ..ResourceLoader.Singleton.GetRecognizedExtensionsForType("PackedScene"),
            ..ResourceLoader.Singleton.GetRecognizedExtensionsForType("Resource"),
        ];

        var recognizedExtensionsLookup = _recognizedExtensions.GetAlternateLookup<ReadOnlySpan<char>>();

        foreach (string filePath in Directory.EnumerateFiles(rootPath, "*", new EnumerationOptions()
        {
            RecurseSubdirectories = true,
        }))
        {
            var extension = Path.GetExtension(filePath.AsSpan()).TrimStart('.');
            if (extension.IsEmpty)
            {
                continue;
            }

            if (!recognizedExtensionsLookup.Contains(extension))
            {
                continue;
            }

            if (!ResourceLoader.Singleton.Exists(filePath))
            {
                // Skip resources that aren't recognized by any loader.
                // This also skips files ignored due to '.gdignore'.
                continue;
            }

            if (FileNeedsUpgrade(filePath))
            {
                upgradableFilePaths.Add(filePath);
            }
        }
    }

    private static bool FileNeedsUpgrade(string filePath)
    {
        // Check if the file is a resource with C# scripts attached.
        Resource? resource = ResourceLoader.Singleton.Load(filePath);
        if (resource is null)
        {
            // Not a resource, or not one we care about anyway.
            return false;
        }

        PackedScene? scene = resource as PackedScene;
        if (scene is not null)
        {
            SceneState sceneState = scene.GetState();
            for (int nodeIndex = 0; nodeIndex < sceneState.GetNodeCount(); nodeIndex++)
            {
                for (int propertyIndex = 0; propertyIndex < sceneState.GetNodePropertyCount(nodeIndex); propertyIndex++)
                {
                    StringName propertyName = sceneState.GetNodePropertyName(nodeIndex, propertyIndex);
                    if (propertyName == PropertyName.Script)
                    {
                        Script? nodeScript = VariantToScript(sceneState.GetNodePropertyValue(nodeIndex, propertyIndex));
                        if (IsCSharpScript(nodeScript))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        Script? resourceScript = VariantToScript(resource.GetScript());
        if (IsCSharpScript(resourceScript))
        {
            return true;
        }

        HashSet<Resource> subResources = [];
        var properties = resource.GetPropertyList();
        foreach (var propertyDictionary in properties)
        {
            var property = new PropertyInfo(propertyDictionary);
            if (!property.Usage.HasFlag(PropertyUsageFlags.Storage))
            {
                // Ignore properties that aren't serialized.
                continue;
            }

            var propertyValue = resource.Get(property.Name);

            subResources.Clear();
            FindSubResources(propertyValue, subResources);

            foreach (var subResource in subResources)
            {
                Script? subResourceScript = VariantToScript(subResource.GetScript());
                if (IsCSharpScript(subResourceScript))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void FindSubResources(Variant variant, HashSet<Resource> resourcesFound)
    {
        switch (variant.VariantType)
        {
            case VariantType.Array:
                foreach (Variant v in variant.AsGodotArray())
                {
                    FindSubResources(v, resourcesFound);
                }
                break;

            case VariantType.Dictionary:
                foreach (var (key, value) in variant.AsGodotDictionary())
                {
                    FindSubResources(key, resourcesFound);
                    FindSubResources(value, resourcesFound);
                }
                break;

            case VariantType.Object:
                if (variant.AsGodotObject() is Resource resource)
                {
                    resourcesFound.Add(resource);
                }
                break;
        }
    }

    private static void CopyProperties(GodotObject source, GodotObject destination, SceneInfo? sceneInfo = null)
    {
        if (sceneInfo is not null)
        {
            // If scene information is provided, the source and destination must be nodes.
            Debug.Assert(source is Node);
            Debug.Assert(destination is Node);

            CopyPropertiesScene((Node)source, (Node)destination, sceneInfo);
            return;
        }

        CopyPropertiesCore(source, destination);
    }

    private static void CopyPropertiesCore(GodotObject source, GodotObject destination)
    {
        using var properties = source.GetPropertyList();
        foreach (var propertyDictionary in properties)
        {
            var property = new PropertyInfo(propertyDictionary);
            if (!property.Usage.HasFlag(PropertyUsageFlags.Storage))
            {
                // Ignore properties that aren't serialized.
                continue;
            }

            using StringName propertyName = property.Name;

            if (propertyName == PropertyName.Script)
            {
                // Ignore the script property, the C# script will become a GDExtension class.
                continue;
            }

            var propertyValue = source.Get(propertyName);

            using StringName className = new StringName(source.GetClass());
            Variant defaultValue = ClassDB.Singleton.ClassGetPropertyDefaultValue(className, property.Name);

            if (defaultValue.VariantType != VariantType.Nil && Variant.Evaluate(VariantOperator.Equal, propertyValue, defaultValue).AsBool())
            {
                // Skip properties that match the default value.
                continue;
            }

            if (property.VariantType == VariantType.Object)
            {
                GodotObject? propertyValueAsObject = propertyValue.AsGodotObject();

                if (propertyValueAsObject is null && !property.Usage.HasFlag(PropertyUsageFlags.StoreIfNull))
                {
                    // Skip null object properties unless explicitly requested by the usage flags.
                    continue;
                }

                Resource? propertyValueAsResource = propertyValueAsObject as Resource;
                if (propertyValueAsResource is not null)
                {
                    // The value is a Resource, attempt to upgrade it in case it has a C# script attached.
                    propertyValue = UpgradeResource(propertyValueAsResource, isRootResource: false);
                }
            }

            destination.Set(propertyName, propertyValue);
        }
    }

    private static void CopyPropertiesScene(Node source, Node destination, SceneInfo sceneInfo)
    {
        int nodeIndex = IndexOfNode(sceneInfo, source);
        if (nodeIndex == -1)
        {
            // This should be unreachable.
            GD.PushWarning($"Node '{source.Name}' not found when copying scene properties.");
            CopyPropertiesCore(source, destination);
            return;
        }

        int propertyCount = sceneInfo.State.GetNodePropertyCount(nodeIndex);
        for (int i = 0; i < propertyCount; i++)
        {
            StringName propertyName = sceneInfo.State.GetNodePropertyName(nodeIndex, i);
            if (propertyName == PropertyName.Script)
            {
                // Ignore the script property, the C# script will become a GDExtension class.
                continue;
            }

            Variant propertyValue = sceneInfo.State.GetNodePropertyValue(nodeIndex, i);
            destination.Set(propertyName, propertyValue);
        }

        static int IndexOfNode(SceneInfo sceneInfo, Node node)
        {
            int nodeCount = sceneInfo.State.GetNodeCount();
            for (int i = 0; i < nodeCount; i++)
            {
                NodePath path = sceneInfo.State.GetNodePath(i);
                Node currentNode = sceneInfo.Node.GetNode(path);
                if (currentNode == node)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    private static void CopySignals(Node source, Node destination, SceneInfo sceneInfo)
    {
        int connectionCount = sceneInfo.State.GetConnectionCount();
        for (int i = 0; i < connectionCount; i++)
        {
            NodePath connectionSourcePath = sceneInfo.State.GetConnectionSource(i);
            var connectionSource = sceneInfo.Node.GetNode(connectionSourcePath);

            if (connectionSource != source)
            {
                // Skip connections in the scene that aren't relevant for the current source node.
                continue;
            }

            NodePath connectionTargetPath = sceneInfo.State.GetConnectionTarget(i);
            var connectionTarget = sceneInfo.Node.GetNode(connectionTargetPath);

            StringName connectionSignal = sceneInfo.State.GetConnectionSignal(i);
            StringName connectionMethod = sceneInfo.State.GetConnectionMethod(i);

            uint connectionFlags = (uint)sceneInfo.State.GetConnectionFlags(i);

            var callable = new Callable(connectionTarget, connectionMethod);

            source.Disconnect(connectionSignal, callable);
            destination.Connect(connectionSignal, callable, connectionFlags);
        }
    }

    public void Prepare()
    {
        // Register a C# script loader so we can parse old resources.
        using var csharpScriptLoader = new ResourceFormatLoaderCSharpScript();
        ResourceLoader.Singleton.AddResourceFormatLoader(csharpScriptLoader);

        // First clear data that may be left from a previous upgrade.
        _upgradableFilePaths.Clear();

        // We are in the 'dotnet' module so an upgrade to Godot .NET must be performed.
        _previewUpgradeEnabled = true;

        // Check if the Godot .NET preview is already enabled in the project.
        string csprojPath = EditorPath.ProjectCSProjPath;
        bool isPreviewEnabled = IsPreviewEnabled(csprojPath);

        if (isPreviewEnabled)
        {
            // If the Godot .NET preview is already enabled, this implies that all future upgrades
            // must enable the preview since a downgrade is not possible.
            _previewUpgradeEnabled = true;
        }
        else if (_previewUpgradeEnabled)
        {
            // Otherwise, the project is still using GodotSharp.
            // If an upgrade to Godot .NET should be performed, collect all the scenes and resources
            // that must be upgraded.
            string rootPath = ProjectSettings.Singleton.GlobalizePath("res://");
            CollectUpgradableFiles(rootPath, _upgradableFilePaths);
        }

        // Unregister the C# script loader.
        ResourceLoader.Singleton.RemoveResourceFormatLoader(csharpScriptLoader);
    }

    public void Upgrade()
    {
        // Register a C# script loader so we can parse old resources.
        using var csharpScriptLoader = new ResourceFormatLoaderCSharpScript();
        ResourceLoader.Singleton.AddResourceFormatLoader(csharpScriptLoader);

        if (_previewUpgradeEnabled)
        {
            // We only upgrade scenes and resources when an upgrade to Godot .NET should be performed.

            // Create all instances upfront to avoid 'missing class' errors.
            foreach (string filePath in _upgradableFilePaths)
            {
                var resource = ResourceLoader.Singleton.Load(filePath);
                _upgradableResourceInstances[filePath] = resource;

                if (resource is PackedScene scene)
                {
                    Node rootNode = scene.Instantiate(PackedScene.GenEditState.Instance);
                    _upgradableSceneInstances[filePath] = new SceneInfo(rootNode, scene.GetState());
                }
            }

            foreach (string filePath in _upgradableFilePaths)
            {
                UpgradeFile(filePath);
            }
        }

        // Unregister the C# script loader.
        ResourceLoader.Singleton.RemoveResourceFormatLoader(csharpScriptLoader);

        UpgradeDotNetProject();

        // Clear cached instances.
        _upgradableSceneInstances.Clear();
        _upgradableResourceInstances.Clear();
    }

    private void UpgradeFile(string filePath)
    {
        var resource = _upgradableResourceInstances[filePath];

        PackedScene? scene = resource as PackedScene;
        if (scene is not null)
        {
            SceneInfo sceneInfo = _upgradableSceneInstances[filePath];

            Node rootNode = sceneInfo.Node;
            Node newRootNode = UpgradeScene(sceneInfo, rootNode, rootNode);
            if (rootNode != newRootNode)
            {
                rootNode.ReplaceBy(newRootNode, keepGroups: true);
                rootNode.QueueFree();
                rootNode = newRootNode;
            }
            scene.Pack(rootNode);
        }
        else
        {
            resource = UpgradeResource(resource, isRootResource: true);
        }

        ResourceSaver.Singleton.Save(resource, filePath);
    }

    private static Node UpgradeScene(SceneInfo sceneInfo, Node rootNode, Node originalNode)
    {
        for (int i = 0; i < originalNode.GetChildCount(); i++)
        {
            Node childNode = originalNode.GetChild(i);
            Node newChildNode = UpgradeScene(sceneInfo, rootNode, childNode);
            if (childNode != newChildNode)
            {
                childNode.ReplaceBy(newChildNode, keepGroups: true);
                childNode.QueueFree();
            }
        }

        Script? script = VariantToScript(originalNode.GetScript());
        if (IsCSharpScript(script, out string? className))
        {
            return UpgradeNode(sceneInfo, originalNode, className);
        }

        return originalNode;
    }

    private static MissingNode UpgradeNode(SceneInfo sceneInfo, Node originalNode, string className)
    {
        var newNode = new MissingNode()
        {
            Name = originalNode.Name,
            OriginalClass = className,
            RecordingProperties = true,
            RecordingSignals = true,
        };

        CopyProperties(originalNode, newNode, sceneInfo);
        CopySignals(originalNode, newNode, sceneInfo);

        return newNode;
    }

    private static Resource UpgradeResource(Resource originalResource, bool isRootResource)
    {
        if (!isRootResource)
        {
            // If the resource is not the root of a file,
            // it may be a sub-resource or an external resource.
            string resourcePath = originalResource.GetPath();

            // If it's a sub-resource, the path will contain `::`.
            // Otherwise, it's an external resource.
            bool isSubResource = resourcePath.Contains("::");

            if (!isSubResource)
            {
                // We don't upgrade external resources referenced by other resources,
                // they'll be upgraded when we reach the file that they're the root of.
                return originalResource;
            }
        }

        // Find sub-resources in the properties and upgrade them if needed.
        var properties = originalResource.GetPropertyList();
        foreach (var propertyDictionary in properties)
        {
            var property = new PropertyInfo(propertyDictionary);
            if (!property.Usage.HasFlag(PropertyUsageFlags.Storage))
            {
                // Ignore properties that aren't serialized.
                continue;
            }

            if (property.Name == PropertyName.Script)
            {
                // Skip script property.
                continue;
            }

            var propertyValue = originalResource.Get(property.Name);
            var newPropertyValue = UpgradeSubResources(propertyValue);
            if (!Variant.Evaluate(VariantOperator.Equal, propertyValue, newPropertyValue).AsBool())
            {
                originalResource.Set(property.Name, newPropertyValue);
            }
        }

        Script? script = VariantToScript(originalResource.GetScript());
        if (IsCSharpScript(script, out string? className))
        {
            return UpgradeResourceCore(originalResource, className);
        }

        return originalResource;
    }

    private static Variant UpgradeSubResources(Variant variant)
    {
        switch (variant.VariantType)
        {
            case VariantType.Array:
                var array = variant.AsGodotArray();
                for (int i = 0; i < array.Count; i++)
                {
                    array[i] = UpgradeSubResources(array[i]);
                }
                break;

            case VariantType.Dictionary:
                var dictionary = variant.AsGodotDictionary();
                foreach (var (key, value) in dictionary)
                {
                    dictionary.Remove(key);
                    var newKey = UpgradeSubResources(key);
                    var newValue = UpgradeSubResources(value);
                    dictionary[newKey] = newValue;
                }
                break;

            case VariantType.Object:
                if (variant.AsGodotObject() is Resource resource)
                {
                    return UpgradeResource(resource, isRootResource: false);
                }
                break;
        }

        return variant;
    }

    private static MissingResource UpgradeResourceCore(Resource originalResource, string className)
    {
        var newResource = new MissingResource()
        {
            OriginalClass = className,
            RecordingProperties = true,
        };

        newResource.TakeOverPath(originalResource.GetPath());
        CopyProperties(originalResource, newResource);

        return newResource;
    }

    private static void UpgradeDotNetProject()
    {
        string godotDotNetUpgradeAssistantPath = Path.Join(EditorPath.EditorAssembliesPath, "Godot.UpgradeAssistant.Cli", "Godot.UpgradeAssistant.Cli.dll");

        string projectGodotFilePath = ProjectSettings.Singleton.GlobalizePath("res://project.godot");
        string projectName = EditorPath.ProjectAssemblyName;

        var startInfo = new ProcessStartInfo()
        {
            FileName = "dotnet",
            ArgumentList =
            {
                godotDotNetUpgradeAssistantPath,
                "upgrade",
                projectGodotFilePath,
                "--solution", EditorPath.ProjectSlnPath,
                "--project", EditorPath.ProjectCSProjPath,
                "--target-godot-version", GodotBridge.GodotVersion.GetGodotDotNetVersion(),
                "--enable-preview",
            },
        };

        try
        {
            using var process = StartProcess(startInfo);

            process.WaitForExit();
            int exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                // Print the process output because it may contain more details about the error.
                GD.Print(process.StandardError.ReadToEnd());
                GD.PushError($".NET failed to upgrade project '{projectName}'. See output above for more details.");
            }
        }
        catch (Exception e)
        {
            GD.PushError($".NET failed to upgrade project '{projectName}'. Exception: {e}");
        }
    }

    private static Process StartProcess(ProcessStartInfo startInfo)
    {
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.StandardOutputEncoding = Encoding.UTF8;
        startInfo.StandardErrorEncoding = Encoding.UTF8;

        var process = new Process() { StartInfo = startInfo };
        process.Start();

        return process;
    }

    private static Script? VariantToScript(Variant variant)
    {
        if (variant.VariantType != VariantType.Object)
        {
            return null;
        }

        return variant.AsGodotObject() as Script;
    }

    private sealed class SceneInfo
    {
        public Node Node { get; }
        public SceneState State { get; }

        public SceneInfo(Node node, SceneState state)
        {
            Node = node;
            State = state;
        }
    }

    private readonly ref struct PropertyInfo
    {
        private readonly GodotDictionary _value;

        public VariantType VariantType => (VariantType)_value["type"].AsInt64();

        public PropertyUsageFlags Usage => (PropertyUsageFlags)_value["usage"].AsInt64();

        public StringName Name => _value["name"].AsStringName();

        public PropertyInfo(GodotDictionary value)
        {
            _value = value;
        }
    }
}
