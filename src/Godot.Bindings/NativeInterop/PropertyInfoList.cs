using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot.Bridge;

namespace Godot.NativeInterop;

internal sealed class PropertyInfoList : IList<PropertyInfo>
{
    private readonly List<PropertyInfo> _managedValues = [];
    private readonly List<GDExtensionPropertyInfo> _nativeValues = [];

    internal Span<GDExtensionPropertyInfo> AsSpan()
    {
        return CollectionsMarshal.AsSpan(_nativeValues);
    }

    public PropertyInfo this[int index]
    {
        get => _managedValues[index];
        set
        {
            _managedValues[index] = value;
            _nativeValues[index] = ConvertPropertyInfoToNative(value);
        }
    }

    public int Count => _managedValues.Count;

    bool ICollection<PropertyInfo>.IsReadOnly => false;

    public void Add(PropertyInfo item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _managedValues.Add(item);
        _nativeValues.Add(ConvertPropertyInfoToNative(item));
    }

    public void AddRange(IEnumerable<PropertyInfo> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _managedValues.AddRange(items);
        _nativeValues.AddRange(items.Select(ConvertPropertyInfoToNative));
    }

    public void AddRange(ReadOnlySpan<PropertyInfo> items)
    {
        _managedValues.AddRange(items);

        int oldCount = _nativeValues.Count;
        CollectionsMarshal.SetCount(_nativeValues, oldCount + items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            _nativeValues[oldCount + i] = ConvertPropertyInfoToNative(items[i]);
        }
    }

    public bool Contains(PropertyInfo item)
    {
        return _managedValues.Contains(item);
    }

    public void Clear()
    {
        _managedValues.Clear();
        _nativeValues.Clear();
    }

    public int IndexOf(PropertyInfo item)
    {
        return _managedValues.IndexOf(item);
    }

    public void Insert(int index, PropertyInfo item)
    {
        _managedValues.Insert(index, item);
    }

    public bool Remove(PropertyInfo item)
    {
        int index = IndexOf(item);
        if (index != -1)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        _managedValues.RemoveAt(index);
        _nativeValues.RemoveAt(index);
    }

    void ICollection<PropertyInfo>.CopyTo(PropertyInfo[] array, int arrayIndex)
    {
        _managedValues.CopyTo(array, arrayIndex);
    }

    public IEnumerator<PropertyInfo> GetEnumerator()
    {
        return _managedValues.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private unsafe static GDExtensionPropertyInfo ConvertPropertyInfoToNative(PropertyInfo propertyInfo)
    {
        return new GDExtensionPropertyInfo()
        {
            type = (GDExtensionVariantType)propertyInfo.Type,
            name = propertyInfo.Name.NativeValue.DangerousSelfRef.GetUnsafeAddress(),
            hint = (uint)propertyInfo.Hint,
            hint_string = NativeGodotString.Create(propertyInfo.HintString).GetUnsafeAddress(),
            class_name = (propertyInfo.ClassName?.NativeValue ?? default).DangerousSelfRef.GetUnsafeAddress(),
            usage = (uint)propertyInfo.Usage,
        };
    }
}
