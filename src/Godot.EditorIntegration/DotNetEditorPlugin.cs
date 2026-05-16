using System;
using System.IO;
using System.Linq;
using System.Threading;
using Godot.Collections;
using Godot.EditorIntegration.Build;
using Godot.EditorIntegration.Build.Cli;
using Godot.EditorIntegration.Build.UI;
using Godot.EditorIntegration.CodeEditors;
using Godot.EditorIntegration.Export;
using Godot.EditorIntegration.Internals;
using Godot.EditorIntegration.ProjectEditor;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace Godot.EditorIntegration;

[GodotClass]
internal sealed partial class DotNetEditorPlugin : EditorPlugin
{
    private static DotNetEditorPlugin? _singleton;
    public static DotNetEditorPlugin Singleton => _singleton ?? throw new InvalidOperationException(SR.FormatInvalidOperation_AnInstanceDoesNotExist(typeof(DotNetEditorPlugin)));

#nullable disable
    private EditorSettings _editorSettings;

    private ConfirmationDialog _confirmCreateSlnDialog;

    private StatusIndicatorPanel _statusIndicatorPanel;

    private MSBuildPanel _msbuildPanel;

    private Button _toolBarBuildButton;

    private DotNetExportPlugin _exportPlugin;

    private DotNetEditorExtensionSourceCodePlugin _sourceCodePlugin;

    public CodeEditorManagers CodeEditorManager { get; private set; }
#nullable enable

    protected override string _GetPluginName() => ".NET";

    internal bool CreateProjectSolution()
    {
        string? errorMessage = EditorProgress.Invoke("create_csharp_solution", SR.DotNetEditorPlugin_GenerateSolutionEditorProgressLabel, 2, progress =>
        {
            progress.Step(SR.DotNetEditorPlugin_GenerateSolutionEditorProgressStep, 0);

            string csprojDir = Path.GetDirectoryName(EditorPath.ProjectCSProjPath)!;
            string solutionDir = Path.GetDirectoryName(EditorPath.ProjectSolutionPath)!;
            string name = EditorPath.ProjectAssemblyName;

            try
            {
                var msbuildProject = ProjectUtils.GenerateProject(name);
                msbuildProject.Save(Path.Join(csprojDir, $"{name}.csproj"));
            }
            catch (IOException e)
            {
                return SR.FormatDotNetEditorPlugin_CreateCSharpProjectFailed(e.Message);
            }

            progress.Step(SR.DotNetEditorPlugin_GenerateSolutionEditorProgressStep, 1);

            var solutionModel = new SolutionModel();
            solutionModel.AddPlatform("Any CPU");
            solutionModel.AddBuildType("Debug");
            solutionModel.AddBuildType("ExportDebug");
            solutionModel.AddBuildType("ExportRelease");
            solutionModel.AddProject(Path.GetRelativePath(solutionDir, EditorPath.ProjectCSProjPath));

            try
            {
                string solutionMoniker = EditorPath.ProjectSolutionPath;
                SolutionSerializers.SlnFileV12.SaveAsync(solutionMoniker, solutionModel, CancellationToken.None).Wait();
            }
            catch (IOException e)
            {
                return SR.FormatDotNetEditorPlugin_SaveSolutionFailed(e.Message);
            }

            return null;
        });

        if (!string.IsNullOrEmpty(errorMessage))
        {
            EditorInternal.ShowWarning(errorMessage, SR.DotNetEditorPlugin_AlertTitleError);
            return false;
        }

        // Show .NET features.
        _msbuildPanel.Open();
        _toolBarBuildButton.Show();

        return true;
    }

