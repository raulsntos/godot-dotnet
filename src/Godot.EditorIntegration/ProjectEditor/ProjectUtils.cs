using System;
using Godot.Bridge;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace Godot.EditorIntegration.ProjectEditor;

internal static class ProjectUtils
{
    private const string GodotMSBuildSdk = "Godot.NET.Sdk";

    public static void MSBuildLocatorRegisterDefaults()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public static MSBuildProject GenerateProject(string projectName)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        var root = ProjectRootElement.Create(NewProjectFileOptions.None);

        root.Sdk = $"{GodotMSBuildSdk}/{GodotBridge.GodotVersion.GetGodotDotNetVersion()}";

        var mainGroup = root.AddPropertyGroup();
        mainGroup.AddProperty("TargetFramework", "net9.0");

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
        string godotSdkAttrValue = $"{GodotMSBuildSdk}/{GodotBridge.GodotVersion.GetGodotDotNetVersion()}";

        string rootSdk = root.Sdk?.Trim() ?? string.Empty;

        if (rootSdk.Equals(godotSdkAttrValue, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        root.Sdk = godotSdkAttrValue;
        project.HasUnsavedChanges = true;
    }
}
