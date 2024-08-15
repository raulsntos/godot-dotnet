using System;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace Godot.EditorIntegration.ProjectEditor;

internal static class ProjectUtils
{
    // TODO: The Godot.NET.Sdk is in the main repo (https://github.com/godotengine/godot)
    // and we're currently hardcoding the latest version, make sure to keep it in sync.
    private static string GodotSdkAttrValue => $"Godot.NET.Sdk/4.4.0-dev";

    public static void MSBuildLocatorRegisterDefaults()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public static MSBuildProject GenerateProject(string projectName)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        var root = ProjectRootElement.Create(NewProjectFileOptions.None);

        root.Sdk = GodotSdkAttrValue;

        var mainGroup = root.AddPropertyGroup();
        mainGroup.AddProperty("TargetFramework", "net8.0");

        mainGroup.AddProperty("EnableDynamicLoading", "true");

        string sanitizedName = IdentifierUtils.SanitizeQualifiedIdentifier(projectName);

        // If the name is not a valid namespace, manually set RootNamespace to a sanitized one.
        if (sanitizedName != projectName)
        {
            mainGroup.AddProperty("RootNamespace", sanitizedName);
        }

        return new MSBuildProject(root);
    }

    public static void EnsureGodotSdkIsUpToDate(this MSBuildProject project)
    {
        var root = project.Root;
        string godotSdkAttrValue = GodotSdkAttrValue;

        string rootSdk = root.Sdk?.Trim() ?? string.Empty;

        if (rootSdk.Equals(godotSdkAttrValue, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        root.Sdk = godotSdkAttrValue;
        project.HasUnsavedChanges = true;
    }
}
