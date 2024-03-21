using System.Collections.Generic;
using Godot.Bridge;

namespace Godot;

partial class GodotObject
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// Called when the object receives a notification, which can be identified in
    /// <paramref name="what"/> by comparing it with a constant. See also
    /// <see cref="Notification(int, bool)"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override void _Notification(int what)
    /// {
    ///     if (what == NotificationPredelete)
    ///     {
    ///         GD.Print("Goodbye!");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <param name="what">Identifies the notification that was received.</param>
    protected internal virtual void _Notification(int what) { }

    /// <summary>
    /// Override this method to customize the behavior of <see cref="Set(StringName, Variant)"/>.
    /// Should set the property to <paramref name="value"/> and return <see langword="true"/>,
    /// or <see langword="false"/> if the property should be handled normally.
    /// The <i>exact</i> way to set the property is up to this method's implementation.
    /// Combined with <see cref="_Get(StringName, out Variant)"/> and
    /// <see cref="_GetPropertyList(IList{PropertyInfo})"/>, this method allows defining custom
    /// properties, which is particularly useful for editor plugin. Note that a property
    /// <i>must</i> be present in <see cref="GetPropertyList"/>, otherwise this method will
    /// not be called.
    /// </summary>
    /// <example>
    /// <code>
    /// private GodotDictionary _internalData = [];
    ///
    /// protected override bool _Set(StringName property, Variant value)
    /// {
    ///     if (property == new StringName("FakeProperty"))
    ///     {
    ///         // Storing the value in the fake property.
    ///         _internalData["FakeProperty"] = value;
    ///         return true;
    ///     }
    ///
    ///     return false;
    /// }
    ///
    /// protected override void _GetPropertyList(IList&lt;PropertyInfo&gt; properties)
    /// {
    ///     properties.Add(new PropertyInfo(VariantType.Int, new StringName("FakeProperty")));
    /// }
    /// </code>
    /// </example>
    /// <param name="property">Name of the property to handle.</param>
    /// <param name="value">The value that should be assigned to the property.</param>
    /// <returns>Whether the property was handled.</returns>
    protected internal virtual bool _Set(StringName property, Variant value)
    {
        return false;
    }

    /// <summary>
    /// Override this method to customize the behavior of <see cref="Get(StringName)"/>.
    /// Should set <paramref name="value"/> to the given property's value and return
    /// <see langword="true"/>, or <see langword="false"/> if the property should be
    /// handled normally.
    /// Combined with <see cref="_Set(StringName, Variant)"/> and
    /// <see cref="_GetPropertyList(IList{PropertyInfo})"/>, this method allows defining custom
    /// properties, which is particularly useful for editor plugin. Note that a property
    /// <i>must</i> be present in <see cref="GetPropertyList"/>, otherwise this method will
    /// not be called.
    /// </summary>
    /// <example>
    /// <code>
    /// private GodotDictionary _internalData = [];
    ///
    /// protected override bool _Get(StringName property, out Variant value)
    /// {
    ///     if (property == new StringName("FakeProperty"))
    ///     {
    ///         GD.Print("Getting my property!");
    ///         value = 4;
    ///         return true;
    ///     }
    ///
    ///     value = default;
    ///     return false;
    /// }
    ///
    /// protected override void _GetPropertyList(IList&lt;PropertyInfo&gt; properties)
    /// {
    ///     properties.Add(new PropertyInfo(VariantType.Int, new StringName("FakeProperty")));
    /// }
    /// </code>
    /// </example>
    /// <param name="property">Name of the property to handle.</param>
    /// <param name="value">The current value for the property.</param>
    /// <returns>Whether the property was handled.</returns>
    protected internal virtual bool _Get(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    /// <summary>
    /// Override this method to provide a custom list of additional properties to
    /// handle by the engine.
    /// Should add properties to <paramref name="properties"/>. The result is added
    /// to the array of <see cref="GetPropertyList"/>.
    /// You can use <see cref="_PropertyCanRevert(StringName)"/> and
    /// <see cref="_PropertyGetRevert(StringName, out Variant)"/> to customize
    /// the default values of the properties added by this method.
    /// </summary>
    /// <example>
    /// The example below displays a list of numbers shown as words going from
    /// <c>Zero</c> to <c>Five</c>, with <c>_numberCount</c> controlling the size
    /// of the list:
    /// <code>
    /// [GodotClass(Tool = true)]
    /// public partial class MyNode : Node
    /// {
    ///     private int _numberCount;
    ///
    ///     [BindProperty]
    ///     public int NumberCount
    ///     {
    ///         get =&gt; _numberCount;
    ///         set
    ///         {
    ///             _numberCount = value;
    ///             _numbers.Resize(_numberCount);
    ///             NotifyPropertyListChanged();
    ///         }
    ///     }
    ///
    ///     private GodotArray&lt;int&gt; _numbers = new();
    ///
    ///     protected override void _GetPropertyList(IList&lt;PropertyInfo&gt; properties)
    ///     {
    ///         for (int i = 0; i &lt; _numberCount; i++)
    ///         {
    ///             properties.Add(new PropertyInfo(VariantType.Int, new StringName($"number_{i}"))
    ///             {
    ///                 Hint = PropertyHint.Enum,
    ///                 HintString = "Zero,One,Two,Three,Four,Five",
    ///             });
    ///         }
    ///     }
    ///
    ///     protected override bool _Get(StringName property, out Variant value)
    ///     {
    ///         string propertyName = property.ToString();
    ///         if (propertyName.StartsWith("number_"))
    ///         {
    ///             int index = int.Parse(propertyName.Substring("number_".Length));
    ///             return _numbers[index];
    ///         }
    ///         return default;
    ///     }
    ///
    ///     protected override bool _Set(StringName property, Variant value)
    ///     {
    ///         string propertyName = property.ToString();
    ///         if (propertyName.StartsWith("number_"))
    ///         {
    ///             int index = int.Parse(propertyName.Substring("number_".Length));
    ///             numbers[index] = value.As&lt;int&gt;();
    ///             return true;
    ///         }
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <param name="properties">The list of properties that should be populated.</param>
    protected internal virtual void _GetPropertyList(IList<PropertyInfo> properties) { }

    /// <summary>
    /// Override this method to customize the given property's revert behavior.
    /// Should return <see langword="true"/> if the property has a custom default
    /// value and is revertible in the Inspector dock.
    /// Use <see cref="_PropertyGetRevert(StringName, out Variant)"/> to specify
    /// the property's default value.
    /// <b>Note:</b> This method must return consistently, regardless of the current
    /// value of the property.
    /// </summary>
    /// <param name="property">Name of the property to handle.</param>
    /// <returns>Whether the given property can be reverted.</returns>
    protected internal virtual bool _PropertyCanRevert(StringName property)
    {
        return false;
    }

    /// <summary>
    /// Override this method to customize the given property's revert behavior.
    /// Should return the default value for the property. If the default value
    /// differs from the property's current value, a revert icon is displayed in
    /// the Inspector dock.
    /// <b>Note:</b> <see cref="PropertyCanRevert(StringName)"/> must also be
    /// overridden for this method to be called.
    /// </summary>
    /// <param name="property">Name of the property to handle.</param>
    /// <param name="value">Default value for the given property.</param>
    /// <returns>Whether the given property has a default value.</returns>
    protected internal virtual bool _PropertyGetRevert(StringName property, out Variant value)
    {
        value = default;
        return false;
    }

    /// <summary>
    /// Override this method to customize existing properties. Every property info
    /// goes through this method, except properties added with
    /// <see cref="_GetPropertyList(IList{PropertyInfo})"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// [GodotClass(Tool = true)]
    /// public partial class MyNode : Node
    /// {
    ///     private bool _isNumberEditable;
    ///
    ///     [BindProperty]
    ///     public bool IsNumberEditable
    ///     {
    ///         get =&gt; _isNumberEditable;
    ///         set
    ///         {
    ///             _isNumberEditable = value;
    ///             NotifyPropertyListChanged();
    ///         }
    ///     }
    ///
    ///     [BindProperty]
    ///     public int Number { get; set; }
    ///
    ///     protected override void _ValidateProperty(PropertyInfo property)
    ///     {
    ///         if (property.Name == PropertyName.Number &amp;&amp; !IsNumberEditable)
    ///         {
    ///             property.Usage |= PropertyUsageFlags.ReadOnly;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <param name="property">Property to validate.</param>
    protected internal virtual void _ValidateProperty(PropertyInfo property) { }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1716 // Identifiers should not match keywords
#pragma warning restore CA1707 // Identifiers should not contain underscores
}