    private static void ApplyNecessaryChangesToSolution()
    {
        try
        {
            var msbuildProject = MSBuildProject.Open(EditorPath.ProjectCSProjPath);
            if (msbuildProject is null)
            {
                throw new InvalidOperationException(SR.DotNetEditorPlugin_InvalidOperation_CannotOpenCSharpProject);
            }

            // NOTE: The order in which changes are made to the project is important.

            msbuildProject.EnsureGodotSdkIsUpToDate();

            if (msbuildProject.HasUnsavedChanges)
            {
                msbuildProject.Save();
            }
        }
        catch (Exception e)
        {
            GD.PushError(e.ToString());
        }
    }

    private enum MenuOptions
    {
        CreateSln,
    }

    private void MenuOptionPressed(long id)
    {
        switch ((MenuOptions)id)
        {
            case MenuOptions.CreateSln:
            {
                if (File.Exists(EditorPath.ProjectSolutionPath) || File.Exists(EditorPath.ProjectCSProjPath))
                {
                    ShowConfirmCreateSlnDialog();
                }
                else
                {
                    CreateProjectSolution();
                }
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, SR.ArgumentOutOfRange_InvalidMenuOption);
            }
        }
    }

    public void ShowConfirmCreateSlnDialog()
    {
        _confirmCreateSlnDialog.Title = SR.DotNetEditorPlugin_CreateCSharpSolution;
        _confirmCreateSlnDialog.DialogText = SR.DotNetEditorPlugin_CSharpSolutionAlreadyExists;
        EditorInterface.Singleton.PopupDialogCentered(_confirmCreateSlnDialog);
    }

    public void BuildProjectPressed()
    {
        if (!File.Exists(EditorPath.ProjectCSProjPath))
        {
            if (!CreateProjectSolution())
            {
                // Failed to create project.
                return;
            }
        }

        BuildCore();
    }

    protected override bool _Build()
    {
        return BuildCore();
    }

    private static bool BuildCore()
    {
        string projectPath = EditorPath.ProjectCSProjPath;

        if (!File.Exists(projectPath))
        {
            // No project to build.
            return true;
        }

        return BuildManager.BuildProjectBlocking(new BuildOptions()
        {
            SlnOrProject = projectPath,
        }
            .WithGodotDebugDefaults());
    }

    public void MakeMSBuildPanelVisible()
    {
        _msbuildPanel.MakeVisible();
    }

    protected override void _EnterTree()
    {
        if (_singleton is not null)
        {
            throw new InvalidOperationException(SR.FormatInvalidOperation_AnInstanceAlreadyExists(nameof(DotNetEditorPlugin)));
        }
        _singleton = this;

        ProjectSettings.Singleton.SettingsChanged += EditorPath.InvalidateCachedDirectories;

        // Register MSBuildLocator defaults so ProjectUtils can create/edit projects.
        // This must be done before ProjectUtils is used for the first time.
        if (ProjectUtils.MSBuildLocatorTryRegisterDefaults(out string? sdkVersion, out string? sdkPath))
        {
            EditorIntegrationState.SetDotNetSdkInfo(sdkVersion, sdkPath);
        }
        else
        {
            EditorInternal.ModuleFailInitialization(SR.DotNetEditorPlugin_DotNetSdkNotFound);
            return;
        }

        _editorSettings = EditorInterface.Singleton.GetEditorSettings();

        _confirmCreateSlnDialog = new ConfirmationDialog();
        _confirmCreateSlnDialog.SetUnparentWhenInvisible(true);
        _confirmCreateSlnDialog.Confirmed += () => CreateProjectSolution();

        // MSBuild panel.

        _msbuildPanel = new MSBuildPanel();
        AddDock(_msbuildPanel);

        // Create solution command.

        EditorInterface.Singleton.GetCommandPalette().AddCommand(SR.DotNetEditorPlugin_CreateCSharpSolution, "dotnet/create_solution", Callable.From(() =>
        {
            if (File.Exists(EditorPath.ProjectSolutionPath) || File.Exists(EditorPath.ProjectCSProjPath))
            {
                ShowConfirmCreateSlnDialog();
            }
            else
            {
                CreateProjectSolution();
            }
        }));

        // .NET build button.

        _toolBarBuildButton = new Button()
        {
            Flat = false,
            Icon = EditorInterface.Singleton.GetEditorTheme().GetIcon(EditorThemeNames.BuildDotNet, EditorThemeNames.EditorIcons),
            FocusMode = Control.FocusModeEnum.None,
            Shortcut = EditorInternal.EditorDefineShortcut(EditorShortcutNames.BuildSolution, SR.MSBuildPanel_BuildProject, EditorShortcutKeycodes.BuildProject),
            ShortcutInTooltip = true,
            ThemeTypeVariation = EditorThemeNames.RunBarButton,
        };
        EditorInternal.EditorShortcutOverride(EditorShortcutNames.BuildSolution, "macos", (Key)KeyModifierMask.MaskMeta | (Key)KeyModifierMask.MaskCtrl | Key.B);
        EditorInterface.Singleton.GetCommandPalette().AddCommand(SR.DotNetEditorPlugin_BuildProject, EditorShortcutNames.BuildSolution, Callable.From(BuildProjectPressed), _toolBarBuildButton.Shortcut.GetAsText());

        _toolBarBuildButton.Pressed += BuildProjectPressed;
        EditorInternal.AddControlToEditorRunBar(_toolBarBuildButton);
        // Move Build button so it appears to the left of the Play button.
        _toolBarBuildButton.GetParent().MoveChild(_toolBarBuildButton, 0);

        if (File.Exists(EditorPath.ProjectCSProjPath))
        {
            ApplyNecessaryChangesToSolution();
        }
        else
        {
            _msbuildPanel.Close();
            _toolBarBuildButton.Hide();
        }

        // External editor settings.
        EditorInternal.EditorDefineSetting(EditorSettingNames.ExternalEditor, Variant.From(CodeEditorId.None));
        EditorInternal.EditorDefineSetting(EditorSettingNames.CustomExecPath, "");
        EditorInternal.EditorDefineSetting(EditorSettingNames.CustomExecPathArgs, "");
        EditorInternal.EditorDefineSetting(EditorSettingNames.VerbosityLevel, Variant.From(VerbosityOption.Normal));
        EditorInternal.EditorDefineSetting(EditorSettingNames.NoConsoleLogging, false);
        EditorInternal.EditorDefineSetting(EditorSettingNames.CreateBinaryLog, false);
        EditorInternal.EditorDefineSetting(EditorSettingNames.ProblemsLayout, Variant.From(BuildProblemsView.ProblemsLayout.Tree));

        string settingsHintStr = SR.CodeEditorDisabled;

        if (OperatingSystem.IsWindows())
        {
            settingsHintStr += $",{SR.CodeEditorVisualStudio}:{CodeEditorId.VisualStudio:D}" +
                               $",{SR.CodeEditorVisualStudioCode}:{CodeEditorId.VSCode:D}" +
                               $",{SR.CodeEditorJetBrainsRider}:{CodeEditorId.Rider:D}" +
                               $",{SR.CodeEditorCustom}:{CodeEditorId.CustomEditor:D}";
        }
        else if (OperatingSystem.IsMacOS())
        {
            settingsHintStr += $",{SR.CodeEditorVisualStudioCode}:{CodeEditorId.VSCode:D}" +
                               $",{SR.CodeEditorJetBrainsRider}:{CodeEditorId.Rider:D}" +
                               $",{SR.CodeEditorCustom}:{CodeEditorId.CustomEditor:D}";
        }
        else if (OperatingSystem.IsLinux())
        {
            settingsHintStr += $",{SR.CodeEditorVisualStudioCode}:{CodeEditorId.VSCode:D}" +
                               $",{SR.CodeEditorJetBrainsRider}:{CodeEditorId.Rider:D}" +
                               $",{SR.CodeEditorCustom}:{CodeEditorId.CustomEditor:D}";
        }

        _editorSettings.AddPropertyInfo(new GodotDictionary()
        {
            ["type"] = (int)VariantType.Int,
            ["name"] = EditorSettingNames.ExternalEditor,
            ["hint"] = (int)PropertyHint.Enum,
            ["hint_string"] = settingsHintStr,
        });

        _editorSettings.AddPropertyInfo(new GodotDictionary()
        {
            ["type"] = (int)VariantType.String,
            ["name"] = EditorSettingNames.CustomExecPath,
            ["hint"] = (int)PropertyHint.GlobalFile,
        });

        _editorSettings.AddPropertyInfo(new GodotDictionary()
        {
            ["type"] = (int)VariantType.String,
            ["name"] = EditorSettingNames.CustomExecPathArgs,
        });
        _editorSettings.SetInitialValue(new StringName(EditorSettingNames.CustomExecPathArgs), "{file}", updateCurrent: false);

        var verbosityLevels = Enum.GetValues<VerbosityOption>().Select(level => $"{level}:{level:D}");
        _editorSettings.AddPropertyInfo(new GodotDictionary()
        {
            ["type"] = (int)VariantType.Int,
            ["name"] = EditorSettingNames.VerbosityLevel,
            ["hint"] = (int)PropertyHint.Enum,
            ["hint_string"] = string.Join(',', verbosityLevels),
        });

        _editorSettings.AddPropertyInfo(new GodotDictionary()
        {
            ["type"] = (int)VariantType.Int,
            ["name"] = EditorSettingNames.ProblemsLayout,
            ["hint"] = (int)PropertyHint.Enum,
            ["hint_string"] = $"{SR.MSBuildPanel_ViewAsAList},{SR.MSBuildPanel_ViewAsATree}",
        });

        // Export plugin.
        _exportPlugin = new DotNetExportPlugin();
        AddExportPlugin(_exportPlugin);

        // Source code plugin.
        _sourceCodePlugin = new DotNetEditorExtensionSourceCodePlugin();

        // Status indicator panel.
        _statusIndicatorPanel = new StatusIndicatorPanel();
        _statusIndicatorPanel.Workspace = _sourceCodePlugin.Workspace;

        CodeEditorManager = new CodeEditorManagers();

        EditorInternal.ModuleCompleteInitialization();

        // Handle .NET workspace initialization state.
        ProjectSettings.Singleton.SettingsChanged += UpdateWorkspaceProjectPath;
    }

    private void UpdateWorkspaceProjectPath()
    {
        PackedStringArray changedSettings = ProjectSettings.Singleton.GetChangedSettings();

        bool shouldUpdate = false;
        foreach (string setting in changedSettings)
        {
            if (setting.StartsWith("dotnet/", StringComparison.Ordinal))
            {
                shouldUpdate = true;
                break;
            }
        }
        if (!shouldUpdate)
        {
            // No .NET-related settings were changed, so we can ignore this.
            return;
        }

        _sourceCodePlugin.Workspace.UpdateProjectPath(EditorPath.ProjectCSProjPath);
    }

    protected override void _ExitTree()
    {
        ProjectSettings.Singleton.SettingsChanged -= UpdateWorkspaceProjectPath;
        ProjectSettings.Singleton.SettingsChanged -= EditorPath.InvalidateCachedDirectories;

        CodeEditorManager?.Dispose();

        // Export plugin.
        if (_exportPlugin is not null)
        {
            RemoveExportPlugin(_exportPlugin);
            _exportPlugin.Dispose();
        }

        // Status indicator panel.
        _statusIndicatorPanel?.QueueFree();

        // Source code plugin.
        _sourceCodePlugin?.Dispose();

        // .NET build button.
        _toolBarBuildButton?.QueueFree();

        // MSBuild panel.
        _msbuildPanel?.QueueFree();

        _confirmCreateSlnDialog?.QueueFree();

        _singleton = null;
    }
}
