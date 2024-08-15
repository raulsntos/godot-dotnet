using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Godot.Collections;
using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Build.UI;

[GodotClass]
internal sealed partial class BuildProblemsView : HBoxContainer
{
#nullable disable
    private Button _clearButton;
    private Button _copyButton;

    private Button _toggleLayoutButton;

    private Button _showSearchButton;
    private LineEdit _searchBox;
#nullable enable

    private readonly Dictionary<DiagnosticSeverity, BuildProblemsFilter> _filtersBySeverity = [];

#nullable disable
    private Tree _problemsTree;
    private PopupMenu _problemsContextMenu;
#nullable enable

    public enum ProblemsLayout { List, Tree }
    private ProblemsLayout _layout = ProblemsLayout.Tree;

    private readonly List<BuildDiagnostic> _diagnostics = [];

    public int TotalDiagnosticCount => _diagnostics.Count;

    private readonly Dictionary<DiagnosticSeverity, int> _problemCountBySeverity = [];

    public int WarningCount =>
        GetProblemCountForSeverity(DiagnosticSeverity.Warning);

    public int ErrorCount =>
        GetProblemCountForSeverity(DiagnosticSeverity.Error);

    private int GetProblemCountForSeverity(DiagnosticSeverity severity)
    {
        if (!_problemCountBySeverity.TryGetValue(severity, out int count))
        {
            count = _diagnostics.Count(d => d.Severity == severity);
            _problemCountBySeverity[severity] = count;
        }

        return count;
    }

    private static IEnumerable<BuildDiagnostic> ReadDiagnosticsFromFile(string csvFile)
    {
        using var file = FileAccess.Open(csvFile, FileAccess.ModeFlags.Read);

        if (file is null)
        {
            yield break;
        }

        while (!file.EofReached())
        {
            PackedStringArray csvColumns = file.GetCsvLine();

            if (csvColumns.Count == 1 && string.IsNullOrEmpty(csvColumns[0]))
            {
                yield break;
            }

            if (csvColumns.Count != 7)
            {
                GD.PushError(SR.FormatMSBuildPanel_Expected7ColumnsGotN(csvColumns.Count));
                continue;
            }

            var diagnostic = new BuildDiagnostic()
            {
                Severity = csvColumns[0] switch
                {
                    "warning" => DiagnosticSeverity.Warning,
                    "error" or _ => DiagnosticSeverity.Error,
                },
                File = csvColumns[1],
                Line = int.Parse(csvColumns[2], CultureInfo.InvariantCulture),
                Column = int.Parse(csvColumns[3], CultureInfo.InvariantCulture),
                Id = csvColumns[4],
                Message = csvColumns[5],
                ProjectFile = csvColumns[6],
            };

            // If there's no ProjectFile but the File is a csproj, then use that.
            if (string.IsNullOrEmpty(diagnostic.ProjectFile)
             && !string.IsNullOrEmpty(diagnostic.File)
             && diagnostic.File.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                diagnostic.ProjectFile = diagnostic.File;
            }

            yield return diagnostic;
        }
    }

    public void SetDiagnosticsFromFile(string csvFile)
    {
        var diagnostics = ReadDiagnosticsFromFile(csvFile);
        SetDiagnostics(diagnostics);
    }

    public void SetDiagnostics(IEnumerable<BuildDiagnostic> diagnostics)
    {
        _diagnostics.Clear();
        _problemCountBySeverity.Clear();

        _diagnostics.AddRange(diagnostics);
        UpdateProblemsView();
    }

    public void Clear()
    {
        _problemsTree.Clear();
        _diagnostics.Clear();
        _problemCountBySeverity.Clear();

        UpdateProblemsView();
    }

