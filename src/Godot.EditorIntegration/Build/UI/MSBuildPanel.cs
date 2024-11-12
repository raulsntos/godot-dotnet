using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Build.UI;

[GodotClass]
internal sealed partial class MSBuildPanel : MarginContainer
{
    public event Action? BuildStateChanged;

#nullable disable
    private MenuButton _buildMenuButton;
    private Button _openLogsFolderButton;

    private BuildProblemsView _problemsView;
    private BuildOutputView _outputView;
#nullable enable

    public bool IsBuildingOngoing { get; private set; }
    public CommonOptions? LastBuildOptions { get; private set; }
    public BuildResult? LastBuildResult { get; private set; }

    private readonly Lock _pendingBuildLogTextLock = new();
    private string _pendingBuildLogText = string.Empty;

    public Texture2D? GetBuildStateIcon()
    {
        if (IsBuildingOngoing)
        {
            return GetThemeIcon(EditorThemeNames.Stop, EditorThemeNames.EditorIcons);
        }

        if (_problemsView.WarningCount > 0 && _problemsView.ErrorCount > 0)
        {
            return GetThemeIcon(EditorThemeNames.ErrorWarning, EditorThemeNames.EditorIcons);
        }

        if (_problemsView.WarningCount > 0)
        {
            return GetThemeIcon(EditorThemeNames.Warning, EditorThemeNames.EditorIcons);
        }

        if (_problemsView.ErrorCount > 0)
        {
            return GetThemeIcon(EditorThemeNames.Error, EditorThemeNames.EditorIcons);
        }

        return null;
    }

    private enum BuildMenuOptions
    {
        BuildProject,
        RebuildProject,
        CleanProject,
    }

