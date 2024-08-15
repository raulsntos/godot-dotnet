using System.Globalization;

namespace Godot.EditorIntegration.Build.UI;

internal sealed class BuildProblemsFilter
{
    public DiagnosticSeverity Severity { get; }

    public Button ToggleButton { get; }

    private int _problemsCount;

    public int ProblemsCount
    {
        get => _problemsCount;
        set
        {
            _problemsCount = value;
            ToggleButton.Text = _problemsCount.ToString(CultureInfo.CurrentCulture);
        }
    }

    public bool IsActive => ToggleButton.ButtonPressed;

    public BuildProblemsFilter(DiagnosticSeverity severity)
    {
        Severity = severity;
        ToggleButton = new Button()
        {
            ToggleMode = true,
            ButtonPressed = true,
            Text = "0",
            FocusMode = Control.FocusModeEnum.None,
            ThemeTypeVariation = EditorThemeNames.EditorLogFilterButton,
        };
    }
}