    private void CopySelectedProblems()
    {
        var selectedItem = _problemsTree.GetNextSelected(null);
        if (selectedItem is null)
        {
            return;
        }

        var selectedIdxs = new List<int>();
        while (selectedItem is not null)
        {
            int selectedIdx = (int)selectedItem.GetMetadata(0);
            selectedIdxs.Add(selectedIdx);

            selectedItem = _problemsTree.GetNextSelected(selectedItem);
        }

        if (selectedIdxs.Count == 0)
        {
            return;
        }

        var selectedDiagnostics = selectedIdxs.Select(i => _diagnostics[i]);

        var sb = new StringBuilder();

        foreach (var diagnostic in selectedDiagnostics)
        {
            if (!string.IsNullOrEmpty(diagnostic.Id))
            {
                sb.Append(CultureInfo.InvariantCulture, $"{diagnostic.Id}: ");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"{diagnostic.Message} {diagnostic.File}({diagnostic.Line},{diagnostic.Column})");
        }

        string text = sb.ToString();

        if (!string.IsNullOrEmpty(text))
        {
            DisplayServer.Singleton.ClipboardSet(text);
        }
    }

    private void ToggleLayout(bool pressed)
    {
        _layout = pressed ? ProblemsLayout.List : ProblemsLayout.Tree;

        var editorSettings = EditorInterface.Singleton.GetEditorSettings();
        editorSettings.SetSetting(EditorSettingNames.ProblemsLayout, Variant.From(_layout));

        _toggleLayoutButton.Icon = GetToggleLayoutIcon();
        _toggleLayoutButton.TooltipText = GetToggleLayoutTooltipText();

        UpdateProblemsView();
    }

    private bool GetToggleLayoutPressedState()
    {
        // If pressed: List layout.
        // If not pressed: Tree layout.
        return _layout == ProblemsLayout.List;
    }

    private Texture2D? GetToggleLayoutIcon()
    {
        return _layout switch
        {
            ProblemsLayout.List =>
                GetThemeIcon(EditorThemeNames.FileList, EditorThemeNames.EditorIcons),
            ProblemsLayout.Tree or _ =>
                GetThemeIcon(EditorThemeNames.FileTree, EditorThemeNames.EditorIcons),
        };
    }

    private string GetToggleLayoutTooltipText()
    {
        return _layout switch
        {
            ProblemsLayout.List => SR.MSBuildPanel_ViewAsATree,
            ProblemsLayout.Tree or _ => SR.MSBuildPanel_ViewAsAList,
        };
    }

    private void ToggleSearchBoxVisibility(bool pressed)
    {
        _searchBox.Visible = pressed;
        if (pressed)
        {
            _searchBox.GrabFocus();
        }
    }

    private void SearchTextChanged(string text)
    {
        UpdateProblemsView();
    }

    private void ToggleFilter(bool pressed)
    {
        UpdateProblemsView();
    }

    private void GoToSelectedProblem()
    {
        var selectedItem = _problemsTree.GetSelected();
        if (selectedItem is null)
        {
            throw new InvalidOperationException(SR.MSBuildPanel_InvalidOperation_ItemTreeHasNoSelectedItems);
        }

        // Get correct diagnostic index from problems tree.
        int diagnosticIndex = (int)selectedItem.GetMetadata(0);

        if (diagnosticIndex < 0 || diagnosticIndex >= _diagnostics.Count)
        {
            throw new InvalidOperationException(SR.MSBuildPanel_InvalidOperation_DiagnosticIndexOutOfRange);
        }

        var diagnostic = _diagnostics[diagnosticIndex];

        if (string.IsNullOrEmpty(diagnostic.File))
        {
            return;
        }

        DotNetEditorPlugin.Singleton.CodeEditorManager.OpenInCurrentEditor(diagnostic.File, diagnostic.Line, diagnostic.Column);
    }

