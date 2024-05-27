using System;

namespace Godot;

/// <summary>
/// Disable the generation of the GDExtension entry-point, so it can
/// be manually implemented.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class DisableGodotEntryPointGenerationAttribute : Attribute { }
