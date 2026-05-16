using System;
using System.IO;

namespace Godot.EditorIntegration;

[GodotClass]
internal sealed partial class ProjectSelectorDialog : ConfirmationDialog
{
#nullable disable
    private LineEdit _searchBox;
    private ItemList _itemList;
    private TextureRect _warningIcon;
    private HBoxContainer _warningContainer;
#nullable restore

    private Action<string>? _itemSelectedCallback;

    public ProjectSelectorDialog()
    {
        SetTitle(SR.ProjectSelectorDialog_Title);
        SetUnparentWhenInvisible(true);
        Confirmed += OkPressed;

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride(EditorThemeNames.Separation, 0);
        AddChild(vbox);

        var mc = new MarginContainer();
        mc.AddThemeConstantOverride(EditorThemeNames.MarginTop, 6);
        mc.AddThemeConstantOverride(EditorThemeNames.MarginBottom, 6);
        mc.AddThemeConstantOverride(EditorThemeNames.MarginLeft, 1);
        mc.AddThemeConstantOverride(EditorThemeNames.MarginRight, 1);
        vbox.AddChild(mc);

        _searchBox = new LineEdit()
        {
            PlaceholderText = SR.ProjectSelectorDialog_SearchProjects,
            ClearButtonEnabled = true,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        mc.AddChild(_searchBox);

        _warningContainer = new HBoxContainer();
        _warningContainer.Hide();
        vbox.AddChild(_warningContainer);

        _warningIcon = new TextureRect()
        {
            StretchMode = TextureRect.StretchModeEnum.KeepCentered,
        };
        _warningContainer.AddChild(_warningIcon);

        var warningLabel = new Label()
        {
            Text = SR.ProjectSelectorDialog_SomeProjectsNotShown,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2I(600, 1),
        };
        _warningContainer.AddChild(warningLabel);

        _itemList = new ItemList()
        {
            SelectMode = ItemList.SelectModeEnum.Single,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2I(0, 300),
        };
        vbox.AddChild(_itemList);

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.GuiInput += SearchBoxGuiInput;
        _itemList.ItemActivated += ItemActivated;
        RegisterTextEnter(_searchBox);

        MinSize = new Vector2I(600, 400);
    }

    private void PopulateItems(string filter = "")
    {
        _itemList.Clear();
        _warningContainer.Hide();

        string rootPath = ProjectSettings.Singleton.GlobalizePath("res://");

        var rootDirectory = new DirectoryInfo(rootPath);

        var projects = Directory.EnumerateFiles(rootPath, "*.csproj", new EnumerationOptions()
        {
            RecurseSubdirectories = true,
        });

        bool hasProjectsOutsideRoot = false;
        foreach (string projectPath in projects)
        {
            string fileName = Path.GetFileName(projectPath);

            var fileInfo = new FileInfo(projectPath);
            if (!ArePathsEqual(fileInfo.Directory?.FullName, rootDirectory.FullName))
            {
                // Only projects in the root directory are supported.
                hasProjectsOutsideRoot = true;
                continue;
            }

            if (!string.IsNullOrEmpty(filter)
             && !fileName.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int index = _itemList.AddItem(fileName);
            _itemList.SetItemMetadata(index, projectPath);
        }

        if (hasProjectsOutsideRoot)
        {
            _warningContainer.Show();
        }

        // Select the first item by default, if any.
        if (_itemList.GetItemCount() > 0)
        {
            _itemList.Select(0);
        }

        GetOkButton().Disabled = _itemList.GetSelectedItems().Count == 0;

        static bool ArePathsEqual(scoped ReadOnlySpan<char> left, scoped ReadOnlySpan<char> right)
        {
            left = left.TrimEnd([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
            right = right.TrimEnd([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
            return left.SequenceEqual(right);
        }
    }

    private void SearchTextChanged(string text)
    {
        PopulateItems(text);
    }

    private void SearchBoxGuiInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed)
        {
            return;
        }

        int itemCount = _itemList.GetItemCount();
        if (itemCount == 0)
        {
            return;
        }

        switch (keyEvent.Keycode)
        {
            case Key.Up:
            case Key.Down:
                var selected = _itemList.GetSelectedItems();
                int current = selected.Count > 0 ? selected[0] : -1;

                int next = keyEvent.Keycode == Key.Up
                    ? current <= 0 ? itemCount - 1 : current - 1
                    : current < 0 || current >= itemCount - 1 ? 0 : current + 1;

                _itemList.Select(next);
                _itemList.EnsureCurrentIsVisible();
                GetOkButton().Disabled = false;
                _searchBox.AcceptEvent();
                break;
        }
    }

    private void ItemActivated(long index)
    {
        string path = _itemList.GetItemMetadata((int)index).AsString();
        _itemSelectedCallback?.Invoke(path);
    }

    private void OkPressed()
    {
        var selected = _itemList.GetSelectedItems();
        if (selected.Count == 0)
        {
            return;
        }

        string path = _itemList.GetItemMetadata(selected[0]).AsString();
        _itemSelectedCallback?.Invoke(path);
    }

    protected internal override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationThemeChanged:
                _warningIcon.Texture = GetThemeIcon(EditorThemeNames.NodeWarning, EditorThemeNames.EditorIcons);
                break;
        }
    }

    public void PopupDialog(Action<string> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        EditorInterface.Singleton.PopupDialogCentered(this);

        _itemSelectedCallback = callback;
        _searchBox.Clear();
        PopulateItems();
        _searchBox.GrabFocus();

        GetOkButton().Disabled = _itemList.GetSelectedItems().Count == 0;

        PopupCentered(new Vector2I(600, 400));
    }
}
