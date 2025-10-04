using System;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Base configuration for all the assistant's steps.
/// </summary>
public class AssistantStepConfiguration
{
    /// <summary>
    /// Workspace that the step will be applied to.
    /// </summary>
    public required WorkspaceInfo Workspace { get; init; }

    /// <summary>
    /// The target Godot version that the workspace will be upgraded to.
    /// It must be within the range of <see cref="Constants.MinSupportedGodotVersion"/>
    /// and <see cref="Constants.LatestGodotVersion"/>.
    /// </summary>
    public required SemVer TargetGodotVersion { get; init; }

    /// <summary>
    /// Indicates whether the Godot .NET packages should be used instead of GodotSharp
    /// for a target Godot version that supports both.
    /// </summary>
    /// <remarks>
    /// This option maps directly to the CLI arguments provided by the user, to determine
    /// whether the Godot .NET packages should be used see <see cref="IsGodotDotNetEnabled"/>.
    /// </remarks>
    public required bool EnableGodotDotNetPreview { get; init; }

    /// <summary>
    /// Indicates if the target should use the new Godot .NET bindings.
    ///
    ///   1) If the target Godot version doesn't support the Godot .NET bindings,
    ///      it will always be <see langword="false"/>.
    ///   2) If the target Godot version only supports the Godot .NET bindings,
    ///      it will always be <see langword="true"/>.
    ///   3) If the target Godot version supports both the Godot .NET bindings and the GodotSharp bindings,
    ///      it will be <see langword="false"/> unless the user explicitly requested the Godot .NET bindings.
    /// </summary>
    public bool IsGodotDotNetEnabled
    {
        get
        {
            // Check if the Godot .NET preview is supported for the target Godot version.
            if (EnableGodotDotNetPreview)
            {
                if (TargetGodotVersion < Constants.FirstSupportedGodotDotNetVersion)
                {
                    throw new InvalidOperationException(SR.FormatInvalidOperation_GodotDotNetPreviewUnsupportedInTargetGodotVersion(TargetGodotVersion));
                }
                if (TargetGodotVersion > Constants.LastSupportedGodotSharpVersion)
                {
                    throw new InvalidOperationException(SR.FormatInvalidOperation_GodotDotNetPreviewIsOutOfPreviewInTargetGodotVersion(TargetGodotVersion));
                }

                // Since the preview is explicitly requested and the target version supports it,
                // the Godot .NET packages are enabled.
                return true;
            }

            return TargetGodotVersion > Constants.LastSupportedGodotSharpVersion;
        }
    }
}
