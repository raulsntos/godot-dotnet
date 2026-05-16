using System;
using System.IO;
using System.Linq;
using Godot.EditorIntegration.Internals;
using Godot.EditorIntegration.Workspace;

namespace Godot.EditorIntegration;

[GodotClass]
internal sealed partial class StatusIndicatorPanel : VBoxContainer
{
    private PopupPanel? _panelParent;

#nullable disable
    private Label _dotnetSdkInfoLabel;
    private LinkButton _dotnetSdkInfoButton;

    private Label _workspaceInfoLabel;
    private TextureRect _workspaceInfoIcon;

    private Label _assemblyInfoLabel;
    private TextureRect _assemblyInfoIcon;

    private LinkButton _selectProjectButton;
    private ProjectSelectorDialog _projectSelectorDialog;

    private LinkButton _buildButton;
#nullable restore

    private DotNetWorkspace? _workspace;

    public DotNetWorkspace? Workspace
    {
        get => _workspace;
        set
        {
            _workspace = value;
            EditorInternal.StatusIndicatorNotifyStateChanged();
        }
    }

    public StatusIndicatorPanel()
    {
        EditorInternal.StatusPanelSetContent(this);
    }

    protected override void _Ready()
    {
        _projectSelectorDialog = new ProjectSelectorDialog();

        // Find the parent PopupPanel to be able to hide it as a response to certain actions.
        {
            var current = GetParent();
            while (current is not null)
            {
                if (current is PopupPanel popupPanel)
                {
                    _panelParent = popupPanel;
                    break;
                }

                current = current.GetParent();
            }
        }

        // Godot.EditorIntegration line.
        {
            var hbox = new HBoxContainer();
            AddChild(hbox);

            var versionLabel = new Label();
            versionLabel.Text = SR.StatusIndicatorPanel_EditorIntegrationVersionLabel;
            hbox.AddChild(versionLabel);

            hbox.AddSpacer(begin: false);

            // If the the version is a prerelease, it may contain additional metadata after a '+' character,
            // which ends up being extremely long so we hide it from the UI. It will still be copied to the
            // clipboard when the user clicks the button.
            string version = EditorIntegrationState.Version;
            if (version.Contains('+'))
            {
                version = version.Substring(0, version.IndexOf('+'));
            }

            var versionInfoButton = new LinkButton()
            {
                Text = version,
                TooltipText = SR.StatusIndicatorPanel_ClickToCopyVersion,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            versionInfoButton.Pressed += CopyEditorIntegrationVersionToClipboard;
            hbox.AddChild(versionInfoButton);
        }

        // .NET SDK line.
        {
            var hbox = new HBoxContainer();
            AddChild(hbox);

            var sdkLabel = new Label()
            {
                Text = ".NET SDK",
            };
            hbox.AddChild(sdkLabel);

            hbox.AddSpacer(begin: false);

            _dotnetSdkInfoLabel = new Label()
            {
                Text = EditorIntegrationState.DotNetSdkVersion,
            };
            hbox.AddChild(_dotnetSdkInfoLabel);

            _dotnetSdkInfoButton = new LinkButton()
            {
                Text = EditorIntegrationState.DotNetSdkVersion,
                TooltipText = SR.StatusIndicatorPanel_ClickToCopyVersion,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            _dotnetSdkInfoButton.Pressed += CopyDotNetSdkToClipboard;
            _dotnetSdkInfoButton.Hide();
            hbox.AddChild(_dotnetSdkInfoButton);
        }

        // Workspace line.
        {
            var hbox = new HBoxContainer();
            AddChild(hbox);

            _workspaceInfoIcon = new TextureRect()
            {
                StretchMode = TextureRect.StretchModeEnum.KeepCentered,
            };
            _workspaceInfoIcon.Hide();
            hbox.AddChild(_workspaceInfoIcon);

            _workspaceInfoLabel = new Label()
            {
                Text = SR.StatusIndicatorPanel_WorkspaceOpenState_None,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                CustomMinimumSize = new Vector2(512, 1),
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            hbox.AddChild(_workspaceInfoLabel);

            hbox.AddSpacer(begin: false);

            _selectProjectButton = new LinkButton()
            {
                Text = SR.StatusIndicatorPanel_ProjectOpen,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            _selectProjectButton.Pressed += SelectProject;
            hbox.AddChild(_selectProjectButton);
        }

        // Assembly line.
        {
            var hbox = new HBoxContainer();
            AddChild(hbox);

            var assemblyIcon = new TextureRect()
            {
                StretchMode = TextureRect.StretchModeEnum.KeepCentered,
            };
            _assemblyInfoIcon = new TextureRect()
            {
                StretchMode = TextureRect.StretchModeEnum.KeepCentered,
            };
            _assemblyInfoIcon.Hide();
            hbox.AddChild(_assemblyInfoIcon);

            _assemblyInfoLabel = new Label()
            {
                Text = SR.StatusIndicatorPanel_AssemblyLoadState_None,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                CustomMinimumSize = new Vector2(512, 1),
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            hbox.AddChild(_assemblyInfoLabel);

            hbox.AddSpacer(begin: false);

            _buildButton = new LinkButton()
            {
                Text = SR.StatusIndicatorPanel_ProjectBuild,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            };
            _buildButton.Pressed += BuildAssembly;
            _buildButton.Hide();
            hbox.AddChild(_buildButton);
        }
    }

    // IMPORTANT: Do not rename this method, it's called from the .NET module
    // using the name "update" and changing it will break.
    [BindMethod(Name = "update")]
    public void Update()
    {
        UpdateContents();
    }

    private struct Severity
    {
        public StatusIndicatorSeverity Value { get; private set; }

        public void Update(StatusIndicatorSeverity newSeverity)
        {
            // We only want to update the severity if it's worse,
            // so we don't end up showing a warning icon when there's
            // already an error. The highest severity wins.
            if (newSeverity > Value)
            {
                Value = newSeverity;
            }
        }
    }

    private void UpdateContents()
    {
        var severity = new Severity();

        EditorInternal.ModuleGetAssemblyLoadState(out string? loadedAssemblyName, out AssemblyLoadState assemblyLoadState);

        // .NET SDK line.
        if (string.IsNullOrEmpty(EditorIntegrationState.DotNetSdkPath))
        {
            _dotnetSdkInfoLabel.Show();
            _dotnetSdkInfoButton.Hide();
        }
        else
        {
            _dotnetSdkInfoButton.Text = EditorIntegrationState.DotNetSdkVersion;
            _dotnetSdkInfoLabel.Hide();
            _dotnetSdkInfoButton.Show();
        }

        // Workspace line.
        {
            _selectProjectButton.Text = SR.StatusIndicatorPanel_ProjectOpen;
            _selectProjectButton.Disabled = false;
            _workspaceInfoIcon.Hide();

            switch (_workspace.State)
            {
                case DotNetWorkspaceState.NotOpened:
                case DotNetWorkspaceState.ProjectNotFound:
                {
                    // This is an acceptable scenario if there is no .csproj files at all in the workspace.
                    // It indicates it's likely a project that doesn't use C# (i.e., a GDScript-only project).
                    // But if there are .csproj files somewhere, it means the project settings need to be
                    // configured to point to the right one.
                    // IMPORTANT: We only support .csproj files in the root directory, but we still show
                    // a warning if there are .csproj files in subdirectories, in case the user accidentally
                    // misplaced them.
                    if (WorkspaceHasProjects())
                    {
                        severity.Update(StatusIndicatorSeverity.Warning);
                        _workspaceInfoIcon.Show();
                        _workspaceInfoIcon.Texture = GetThemeIcon(EditorThemeNames.StatusWarning, EditorThemeNames.EditorIcons);
                        _workspaceInfoLabel.Text = SR.StatusIndicatorPanel_WorkspaceOpenState_None_FoundProjects;
                    }
                    else
                    {
                        _workspaceInfoLabel.Text = SR.StatusIndicatorPanel_WorkspaceOpenState_None;
                    }
                    break;
                }

                case DotNetWorkspaceState.Opening:
                {
                    severity.Update(StatusIndicatorSeverity.Loading);
                    _workspaceInfoLabel.Text = SR.StatusIndicatorPanel_WorkspaceOpenState_Opening;
                    _selectProjectButton.Disabled = true;
                    break;
                }

                case DotNetWorkspaceState.Opened:
                {
                    string csprojName = Path.GetFileName(_workspace.ProjectPath);
                    _workspaceInfoLabel.Text = csprojName;
                    _selectProjectButton.Text = SR.StatusIndicatorPanel_ProjectChange;
                    break;
                }

                case DotNetWorkspaceState.FailedToOpen:
                {
                    severity.Update(StatusIndicatorSeverity.Error);
                    _workspaceInfoIcon.Show();
                    _workspaceInfoIcon.Texture = GetThemeIcon(EditorThemeNames.StatusError, EditorThemeNames.EditorIcons);
                    _workspaceInfoLabel.Text = SR.StatusIndicatorPanel_WorkspaceOpenState_Failed;
                    break;
                }
            }
        }

        // Assembly line.
        {
            _buildButton.Hide();
            _assemblyInfoIcon.Hide();

            switch (assemblyLoadState)
            {
                case AssemblyLoadState.NotLoaded:
                case AssemblyLoadState.ProjectNotFound:
                {
                    _assemblyInfoLabel.Text = SR.StatusIndicatorPanel_AssemblyLoadState_None;
                    break;
                }

                case AssemblyLoadState.Loaded:
                {
                    _assemblyInfoLabel.Text = !string.IsNullOrEmpty(loadedAssemblyName)
                        ? $"{loadedAssemblyName}.dll"
                        : SR.StatusIndicatorPanel_AssemblyLoadState_Unknown;
                    break;
                }

                case AssemblyLoadState.DllNotFound:
                {
                    // We found a .csproj in the expected location, but no DLL.
                    // This likely means the user hasn't built the project yet, so it couldn't be loaded,
                    // which is needed to register C# classes and run editor classes.
                    // We let the user know about it, so they don't get confused about why their C# classes
                    // don't show up in the editor, and show a build button to fix it.
                    severity.Update(StatusIndicatorSeverity.Warning);
                    _assemblyInfoIcon.Show();
                    _assemblyInfoIcon.Texture = GetThemeIcon(EditorThemeNames.StatusWarning, EditorThemeNames.EditorIcons);
                    _assemblyInfoLabel.Text = SR.StatusIndicatorPanel_AssemblyLoadState_DllNotFound;
                    _buildButton.Show();
                    break;
                }

                case AssemblyLoadState.FailedToLoad:
                {
                    severity.Update(StatusIndicatorSeverity.Error);
                    _assemblyInfoIcon.Show();
                    _assemblyInfoIcon.Texture = GetThemeIcon(EditorThemeNames.StatusError, EditorThemeNames.EditorIcons);
                    _assemblyInfoLabel.Text = SR.StatusIndicatorPanel_AssemblyLoadState_Failed;
                    _buildButton.Show();
                    break;
                }

                case AssemblyLoadState.FailedToResolveGDExtensionEntryPoint:
                {
                    severity.Update(StatusIndicatorSeverity.Error);
                    _assemblyInfoIcon.Show();
                    _assemblyInfoIcon.Texture = GetThemeIcon(EditorThemeNames.StatusError, EditorThemeNames.EditorIcons);
                    _assemblyInfoLabel.Text = SR.StatusIndicatorPanel_AssemblyLoadState_FailedToResolveGDExtensionEntryPoint;
                    _buildButton.Show();
                    break;
                }


                case AssemblyLoadState.FailedToInitializeGDExtension:
                {
                    severity.Update(StatusIndicatorSeverity.Error);
                    _assemblyInfoIcon.Show();
                    _assemblyInfoIcon.Texture = GetThemeIcon(EditorThemeNames.StatusError, EditorThemeNames.EditorIcons);
                    _assemblyInfoLabel.Text = SR.StatusIndicatorPanel_AssemblyLoadState_FailedToInitializeGDExtension;
                    _buildButton.Show();
                    break;
                }
            }
        }

        EditorInternal.StatusIndicatorUpdateSeverity(severity.Value);
    }

    private static bool WorkspaceHasProjects()
    {
        string rootPath = ProjectSettings.Singleton.GlobalizePath("res://");

        return Directory.EnumerateFiles(rootPath, "*.csproj", new EnumerationOptions()
        {
            RecurseSubdirectories = true,
        }).Any();
    }

    private void OpenUrl(string url)
    {
        _panelParent?.Hide();
        OS.Singleton.ShellOpen(url);
    }

    private static void CopyEditorIntegrationVersionToClipboard()
    {
        DisplayServer.Singleton.ClipboardSet($"Godot.EditorIntegration {EditorIntegrationState.Version}");
    }

    private static void CopyDotNetSdkToClipboard()
    {
        DisplayServer.Singleton.ClipboardSet($"{EditorIntegrationState.DotNetSdkVersion} [{EditorIntegrationState.DotNetSdkPath}]");
    }

    private void SelectProject()
    {
        _panelParent?.Hide();
        _projectSelectorDialog.PopupDialog(LoadProject);
    }

    private static void LoadProject(string projectPath)
    {
        string assemblyName = Path.GetFileNameWithoutExtension(projectPath);
        EditorInternal.ModuleChangeProjectAssembly(assemblyName);
    }

    private void BuildAssembly()
    {
        _panelParent?.Hide();
        DotNetEditorPlugin.Singleton.BuildProjectPressed();
    }

    protected override void Dispose(bool disposing)
    {
        _projectSelectorDialog?.QueueFree();
        EditorInternal.StatusPanelSetContent(null);
        base.Dispose(disposing);
    }
}
