using System.Text;

namespace Godot.SourceGenerators;

internal class IndentedStringBuilder
{
    private readonly StringBuilder _sb = new();

    private bool _indentationPending;

    public int Indent { get; set; }

    public void OpenBlock()
    {
        AppendLine('{');
        Indent++;
    }

    public void CloseBlock()
    {
        Indent--;
        AppendLine('}');
    }

    public void Append(string value)
    {
        AppendIndentationIfNeeded();
        _sb.Append(value);
    }

    public void Append(char value)
    {
        AppendIndentationIfNeeded();
        _sb.Append(value);
    }

    public void AppendLine(string value)
    {
        AppendIndentationIfNeeded();
        _sb.AppendLine(value);
        _indentationPending = true;
    }

    public void AppendLine(char value)
    {
        AppendIndentationIfNeeded();
        _sb.Append(value);
        _sb.AppendLine();
        _indentationPending = true;
    }

    public void AppendLine()
    {
        AppendIndentationIfNeeded();
        _sb.AppendLine();
        _indentationPending = true;
    }

    private void AppendIndentationIfNeeded()
    {
        if (!_indentationPending)
        {
            return;
        }

        for (int i = 0; i < Indent; i++)
        {
            _sb.Append("    ");
        }
        _indentationPending = false;
    }

    public override string ToString() => _sb.ToString();
}
