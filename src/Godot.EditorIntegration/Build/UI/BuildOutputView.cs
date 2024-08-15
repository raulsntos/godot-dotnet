using Godot.EditorIntegration.Internals;

namespace Godot.EditorIntegration.Build.UI;

[GodotClass]
internal sealed partial class BuildOutputView : HBoxContainer
{
#nullable disable
    private RichTextLabel _log;

    private Button _clearButton;
    private Button _copyButton;
#nullable enable

    public void Append(string text)
    {
        _log.AddText(text);
    }

    public void Clear()
    {
        _log.Clear();
    }

    private void CopyRequested()
    {
        string text = _log.GetSelectedText();

        if (string.IsNullOrEmpty(text))
        {
            text = _log.GetParsedText();
        }

        if (!string.IsNullOrEmpty(text))
        {
            DisplayServer.Singleton.ClipboardSet(text);
        }
    }

    protected override void _Ready()
    {
        Name = new StringName(SR.MSBuildPanel_Output);

        var vbLeft = new VBoxContainer()
        {
            CustomMinimumSize = new Vector2(0, 180 * EditorInterface.Singleton.GetEditorScale()),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        AddChild(vbLeft);

        // Log - Rich Text Label.
        _log = new RichTextLabel()
        {
            BbcodeEnabled = true,
            ScrollFollowing = true,
            SelectionEnabled = true,
            ContextMenuEnabled = true,
            FocusMode = FocusModeEnum.Click,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            DeselectOnFocusLossEnabled = false,

        };
        vbLeft.AddChild(_log);

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
        _copyButton.Pressed += CopyRequested;
        hbTools.AddChild(_copyButton);

        UpdateTheme();
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
        if (_log is null)
        {
            return;
        }

        var normalFont = GetThemeFont(EditorThemeNames.OutputSource, EditorThemeNames.EditorFonts);
        if (normalFont is not null)
        {
            _log.AddThemeFontOverride(EditorThemeNames.NormalFont, normalFont);
        }

        var boldFont = GetThemeFont(EditorThemeNames.OutputSourceBold, EditorThemeNames.EditorFonts);
        if (boldFont is not null)
        {
            _log.AddThemeFontOverride(EditorThemeNames.BoldFont, boldFont);
        }

        var italicsFont = GetThemeFont(EditorThemeNames.OutputSourceItalic, EditorThemeNames.EditorFonts);
        if (italicsFont is not null)
        {
            _log.AddThemeFontOverride(EditorThemeNames.ItalicsFont, italicsFont);
        }

        var boldItalicsFont = GetThemeFont(EditorThemeNames.OutputSourceBoldItalic, EditorThemeNames.EditorFonts);
        if (boldItalicsFont is not null)
        {
            _log.AddThemeFontOverride(EditorThemeNames.BoldItalicsFont, boldItalicsFont);
        }

        var monoFont = GetThemeFont(EditorThemeNames.OutputSourceMono, EditorThemeNames.EditorFonts);
        if (monoFont is not null)
        {
            _log.AddThemeFontOverride(EditorThemeNames.MonoFont, monoFont);
        }

        // Disable padding for highlighted background/foreground to prevent highlights from overlapping on close lines.
        // This also better matches terminal output, which does not use any form of padding.
        _log.AddThemeConstantOverride(EditorThemeNames.TextHighlightHorizontalPadding, 0);
        _log.AddThemeConstantOverride(EditorThemeNames.TextHighlightVerticalPadding, 0);

        int font_size = GetThemeFontSize(EditorThemeNames.OutputSourceSize, EditorThemeNames.EditorFonts);
        _log.AddThemeFontSizeOverride(EditorThemeNames.NormalFontSize, font_size);
        _log.AddThemeFontSizeOverride(EditorThemeNames.BoldFontSize, font_size);
        _log.AddThemeFontSizeOverride(EditorThemeNames.ItalicsFontSize, font_size);
        _log.AddThemeFontSizeOverride(EditorThemeNames.MonoFontSize, font_size);

        _clearButton.Icon = GetThemeIcon(EditorThemeNames.Clear, EditorThemeNames.EditorIcons);
        _copyButton.Icon = GetThemeIcon(EditorThemeNames.ActionCopy, EditorThemeNames.EditorIcons);
    }
}
