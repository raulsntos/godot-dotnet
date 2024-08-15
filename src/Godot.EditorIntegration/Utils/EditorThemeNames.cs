namespace Godot.EditorIntegration;

/// <summary>
/// Contains cached StringNames used to retrieve theme values.
/// </summary>
internal static class EditorThemeNames
{
    // Editor.
    public static StringName Editor { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Editor"u8);

    // Editor icons.
    public static StringName EditorIcons { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("EditorIcons"u8);
    public static StringName BuildDotNet { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("BuildDotNet"u8);
    public static StringName ActionCopy { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("ActionCopy"u8);
    public static StringName Search { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Search"u8);
    public static StringName Clear { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Clear"u8);
    public static StringName Stop { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Stop"u8);
    public static StringName FileList { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("FileList"u8);
    public static StringName FileTree { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("FileTree"u8);
    public static StringName FileSystem { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Filesystem"u8);
    public static StringName Popup { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Popup"u8);
    public static StringName StatusWarning { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("StatusWarning"u8);
    public static StringName StatusError { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("StatusError"u8);
    public static StringName Warning { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Warning"u8);
    public static StringName Error { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("Error"u8);
    public static StringName ErrorWarning { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("ErrorWarning"u8);

    // Theme variations.
    public static StringName FlatButton { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("FlatButton"u8);
    public static StringName RunBarButton { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("RunBarButton"u8);
    public static StringName EditorLogFilterButton { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("EditorLogFilterButton"u8);

    // Editor colors.
    public static StringName IconPressedColor { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("icon_pressed_color"u8);
    public static StringName WarningColor { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("warning_color"u8);
    public static StringName ErrorColor { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("error_color"u8);

    // Editor fonts.
    public static StringName EditorFonts { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("EditorFonts"u8);
    public static StringName OutputSource { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("output_source"u8);
    public static StringName NormalFont { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("normal_font"u8);
    public static StringName OutputSourceBold { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("output_source_bold"u8);
    public static StringName BoldFont { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("bold_font"u8);
    public static StringName OutputSourceItalic { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("output_source_italic"u8);
    public static StringName ItalicsFont { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("italics_font"u8);
    public static StringName OutputSourceBoldItalic { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("output_source_bold_italic"u8);
    public static StringName BoldItalicsFont { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("bold_italics_font"u8);
    public static StringName OutputSourceMono { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("output_source_mono"u8);
    public static StringName MonoFont { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("mono_font"u8);

    // Editor constants.
    public static StringName TextHighlightHorizontalPadding { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("text_highlight_h_padding"u8);
    public static StringName TextHighlightVerticalPadding { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("text_highlight_v_padding"u8);
    public static StringName MarginTop { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("margin_top"u8);
    public static StringName MarginLeft { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("margin_left"u8);
    public static StringName MarginRight { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("margin_right"u8);
    public static StringName OutputSourceSize { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("output_source_size"u8);
    public static StringName NormalFontSize { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("normal_font_size"u8);
    public static StringName BoldFontSize { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("bold_font_size"u8);
    public static StringName ItalicsFontSize { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("italics_font_size"u8);
    public static StringName MonoFontSize { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("mono_font_size"u8);

    // Editor styles.
    public static StringName EditorStyles { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("EditorStyles"u8);
    public static StringName BottomPanel { get; } = StringName.CreateStaticStringNameFromAsciiLiteral("BottomPanel"u8);
}
