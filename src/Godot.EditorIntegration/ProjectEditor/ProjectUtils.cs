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

    public static ProjectRootElement GenerateProject(string projectName)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        var root = ProjectRootElement.Create(NewProjectFileOptions.None);

        root.Sdk = $"{GodotMSBuildSdk}/{GodotBridge.GodotVersion.GetGodotDotNetVersion()}";

        var mainGroup = root.AddPropertyGroup();
        mainGroup.AddProperty("TargetFramework", "net9.0");

        mainGroup.AddProperty("EnableDynamicLoading", "true");

        mainGroup.AddProperty("EnableGodotDotNetPreview", "true");

        string sanitizedName = IdentifierUtils.SanitizeQualifiedIdentifier(projectName);

        // If the name is not a valid namespace, manually set RootNamespace to a sanitized one.
        if (sanitizedName != projectName)
        {
            mainGroup.AddProperty("RootNamespace", sanitizedName);
        }

        return root;
    }
}
