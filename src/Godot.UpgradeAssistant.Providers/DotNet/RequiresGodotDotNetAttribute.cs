using System;

namespace Godot.UpgradeAssistant.Providers;

/// <summary>
/// Indicates that the annotated analyzer is only valid when upgrading to the
/// new GDExtension-based Godot .NET bindings.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal class RequiresGodotDotNetAttribute : Attribute { }
