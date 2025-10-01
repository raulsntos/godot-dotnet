using System;
using System.Diagnostics.CodeAnalysis;
using Godot.Bridge;
using Godot.Common.CodeAnalysis;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace Godot.EditorIntegration.ProjectEditor;

internal static class ProjectUtils
{
    private const string GodotMSBuildSdk = "Godot.NET.Sdk";

    public static bool MSBuildLocatorTryRegisterDefaults([NotNullWhen(true)] out string? version, [NotNullWhen(true)] out string? path)
    {
        try
        {
            var instance = MSBuildLocator.RegisterDefaults();
            version = instance.Version.ToString();
            path = instance.MSBuildPath;
            return true;
        }
        catch
        {
            // We could not find a valid MSBuild instance.
            version = null;
            path = null;
            return false;
        }
    }

    public static ProjectRootElement GenerateProject(string projectName)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        var root = ProjectRootElement.Create(NewProjectFileOptions.None);

        root.Sdk = $"{GodotMSBuildSdk}/{GodotBridge.GodotVersion.GetGodotDotNetVersion()}";

        var mainGroup = root.AddPropertyGroup();
        mainGroup.AddProperty("TargetFramework", "net10.0");
        mainGroup.AddProperty("EnableDynamicLoading", "true");
        mainGroup.AddProperty("EnableGodotDotNetPreview", "true");

        string sanitizedName = IdentifierUtils.SanitizeName(projectName);

        // If the name is not a valid namespace, manually set RootNamespace to a sanitized one.
        if (sanitizedName != projectName)
        {
            mainGroup.AddProperty("RootNamespace", sanitizedName);
        }

        return root;
    }
}
