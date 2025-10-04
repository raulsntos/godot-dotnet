using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;

namespace Godot.UpgradeAssistant;

/// <summary>
/// Describes a .NET workspace, including the solution and project files contained within.
/// </summary>
public abstract class DotNetWorkspaceInfo : IDisposable
{
    /// <summary>
    /// The MSBuild workspace for the Godot project.
    /// </summary>
    public abstract Workspace Workspace { get; }

    /// <summary>
    /// The loaded solution for the Godot project.
    /// </summary>
    public Solution Solution => Workspace.CurrentSolution;

    /// <summary>
    /// The loaded project that represents the Godot project in the solution.
    /// </summary>
    public abstract Project Project { get; }

    /// <summary>
    /// Open a <see cref="ProjectRootElement"/> from the project that represents
    /// the Godot project in the solution.
    /// </summary>
    /// <returns>The opened project.</returns>
    public abstract ProjectRootElement OpenProjectRootElement();

    /// <summary>
    /// Save the <see cref="ProjectRootElement"/> for the project that represents
    /// the Godot project in the solution.
    /// </summary>
    /// <param name="projectRootElement">The modified project root element.</param>
    public abstract void SaveProjectRootElement(ProjectRootElement projectRootElement);

    /// <summary>
    /// Open a <see cref="SolutionModel"/> for the solution that contains the Godot project.
    /// </summary>
    /// <param name="solutionSerializer">
    /// The serializer used to open the solution. It will be determined from the solution moniker
    /// and can be used to save the solution so it's serialized using the same format.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    /// <returns>The opened solution model.</returns>
    public abstract Task<SolutionModel> OpenSolutionModelAsync(out ISolutionSerializer solutionSerializer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save the <see cref="SolutionModel"/> for the solution that contains the Godot project.
    /// </summary>
    /// <param name="solutionModel">The modified solution model.</param>
    /// <param name="solutionSerializer">The serializer used to save the solution.</param>
    /// <param name="cancellationToken">
    /// Optional token to cancel the asynchronous operation.
    /// </param>
    public abstract Task SaveSolutionModelAsync(SolutionModel solutionModel, ISolutionSerializer solutionSerializer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to open the global.json file for the project as a writable stream.
    /// </summary>
    /// <param name="stream">The stream for the global.json file, if it exists.</param>
    /// <returns>Whether the global.json file was successfully opened.</returns>
    public abstract bool TryOpenGlobalJsonStream([NotNullWhen(true)] out Stream? stream);

    /// <summary>
    /// Opens the global.json file for the project as a writable stream.
    /// </summary>
    /// <exception cref="InvalidOperationException">The global.json file does not exist.</exception>
    /// <returns>The stream for the global.json file, if it exists.</returns>
    public Stream OpenGlobalJsonStream()
    {
        if (!TryOpenGlobalJsonStream(out var stream))
        {
            throw new InvalidOperationException(SR.InvalidOperation_GlobalJsonFileNotAvailable);
        }

        return stream;
    }

    /// <summary>
    /// Retrieve the <c>Godot.NET.Sdk</c> version from the <c>.csproj</c> or <c>global.json</c> files.
    /// </summary>
    /// <param name="sdkVersion">The version retrieved.</param>
    /// <returns>Whether we could successfully retrieve the version of the SDK.</returns>
    public bool TryGetGodotSdkVersion(out SemVer sdkVersion)
    {
        Stream? globalJsonStream = null;
        try
        {
            JsonDocument? globalJson = null;
            if (TryOpenGlobalJsonStream(out globalJsonStream))
            {
                globalJson = GlobalJson.ParseAsDocument(globalJsonStream);
            }

            return TryGetGodotSdkVersion(OpenProjectRootElement(), globalJson, out sdkVersion);
        }
        finally
        {
            globalJsonStream?.Dispose();
        }
    }

    internal static bool TryGetGodotSdkVersion(ProjectRootElement projectRoot, JsonDocument? globalJson, out SemVer sdkVersion)
    {
        string sdk = projectRoot.Sdk;

        if (!sdk.StartsWith(Constants.GodotSdkAssemblyName, StringComparison.OrdinalIgnoreCase))
        {
            // An SDK other than Godot.NET.Sdk is not supported.
            sdkVersion = default;
            return false;
        }

        int idx = sdk.IndexOf('/');
        if (idx == -1 && globalJson is not null)
        {
            // A non-versioned Godot.NET.Sdk is not supported, unless the version is specified in 'global.json'.
            return TryFindSdkVersionFromGlobalJson(globalJson, out sdkVersion);
        }

        // If we couldn't parse the SDK version, assume unsupported.
        return SemVer.TryParse(sdk.AsSpan(idx + 1), out sdkVersion);

        static bool TryFindSdkVersionFromGlobalJson(JsonDocument document, out SemVer sdkVersion)
        {
            sdkVersion = default;

            var root = document.RootElement;
            foreach (var rootElement in root.EnumerateObject())
            {
                if (rootElement.Name != "msbuild-sdks")
                {
                    continue;
                }

                foreach (var sdkElement in rootElement.Value.EnumerateObject())
                {
                    if (sdkElement.Name != "Godot.NET.Sdk")
                    {
                        continue;
                    }

                    return SemVer.TryParse(sdkElement.Value.GetString(), out sdkVersion);
                }
            }

            return false;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the workspace info.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Workspace?.Dispose();
        }
    }
}
