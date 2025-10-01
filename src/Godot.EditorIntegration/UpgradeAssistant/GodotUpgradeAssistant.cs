using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
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

    private static bool IsCSharpScript([NotNullWhen(true)] Script? script)
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
        // IMPORTANT: We avoid caching the loaded resources so we can make sure everything is disposed
        // by the time we finish the upgrade, so we can unregister the C# script and loader classes.
        using Resource? resource = ResourceLoader.Singleton.Load(filePath, cacheMode: ResourceLoader.CacheMode.IgnoreDeep);
        if (resource is null)
        {
            // Not a resource, or not one we care about anyway.
            return false;
        }

        PackedScene? scene = resource as PackedScene;
        if (scene is not null)
        {
            using SceneState sceneState = scene.GetState();
            for (int nodeIndex = 0; nodeIndex < sceneState.GetNodeCount(); nodeIndex++)
            {
                for (int propertyIndex = 0; propertyIndex < sceneState.GetNodePropertyCount(nodeIndex); propertyIndex++)
                {
                    StringName propertyName = sceneState.GetNodePropertyName(nodeIndex, propertyIndex);
                    if (propertyName == PropertyName.Script)
                    {
                        Script? nodeScript = sceneState.GetNodePropertyAsScript(nodeIndex, propertyIndex);
                        if (IsCSharpScript(nodeScript))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        using Script? resourceScript = resource.GetScriptAsScript();
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

            using Variant propertyValue = resource.Get(property.Name);

            subResources.Clear();
            FindSubResources(propertyValue, subResources);

            foreach (var subResource in subResources)
            {
                using Script? subResourceScript = subResource.GetScriptAsScript();
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

            using Variant propertyValue = source.Get(propertyName);

            using StringName className = new StringName(source.GetClass());
            using Variant defaultValue = ClassDB.Singleton.ClassGetPropertyDefaultValue(className, property.Name);

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
                    using Resource upgradedResource = UpgradeResource(propertyValueAsResource, isRootResource: false);
                    destination.Set(propertyName, upgradedResource);
                    continue;
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
            using StringName propertyName = sceneInfo.State.GetNodePropertyName(nodeIndex, i);
            if (propertyName == PropertyName.Script)
            {
                // Ignore the script property, the C# script will become a GDExtension class.
                continue;
            }

            using Variant propertyValue = sceneInfo.State.GetNodePropertyValue(nodeIndex, i);

            // Upgrade sub-resources that may have C# scripts attached.
            using Variant upgradedValue = UpgradeSubResources(propertyValue);

            destination.Set(propertyName, upgradedValue);
        }
    }

    private static int IndexOfNode(SceneInfo sceneInfo, Node node)
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
        try
        {
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
        }
        finally
        {
            // Unregister the C# script loader.
            ResourceLoader.Singleton.RemoveResourceFormatLoader(csharpScriptLoader);
        }
    }

    public void Upgrade()
    {
        // Register a C# script loader so we can parse old resources.
        using var csharpScriptLoader = new ResourceFormatLoaderCSharpScript();
        ResourceLoader.Singleton.AddResourceFormatLoader(csharpScriptLoader);
        try
        {
            if (_previewUpgradeEnabled)
            {
                // We only upgrade scenes and resources when an upgrade to Godot .NET should be performed.

                // Create all instances upfront to avoid 'missing class' errors.
                foreach (string filePath in _upgradableFilePaths)
                {
                    // IMPORTANT: We avoid caching the loaded resources so we can make sure everything is disposed
                    // by the time we finish the upgrade, so we can unregister the C# script and loader classes.
                    var resource = ResourceLoader.Singleton.Load(filePath, cacheMode: ResourceLoader.CacheMode.IgnoreDeep);
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

            UpgradeDotNetProject();
        }
        finally
        {
            // Unregister the C# script loader.
            ResourceLoader.Singleton.RemoveResourceFormatLoader(csharpScriptLoader);

            // Clear cached instances.

            foreach (var sceneInfo in _upgradableSceneInstances.Values)
            {
                sceneInfo.State.Dispose();
                sceneInfo.Node.Free();
            }
            foreach (var resource in _upgradableResourceInstances.Values)
            {
                resource.Dispose();
            }

            _upgradableSceneInstances.Clear();
            _upgradableResourceInstances.Clear();
        }
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
                rootNode = newRootNode;
            }
            scene.Pack(rootNode);
            if (rootNode != sceneInfo.Node)
            {
                // If the node was replaced, free the new root node after packing
                // so we don't leave nodes around.
                rootNode.Free();
            }
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
                childNode.Free();
            }
        }

        Script? script = originalNode.GetScriptAsScript();
        if (IsCSharpScript(script, out string? className))
        {
            return UpgradeNode(sceneInfo, originalNode, className);
        }

        // For non-C# nodes, we still need to upgrade sub-resources that might have C# scripts attached.
        int nodeIndex = IndexOfNode(sceneInfo, originalNode);
        if (nodeIndex != -1)
        {
            int propertyCount = sceneInfo.State.GetNodePropertyCount(nodeIndex);
            for (int i = 0; i < propertyCount; i++)
            {
                StringName propertyName = sceneInfo.State.GetNodePropertyName(nodeIndex, i);
                if (propertyName == PropertyName.Script)
                {
                    continue;
                }

                using Variant propertyValue = sceneInfo.State.GetNodePropertyValue(nodeIndex, i);
                using Variant newPropertyValue = UpgradeSubResources(propertyValue);
                if (!Variant.Evaluate(VariantOperator.Equal, propertyValue, newPropertyValue).AsBool())
                {
                    originalNode.Set(propertyName, newPropertyValue);
                }
            }
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

            using Variant propertyValue = originalResource.Get(property.Name);
            using Variant newPropertyValue = UpgradeSubResources(propertyValue);
            if (!Variant.Evaluate(VariantOperator.Equal, propertyValue, newPropertyValue).AsBool())
            {
                originalResource.Set(property.Name, newPropertyValue);
            }
        }

        Script? script = originalResource.GetScriptAsScript();
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
            {
                using var array = variant.AsGodotArray();
                for (int i = 0; i < array.Count; i++)
                {
                    using var upgraded = UpgradeSubResources(array[i]);
                    array[i] = upgraded;
                }
                break;
            }

            case VariantType.Dictionary:
            {
                using var dictionary = variant.AsGodotDictionary();
                foreach (var (key, value) in dictionary)
                {
                    dictionary.Remove(key);
                    using var newKey = UpgradeSubResources(key);
                    using var newValue = UpgradeSubResources(value);
                    dictionary[newKey] = newValue;
                }
                break;
            }

            case VariantType.Object:
            {
                using var @object = variant.AsGodotObject();
                if (@object is Resource resource)
                {
                    using Resource upgraded = UpgradeResource(resource, isRootResource: false);
                    return Variant.From(upgraded);
                }
                break;
            }
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

        CopyProperties(originalResource, newResource);

        return newResource;
    }

    private static void UpgradeDotNetProject()
    {
        const string ResultsFileName = "results.json";

        string godotDotNetUpgradeAssistantPath = Path.Join(EditorPath.EditorAssembliesPath, "Godot.UpgradeAssistant.Cli", "Godot.UpgradeAssistant.Cli.dll");

        string projectGodotFilePath = ProjectSettings.Singleton.GlobalizePath("res://project.godot");
        string projectDirectory = Path.GetDirectoryName(projectGodotFilePath)!;
        string projectName = EditorPath.ProjectAssemblyName;

        var outputDirectory = Directory.CreateTempSubdirectory(projectName);

        var startInfo = new ProcessStartInfo()
        {
            FileName = "dotnet",
            ArgumentList =
            {
                godotDotNetUpgradeAssistantPath,
                "upgrade",
                projectGodotFilePath,
                "--solution", EditorPath.ProjectSolutionPath,
                "--project", EditorPath.ProjectCSProjPath,
                "--target-godot-version", GodotBridge.GodotVersion.GetGodotDotNetVersion(),
                "--enable-preview",
                "--results-json", ResultsFileName,
            },
            WorkingDirectory = outputDirectory.FullName,
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
                CopyOutputToProjectDirectory(outputDirectory, projectDirectory);
            }
            else
            {
                if (NeedsToCopyOutputToProjectDirectory(outputDirectory))
                {
                    CopyOutputToProjectDirectory(outputDirectory, projectDirectory);
                }
            }
        }
        catch (Exception e)
        {
            GD.PushError($".NET failed to upgrade project '{projectName}'. Exception: {e}");
            CopyOutputToProjectDirectory(outputDirectory, projectDirectory);
        }

        static void CopyOutputToProjectDirectory(DirectoryInfo outputDirectory, string projectDirectory)
        {
            foreach (var file in outputDirectory.EnumerateFiles())
            {
                if (file.Name == ResultsFileName)
                {
                    // Don't copy the results file, this is just the machine-readable output
                    // that we use to determine what changes were made and if the output files
                    // need to be copied to the project directory.
                    continue;
                }

                string destinationPath = Path.Join(projectDirectory, file.Name);
                file.CopyTo(destinationPath, overwrite: true);
            }
        }

        static bool NeedsToCopyOutputToProjectDirectory(DirectoryInfo outputDirectory)
        {
            string resultsFilePath = Path.Join(outputDirectory.FullName, ResultsFileName);

            if (!File.Exists(resultsFilePath))
            {
                // If the results file doesn't exist, we have no way of knowing if we need to copy the output files.
                // So we copy just in case.
                return true;
            }

            using var stream = File.OpenRead(resultsFilePath);
            var results = JsonSerializer.Deserialize(stream, UpgradeResultsJsonContext.Default.UpgradeResults);
            if (results is null)
            {
                // Failed to deserialize results, so we'll copy just in case.
                return true;
            }

            // Copy the output files if there were any problems reported, regardless of whether
            // the fixes were applied or not, because we want the user to also see the summary
            // even if all problems were fixed. We just want to skip copying the output files
            // if there were no problems to fix, because it indicates the project was already
            // upgraded.
            return results.ProblemsReported > 0;
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

file static class Extensions
{
    public static Script? GetScriptAsScript(this GodotObject @object)
    {
        using Variant script = @object.GetScript();
        if (script.VariantType != VariantType.Object)
        {
            return null;
        }

        return script.AsGodotObject() as Script;
    }

    public static Script? GetNodePropertyAsScript(this SceneState sceneState, int nodeIndex, int propertyIndex)
    {
        using Variant propertyValue = sceneState.GetNodePropertyValue(nodeIndex, propertyIndex);
        if (propertyValue.VariantType != VariantType.Object)
        {
            return null;
        }

        return propertyValue.AsGodotObject() as Script;
    }
}