    private void ShowProblemContextMenu(Vector2 position, long mouseButtonIndex)
    {
        if (mouseButtonIndex != (long)MouseButton.Right)
        {
            return;
        }

        _problemsContextMenu.Clear();
        _problemsContextMenu.Size = new Vector2I(1, 1);

        var selectedItem = _problemsTree.GetSelected();
        if (selectedItem is not null)
        {
            // Add menu entries for the selected item.
            _problemsContextMenu.AddIconItem(GetThemeIcon(EditorThemeNames.ActionCopy, EditorThemeNames.EditorIcons),
                label: SR.MSBuildPanel_CopyError, (int)ProblemContextMenuOption.Copy);
        }

        if (_problemsContextMenu.ItemCount > 0)
        {
            _problemsContextMenu.Position = (Vector2I)(GetScreenPosition() + position);
            _problemsContextMenu.Popup();
        }
    }

    private enum ProblemContextMenuOption
    {
        Copy,
    }

    private void ProblemContextOptionPressed(long id)
    {
        switch ((ProblemContextMenuOption)id)
        {
            case ProblemContextMenuOption.Copy:
                CopySelectedProblems();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, SR.ArgumentOutOfRange_InvalidMenuOption);
        }
    }

    private bool ShouldDisplayDiagnostic(BuildDiagnostic diagnostic)
    {
        if (!_filtersBySeverity[diagnostic.Severity].IsActive)
        {
            return false;
        }

        string searchText = _searchBox.Text;
        if (string.IsNullOrEmpty(searchText))
        {
            return true;
        }

        if (diagnostic.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (diagnostic.File?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
        {
            return true;
        }

        return false;
    }

    private Color? GetProblemItemColor(BuildDiagnostic diagnostic)
    {
        return diagnostic.Severity switch
        {
            DiagnosticSeverity.Warning =>
                GetThemeColor(EditorThemeNames.WarningColor, EditorThemeNames.Editor),
            DiagnosticSeverity.Error =>
                GetThemeColor(EditorThemeNames.ErrorColor, EditorThemeNames.Editor),
            _ => null,
        };
    }

    public void UpdateProblemsView()
    {
        switch (_layout)
        {
            case ProblemsLayout.List:
                UpdateProblemsList();
                break;

            case ProblemsLayout.Tree:
            default:
                UpdateProblemsTree();
                break;
        }

        foreach (var (severity, filter) in _filtersBySeverity)
        {
            int count = _diagnostics.Count(d => d.Severity == severity);
            filter.ProblemsCount = count;
        }

        Name = _diagnostics.Count == 0
            ? new StringName(SR.MSBuildPanel_Problems)
            : new StringName(SR.FormatMSBuildPanel_ProblemsN(_diagnostics.Count));
    }

    private void UpdateProblemsList()
    {
        _problemsTree.Clear();

        var root = _problemsTree.CreateItem();

        for (int i = 0; i < _diagnostics.Count; i++)
        {
            var diagnostic = _diagnostics[i];

            if (!ShouldDisplayDiagnostic(diagnostic))
            {
                continue;
            }

            var item = CreateProblemItem(diagnostic, includeFileInText: true);

            var problemItem = _problemsTree.CreateItem(root);
            problemItem.SetIcon(0, item.Icon);
            problemItem.SetText(0, item.Text);
            problemItem.SetTooltipText(0, item.TooltipText);
            problemItem.SetMetadata(0, i);

            var color = GetProblemItemColor(diagnostic);
            if (color.HasValue)
            {
                problemItem.SetCustomColor(0, color.Value);
            }
        }
    }

    private void UpdateProblemsTree()
    {
        _problemsTree.Clear();

        var root = _problemsTree.CreateItem();

        var groupedDiagnostics = _diagnostics.Select((d, i) => (Diagnostic: d, Index: i))
            .Where(x => ShouldDisplayDiagnostic(x.Diagnostic))
            .GroupBy(x => x.Diagnostic.ProjectFile)
            .Select(g => (ProjectFile: g.Key, Diagnostics: g.GroupBy(x => x.Diagnostic.File)
                .Select(x => (File: x.Key, Diagnostics: x.ToArray()))))
            .ToArray();

        if (groupedDiagnostics.Length == 0)
        {
            return;
        }

        foreach (var (projectFile, projectDiagnostics) in groupedDiagnostics)
        {
            TreeItem projectItem;

            if (groupedDiagnostics.Length == 1)
            {
                // Don't create a project item if there's only one project.
                projectItem = root;
            }
            else
            {
                string projectFilePath = !string.IsNullOrEmpty(projectFile)
                    ? projectFile
                    : SR.MSBuildPanel_UnknownProject;
                projectItem = _problemsTree.CreateItem(root);
                projectItem.SetText(0, projectFilePath);
                projectItem.SetSelectable(0, false);
            }

            foreach (var (file, fileDiagnostics) in projectDiagnostics)
            {
                if (fileDiagnostics.Length == 0)
                {
                    continue;
                }

                string? projectDir = Path.GetDirectoryName(projectFile);
                string relativeFilePath = !string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(projectDir)
                    ? Path.GetRelativePath(projectDir, file)
                    : SR.MSBuildPanel_UnknownFile;

                string fileItemText = SR.FormatMSBuildPanel_FilePathIssuesN(relativeFilePath, fileDiagnostics.Length);

                var fileItem = _problemsTree.CreateItem(projectItem);
                fileItem.SetText(0, fileItemText);
                fileItem.SetSelectable(0, false);

                foreach (var (diagnostic, index) in fileDiagnostics)
                {
                    var item = CreateProblemItem(diagnostic);

                    var problemItem = _problemsTree.CreateItem(fileItem);
                    problemItem.SetIcon(0, item.Icon);
                    problemItem.SetText(0, item.Text);
                    problemItem.SetTooltipText(0, item.TooltipText);
                    problemItem.SetMetadata(0, index);

                    var color = GetProblemItemColor(diagnostic);
                    if (color.HasValue)
                    {
                        problemItem.SetCustomColor(0, color.Value);
                    }
                }
            }
        }
    }

    private sealed class ProblemItem
    {
        public string? Text { get; set; }
        public string? TooltipText { get; set; }
        public Texture2D? Icon { get; set; }
    }

    private ProblemItem CreateProblemItem(BuildDiagnostic diagnostic, bool includeFileInText = false)
    {
        var text = new StringBuilder();
        var tooltip = new StringBuilder();

        ReadOnlySpan<char> shortMessage = diagnostic.Message.AsSpan();
        int lineBreakIdx = shortMessage.IndexOf('\n');
        if (lineBreakIdx != -1)
        {
            shortMessage = shortMessage[..lineBreakIdx];
        }
        text.Append(shortMessage);

        tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticMessage(diagnostic.Message));

        if (!string.IsNullOrEmpty(diagnostic.Id))
        {
            tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticId(diagnostic.Id));
        }

        string diagnosticSeverity = diagnostic.Severity switch
        {
            DiagnosticSeverity.Hidden => "hidden",
            DiagnosticSeverity.Info => "info",
            DiagnosticSeverity.Warning => "warning",
            DiagnosticSeverity.Error => "error",
            _ => "unknown",
        };
        tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticSeverity(diagnosticSeverity));

        if (!string.IsNullOrEmpty(diagnostic.File))
        {
            text.Append(' ');
            if (includeFileInText)
            {
                text.Append(diagnostic.File);
            }

            text.Append(CultureInfo.InvariantCulture, $"({diagnostic.Line},{diagnostic.Column})");

            tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticFile(diagnostic.File));
            tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticLine(diagnostic.Line));
            tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticColumn(diagnostic.Column));
        }

        if (!string.IsNullOrEmpty(diagnostic.ProjectFile))
        {
            tooltip.AppendLine(SR.FormatMSBuildPanel_TooltipDiagnosticProject(diagnostic.ProjectFile));
        }

        return new ProblemItem()
        {
            Text = text.ToString(),
            TooltipText = tooltip.ToString(),
            Icon = diagnostic.Severity switch
            {
                DiagnosticSeverity.Warning =>
                    GetThemeIcon(EditorThemeNames.Warning, EditorThemeNames.EditorIcons),
                DiagnosticSeverity.Error =>
                    GetThemeIcon(EditorThemeNames.Error, EditorThemeNames.EditorIcons),
                _ => null,
            },
        };
    }

    protected override void _Ready()
    {
        var editorSettings = EditorInterface.Singleton.GetEditorSettings();
        _layout = editorSettings.GetSetting(EditorSettingNames.ProblemsLayout).As<ProblemsLayout>();

        Name = new StringName(SR.MSBuildPanel_Problems);

        var vbLeft = new VBoxContainer()
        {
            CustomMinimumSize = new Vector2(0, 180 * EditorInterface.Singleton.GetEditorScale()),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        AddChild(vbLeft);

        // Problem Tree.
        _problemsTree = new Tree()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AllowRmbSelect = true,
            HideRoot = true,
        };
        _problemsTree.ItemActivated += GoToSelectedProblem;
        _problemsTree.ItemMouseSelected += ShowProblemContextMenu;
        vbLeft.AddChild(_problemsTree);

        // Problem context menu.
        _problemsContextMenu = new PopupMenu();
        _problemsContextMenu.IdPressed += ProblemContextOptionPressed;
        _problemsTree.AddChild(_problemsContextMenu);

        // Search box.
        _searchBox = new LineEdit()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            PlaceholderText = SR.MSBuildPanel_FilterProblems,
            ClearButtonEnabled = true,
        };
        _searchBox.TextChanged += SearchTextChanged;
        vbLeft.AddChild(_searchBox);

        var vbRight = new VBoxContainer();
        AddChild(vbRight);

        // Tools grid.
        var hbTools = new HBoxContainer()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        vbRight.AddChild(hbTools);

        // Clear.
        _clearButton = new Button()
        {
            ThemeTypeVariation = EditorThemeNames.FlatButton,
            FocusMode = FocusModeEnum.None,
            Shortcut = EditorInternal.EditorDefineShortcut(EditorShortcutNames.ClearOutput, SR.MSBuildPanel_ClearOutput, (Key)KeyModifierMask.MaskCmdOrCtrl | (Key)KeyModifierMask.MaskShift | Key.K),
            ShortcutContext = this,
        };
        _clearButton.Pressed += Clear;
        hbTools.AddChild(_clearButton);

        // Copy.
        _copyButton = new Button()
        {
            ThemeTypeVariation = EditorThemeNames.FlatButton,
            FocusMode = FocusModeEnum.None,
            Shortcut = EditorInternal.EditorDefineShortcut(EditorShortcutNames.CopyOutput, SR.MSBuildPanel_CopySelection, (Key)KeyModifierMask.MaskCmdOrCtrl | Key.C),
            ShortcutContext = this,
        };
        _copyButton.Pressed += CopySelectedProblems;
        hbTools.AddChild(_copyButton);

        // A second hbox to make a 2x2 grid of buttons.
        var hbTools2 = new HBoxContainer()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        };
        vbRight.AddChild(hbTools2);

        // Toggle List/Tree.
        _toggleLayoutButton = new Button()
        {
            Flat = true,
            FocusMode = FocusModeEnum.None,
            TooltipText = GetToggleLayoutTooltipText(),
            ToggleMode = true,
            ButtonPressed = GetToggleLayoutPressedState(),
        };
        // Don't tint the icon even when in "pressed" state.
        _toggleLayoutButton.AddThemeColorOverride(EditorThemeNames.IconPressedColor, NamedColors.White);
        _toggleLayoutButton.Toggled += ToggleLayout;
        hbTools2.AddChild(_toggleLayoutButton);

        // Show Search.
        _showSearchButton = new Button()
        {
            ThemeTypeVariation = EditorThemeNames.FlatButton,
            FocusMode = FocusModeEnum.None,
            ToggleMode = true,
            ButtonPressed = true,
            Shortcut = EditorInternal.EditorDefineShortcut(EditorShortcutNames.OpenSearch, SR.MSBuildPanel_FocusSearchBar, (Key)KeyModifierMask.MaskCmdOrCtrl | Key.F),
            ShortcutContext = this,
        };
        _showSearchButton.Toggled += ToggleSearchBoxVisibility;
        hbTools2.AddChild(_showSearchButton);

        // Diagnostic Type Filters.
        vbRight.AddChild(new HSeparator());

        var infoFilter = new BuildProblemsFilter(DiagnosticSeverity.Info);
        infoFilter.ToggleButton.TooltipText = SR.MSBuildPanel_ToggleVisibilityOfInfoDiagnostics;
        infoFilter.ToggleButton.Toggled += ToggleFilter;
        vbRight.AddChild(infoFilter.ToggleButton);
        _filtersBySeverity[DiagnosticSeverity.Info] = infoFilter;

        var errorFilter = new BuildProblemsFilter(DiagnosticSeverity.Error);
        errorFilter.ToggleButton.TooltipText = SR.MSBuildPanel_ToggleVisibilityOfErrors;
        errorFilter.ToggleButton.Toggled += ToggleFilter;
        vbRight.AddChild(errorFilter.ToggleButton);
        _filtersBySeverity[DiagnosticSeverity.Error] = errorFilter;

        var warningFilter = new BuildProblemsFilter(DiagnosticSeverity.Warning);
        warningFilter.ToggleButton.TooltipText = SR.MSBuildPanel_ToggleVisibilityOfWarnings;
        warningFilter.ToggleButton.Toggled += ToggleFilter;
        vbRight.AddChild(warningFilter.ToggleButton);
        _filtersBySeverity[DiagnosticSeverity.Warning] = warningFilter;

        UpdateTheme();

        UpdateProblemsView();
    }

    protected internal override void _Notification(int what)
    {
        switch ((long)what)
        {
            case EditorSettings.NotificationEditorSettingsChanged:
                var editorSettings = EditorInterface.Singleton.GetEditorSettings();
                _layout = editorSettings.GetSetting(EditorSettingNames.ProblemsLayout).As<ProblemsLayout>();
                _toggleLayoutButton.ButtonPressed = GetToggleLayoutPressedState();
                UpdateProblemsView();
                break;

            case NotificationThemeChanged:
                UpdateTheme();
                break;
        }
    }

    private void UpdateTheme()
    {
        // Nodes will be null until _Ready is called.
        if (_clearButton is null)
        {
            return;
        }

        foreach (var (severity, filter) in _filtersBySeverity)
        {
            filter.ToggleButton.Icon = severity switch
            {
                DiagnosticSeverity.Info =>
                    GetThemeIcon(EditorThemeNames.Popup, EditorThemeNames.EditorIcons),
                DiagnosticSeverity.Warning =>
                    GetThemeIcon(EditorThemeNames.StatusWarning, EditorThemeNames.EditorIcons),
                DiagnosticSeverity.Error =>
                    GetThemeIcon(EditorThemeNames.StatusError, EditorThemeNames.EditorIcons),
                _ => null,
            };
        }

        _clearButton.Icon = GetThemeIcon(EditorThemeNames.Clear, EditorThemeNames.EditorIcons);
        _copyButton.Icon = GetThemeIcon(EditorThemeNames.ActionCopy, EditorThemeNames.EditorIcons);
        _toggleLayoutButton.Icon = GetToggleLayoutIcon();
        _showSearchButton.Icon = GetThemeIcon(EditorThemeNames.Search, EditorThemeNames.EditorIcons);
        _searchBox.RightIcon = GetThemeIcon(EditorThemeNames.Search, EditorThemeNames.EditorIcons);
    }
}
