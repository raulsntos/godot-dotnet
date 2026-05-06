using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Godot.Tasks;

/// <summary>
/// MSBuild task to prepare GDExtensions used by the project for binding generation.
/// </summary>
public class PrepareGDExtensionsTask : Task
{
    /// <summary>
    /// Specifies additional GDExtension files to be considered.
    /// </summary>
    [Required]
    public required ITaskItem[] GDExtensionFiles { get; set; }

    /// <summary>
    /// Path to the intermediate output directory where the generated source files
    /// will be written.
    /// </summary>
    [Output]
    public ITaskItem[] ExtensionApiPaths { get; set; } = [];

    /// <summary>
    /// Execute the MSBuild task.
    /// </summary>
    /// <returns><see langword="true"/> if the task was successful.</returns>
    public override bool Execute()
    {
        try
        {
            List<ITaskItem> extensionApiPaths = [];

            HashSet<string> visitedExtensions = [];
            foreach (ITaskItem gdextensionFile in GDExtensionFiles)
            {
                string gdextensionFilePath = gdextensionFile.ItemSpec;
                if (!visitedExtensions.Add(gdextensionFilePath))
                {
                    continue;
                }

                // For each .gdextension file found, there should be a corresponding extension API dump JSON file.
                // We assume the JSON file is in the same directory as the .gdextension file but the name is always
                // 'extension_api.json'.
                string extensionApiPath = Path.Combine(Path.GetDirectoryName(gdextensionFilePath)!, "extension_api.json");
                if (File.Exists(extensionApiPath))
                {
                    var item = new TaskItem(extensionApiPath);

                    // We let the user specify an optional "ExtensionName" metadata on the .gdextension item,
                    // but fall back to the name of the .gdextension file if not provided.
                    string? extensionName = gdextensionFile.GetMetadata("ExtensionName");
                    if (string.IsNullOrEmpty(extensionName))
                    {
                        extensionName = Path.GetFileNameWithoutExtension(gdextensionFilePath);
                    }

                    // TODO(@raulsntos): We should probably sanitize the 'extensionName' here to ensure it's a valid namespace identifier since it will be used in the generated source code.

                    item.SetMetadata("ExtensionName", extensionName);

                    extensionApiPaths.Add(item);

                    Log.LogMessage(MessageImportance.Low, $"Found GDExtension API dump: '{extensionApiPath}'.");
                }
                else
                {
                    Log.LogMessage(MessageImportance.Low, $"Found .gdextension file '{gdextensionFilePath}' but corresponding API dump file '{extensionApiPath}' does not exist. Skipping this GDExtension.");
                }
            }

            ExtensionApiPaths = extensionApiPaths.ToArray();

            return true;
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to find GDExtensions: {e}");
            return false;
        }
    }
}