    private void BuildMenuOptionPressed(long id)
    {
        switch ((BuildMenuOptions)id)
        {
            case BuildMenuOptions.BuildProject:
                BuildProject();
                break;

            case BuildMenuOptions.RebuildProject:
                RebuildProject();
                break;

            case BuildMenuOptions.CleanProject:
                CleanProject();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, SR.ArgumentOutOfRange_InvalidMenuOption);
        }
    }

    private static void BuildProject()
    {
        string projectPath = EditorPath.ProjectCSProjPath;

        if (!File.Exists(projectPath))
        {
            // No project to build.
            return;
        }

        BuildManager.BuildProjectBlocking(new BuildOptions()
        {
            SlnOrProject = projectPath,
        }
            .WithGodotDebugDefaults());
    }

    private static void RebuildProject()
    {
        string projectPath = EditorPath.ProjectCSProjPath;

        if (!File.Exists(projectPath))
        {
            // No project to build.
            return;
        }

        BuildManager.BuildProjectBlocking(new BuildOptions()
        {
            SlnOrProject = projectPath,
            NoIncremental = true,
        }
            .WithGodotDebugDefaults());
    }

    private static void CleanProject()
    {
        string projectPath = EditorPath.ProjectCSProjPath;

        if (!File.Exists(projectPath))
        {
            // No project to build.
            return;
        }

        BuildManager.CleanProjectBlocking(new CleanOptions()
        {
            SlnOrProject = projectPath,
        }
            .WithGodotDebugDefaults());
    }

    private void OpenLogsFolder()
    {
        OS.Singleton.ShellOpen($"file://{EditorPath.GetLogsDirPathFor(BuildManager.DefaultBuildConfiguration)}");
    }

    private void BuildLaunchFailed(string cause)
    {
        IsBuildingOngoing = false;
        LastBuildResult = BuildResult.Error;

        _problemsView.Clear();
        _outputView.Clear();

        var diagnostic = new BuildDiagnostic()
        {
            Severity = DiagnosticSeverity.Error,
            Message = cause,
        };

        _problemsView.SetDiagnostics([diagnostic]);

        OnBuildStateChanged();
    }

    private void BuildStarted(CommonOptions buildOptions)
    {
        IsBuildingOngoing = true;
        LastBuildOptions = buildOptions;
        LastBuildResult = null;

        _problemsView.Clear();
        _outputView.Clear();

        _problemsView.UpdateProblemsView();

        OnBuildStateChanged();
    }

    private void BuildFinished(BuildResult result)
    {
        IsBuildingOngoing = false;
        LastBuildResult = result;

        Debug.Assert(LastBuildOptions is not null);
        string logsDirPath = EditorPath.GetLogsDirPathFor(LastBuildOptions.SlnOrProject, LastBuildOptions.Configuration ?? BuildManager.DefaultBuildConfiguration);

        string csvFile = Path.Join(logsDirPath, BuildManager.MSBuildIssuesFileName);
        _problemsView.SetDiagnosticsFromFile(csvFile);

        _problemsView.UpdateProblemsView();

        OnBuildStateChanged();
    }

    private void OnBuildStateChanged()
    {
        BuildStateChanged?.Invoke();
    }

    private void UpdateBuildLogText()
    {
        lock (_pendingBuildLogTextLock)
        {
            _outputView.Append(_pendingBuildLogText);
            _pendingBuildLogText = string.Empty;
        }
    }

    private void AppendBuildLogText(string? text)
    {
        lock (_pendingBuildLogTextLock)
        {
            if (_pendingBuildLogText.Length == 0)
            {
                // StdOutput/Error can be received from different threads,
                // so we need to use CallDeferred.
                Callable.From(UpdateBuildLogText).CallDeferred();
            }
            _pendingBuildLogText += $"{text}\n";
        }
    }

    protected override void _Ready()
    {
        var bottomPanelStylebox = EditorInterface.Singleton.GetBaseControl().GetThemeStylebox(EditorThemeNames.BottomPanel, EditorThemeNames.EditorStyles);
        AddThemeConstantOverride(EditorThemeNames.MarginTop, -(int)bottomPanelStylebox.ContentMarginTop);
        AddThemeConstantOverride(EditorThemeNames.MarginLeft, -(int)bottomPanelStylebox.ContentMarginLeft);
        AddThemeConstantOverride(EditorThemeNames.MarginRight, -(int)bottomPanelStylebox.ContentMarginRight);

        var tabs = new TabContainer();
        AddChild(tabs);

        var tabActions = new HBoxContainer()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Alignment = BoxContainer.AlignmentMode.End,
        };
        tabActions.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        tabs.GetTabBar().AddChild(tabActions);

        _buildMenuButton = new MenuButton()
        {
            TooltipText = SR.MSBuildPanel_Build,
            Flat = true,
        };
        tabActions.AddChild(_buildMenuButton);

        var buildMenu = _buildMenuButton.GetPopup();
        buildMenu.AddItem(SR.MSBuildPanel_BuildProject, (int)BuildMenuOptions.BuildProject);
        buildMenu.AddItem(SR.MSBuildPanel_RebuildProject, (int)BuildMenuOptions.RebuildProject);
        buildMenu.AddItem(SR.MSBuildPanel_CleanProject, (int)BuildMenuOptions.CleanProject);
        buildMenu.IdPressed += BuildMenuOptionPressed;

        _openLogsFolderButton = new Button()
        {
            TooltipText = SR.MSBuildPanel_ShowLogsInFileManager,
            Flat = true,
        };
        _openLogsFolderButton.Pressed += OpenLogsFolder;
        tabActions.AddChild(_openLogsFolderButton);

        _problemsView = new BuildProblemsView();
        tabs.AddChild(_problemsView);

        _outputView = new BuildOutputView();
        tabs.AddChild(_outputView);

        UpdateTheme();

        BuildManager.BuildLaunchFailed += BuildLaunchFailed;
        BuildManager.BuildStarted += BuildStarted;
        BuildManager.BuildFinished += BuildFinished;
        BuildManager.StdOutputReceived += AppendBuildLogText;
        BuildManager.StdErrorReceived += AppendBuildLogText;
    }

    protected internal override void _Notification(int what)
    {
        if (what == NotificationThemeChanged)
        {
            UpdateTheme();
        }
    }

    private void UpdateTheme()
    {
        // Nodes will be null until _Ready is called.
        if (_buildMenuButton is null)
        {
            return;
        }

        _buildMenuButton.Icon = GetThemeIcon(EditorThemeNames.BuildDotNet, EditorThemeNames.EditorIcons);
        _openLogsFolderButton.Icon = GetThemeIcon(EditorThemeNames.FileSystem, EditorThemeNames.EditorIcons);
    }
}
