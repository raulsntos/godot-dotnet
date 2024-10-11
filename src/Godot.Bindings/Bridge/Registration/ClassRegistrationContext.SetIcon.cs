namespace Godot.Bridge;

partial class ClassRegistrationContext
{
    /// <summary>
    /// Path to the image that will be used as the class' icon.
    /// </summary>
    public string? IconPath { get; private set; }

    /// <summary>
    /// Set the icon for the class. If an icon is not provided or <paramref name="iconPath"/>
    /// is <see langword="null"/>, the icon will be inherited from the base class.
    /// </summary>
    /// <param name="iconPath">Path to the image that will be used as the class' icon.</param>
    public unsafe void SetIcon(string? iconPath)
    {
        IconPath = iconPath;
    }
}
