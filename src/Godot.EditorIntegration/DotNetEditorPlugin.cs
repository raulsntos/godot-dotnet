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
using Godot.EditorIntegration.UpgradeAssistant;
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

    private PopupMenu _toolsMenuPopup;

    private MSBuildPanel _msbuildPanel;

    private Button _msbuildPanelButton;
    private Button _toolBarBuildButton;

    private DotNetExportPlugin _exportPlugin;

    public CodeEditorManagers CodeEditorManager { get; private set; }
#nullable enable

    protected override string _GetPluginName() => ".NET";

    private bool CreateProjectSolution()
    {
        string? errorMessage = EditorProgress.Invoke("create_csharp_solution", SR.DotNetEditorPlugin_GenerateSolutionEditorProgressLabel, 2, progress =>
        {
            progress.Step(SR.DotNetEditorPlugin_GenerateSolutionEditorProgressStep);

            string csprojDir = Path.GetDirectoryName(EditorPath.ProjectCSProjPath)!;
            string slnDir = Path.GetDirectoryName(EditorPath.ProjectSlnPath)!;
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

            var solutionModel = new SolutionModel();
            solutionModel.AddPlatform("Any CPU");
            solutionModel.AddBuildType("Debug");
            solutionModel.AddBuildType("ExportDebug");
            solutionModel.AddBuildType("ExportRelease");
            solutionModel.AddProject(Path.GetRelativePath(slnDir, EditorPath.ProjectCSProjPath));

            try
            {
                string solutionMoniker = Path.Join(slnDir, $"{name}.sln");
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
        _msbuildPanelButton.Show();
        _toolBarBuildButton.Show();

        return true;
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
                if (File.Exists(EditorPath.ProjectSlnPath) || File.Exists(EditorPath.ProjectCSProjPath))
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

    private void BuildProjectPressed()
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

    private void UpdateMSBuildPanelButtonIcon()
    {
        if (_msbuildPanelButton is not null)
        {
            _msbuildPanelButton.Icon = _msbuildPanel.GetBuildStateIcon();
        }
    }

    public void MakeMSBuildPanelVisible()
    {
        MakeBottomPanelItemVisible(_msbuildPanel);
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
        ProjectUtils.MSBuildLocatorRegisterDefaults();

        _editorSettings = EditorInterface.Singleton.GetEditorSettings();

        _confirmCreateSlnDialog = new ConfirmationDialog();
        _confirmCreateSlnDialog.SetUnparentWhenInvisible(true);
        _confirmCreateSlnDialog.Confirmed += () => CreateProjectSolution();

        // MSBuild panel.

        _msbuildPanel = new MSBuildPanel();
        _msbuildPanel.BuildStateChanged += UpdateMSBuildPanelButtonIcon;
        _msbuildPanelButton = AddControlToBottomPanel(_msbuildPanel, "MSBuild");

        // .NET tools menu.

        _toolsMenuPopup = new PopupMenu();
        _toolsMenuPopup.AddItem(SR.DotNetEditorPlugin_CreateCSharpSolution, (int)MenuOptions.CreateSln);
        _toolsMenuPopup.IdPressed += MenuOptionPressed;

        AddToolSubmenuItem(".NET/C#", _toolsMenuPopup);

        // .NET build button.

        _toolBarBuildButton = new Button()
        {
            Flat = false,
            Icon = EditorInterface.Singleton.GetEditorTheme().GetIcon(EditorThemeNames.BuildDotNet, EditorThemeNames.EditorIcons),
            FocusMode = Control.FocusModeEnum.None,
            Shortcut = EditorInternal.EditorDefineShortcut(EditorShortcutNames.BuildSolution, SR.MSBuildPanel_BuildProject, (Key)KeyModifierMask.MaskAlt | Key.B),
            ShortcutInTooltip = true,
            ThemeTypeVariation = EditorThemeNames.RunBarButton,
        };
        EditorInternal.EditorShortcutOverride(EditorShortcutNames.BuildSolution, "macos", (Key)KeyModifierMask.MaskMeta | (Key)KeyModifierMask.MaskCtrl | Key.B);

        _toolBarBuildButton.Pressed += BuildProjectPressed;
        EditorInternal.AddControlToEditorRunBar(_toolBarBuildButton);
        // Move Build button so it appears to the left of the Play button.
        _toolBarBuildButton.GetParent().MoveChild(_toolBarBuildButton, 0);

        if (File.Exists(EditorPath.ProjectCSProjPath))
        {
            // TODO(@raulsntos): The time at which the upgrade assistant is executed is important, it needs to run early enough to prevent showing errors in the console about broken scenes/resources with attached `.cs` files, but not too early otherwise the scenes/resources fail to load. This also means this process needs to block the editor from continuing loading the project until the upgrade is done, so using EditorProgress doesn't seem to be possible (although it'd be nice to have a progress bar).
            // EditorProgress.Invoke("upgrade_dotnet_project", SR.DotNetEditorPlugin_UpgradeSolutionEditorProgressLabel, 3, progress =>
            {
                var upgradeAssistant = new GodotUpgradeAssistant();

                // progress.Step(SR.DotNetEditorPlugin_UpgradeSolutionEditorProgressStep_Prepare);
                upgradeAssistant.Prepare();

                // progress.Step(SR.DotNetEditorPlugin_UpgradeSolutionEditorProgressStep_Upgrade);
                upgradeAssistant.Upgrade();
            }//);
        }
        else
        {
            _msbuildPanelButton.Hide();
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

        CodeEditorManager = new CodeEditorManagers();
    }

    protected override void _ExitTree()
    {
        CodeEditorManager?.Dispose();

        // Export plugin.
        RemoveExportPlugin(_exportPlugin);
        _exportPlugin.Dispose();

        // .NET build button.
        _toolBarBuildButton?.QueueFree();

        // .NET tools menu.
        _toolsMenuPopup?.QueueFree();

        // MSBuild panel.
        _msbuildPanelButton?.QueueFree();
        _msbuildPanel?.QueueFree();

        _confirmCreateSlnDialog?.QueueFree();

        ProjectSettings.Singleton.SettingsChanged -= EditorPath.InvalidateCachedDirectories;

        _singleton = null;
    }
}
