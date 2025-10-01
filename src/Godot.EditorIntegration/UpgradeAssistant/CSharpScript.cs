using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot.Bridge;
using Godot.NativeInterop;

namespace Godot.EditorIntegration.UpgradeAssistant;

[GodotClass]
internal sealed partial class CSharpScript : ScriptExtension
{
    private sealed class CSharpScriptInstance
    {
        internal nint NativePtr;
        private readonly GCHandle _gcHandle;

        public CSharpScript Script { get; }

        private readonly unsafe GDExtensionScriptInstanceInfo3* _creationInfo;

        private readonly Dictionary<StringName, Variant> _propertyValues = [];

        private readonly PropertyInfoList _properties = [];
        internal PropertyInfoList GetPropertyListStorage() => _properties;

        public unsafe CSharpScriptInstance(CSharpScript script)
        {
            Script = script;
            _gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);

            var creationInfo = (GDExtensionScriptInstanceInfo3*)NativeMemory.Alloc((nuint)sizeof(GDExtensionScriptInstanceInfo3));

            *creationInfo = new GDExtensionScriptInstanceInfo3()
            {
                set_func = &Set_Native,
                get_func = &Get_Native,
                get_property_list_func = &GetPropertyList_Native,
                free_property_list_func = &FreePropertyList_Native,
                get_property_type_func = &GetPropertyType_Native,
                get_script_func = &GetScript_Native,
                free_func = &Free_Native,
            };

            NativePtr = (nint)GodotBridge.GDExtensionInterface.script_instance_create3(creationInfo, (void*)GCHandle.ToIntPtr(_gcHandle));
        }

        public bool Set(StringName property, Variant value)
        {
            _propertyValues[property] = value;
            return true;
        }

        public bool Get(StringName property, out Variant value)
        {
            return _propertyValues.TryGetValue(property, out value);
        }

        public void GetPropertyList(PropertyInfoList properties)
        {
            foreach (var (name, value) in _propertyValues)
            {
                properties.Add(new PropertyInfo(name, value.VariantType)
                {
                    Usage = PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable,
                });
            }
        }

        public bool TryGetPropertyType(StringName property, out VariantType variantType)
        {
            if (!_propertyValues.TryGetValue(property, out var value))
            {
                variantType = VariantType.Nil;
                return false;
            }

            variantType = value.VariantType;
            return true;
        }

        public static unsafe void FreeNative(CSharpScriptInstance instance)
        {
            instance._gcHandle.Free();
            NativeMemory.Free(instance._creationInfo);
        }
    }

    protected override bool _CanInstantiate() => false;

    protected override unsafe void* _InstanceCreate(GodotObject forObject)
    {
        // All instances are placeholders.
        // These C# scripts are only used for upgrading
        // from GodotSharp and can't be instantiated.
        return _PlaceholderInstanceCreate(forObject);
    }

    protected override unsafe void* _PlaceholderInstanceCreate(GodotObject forObject)
    {
        var scriptInstance = new CSharpScriptInstance(this);
        return (void*)scriptInstance.NativePtr;
    }

    protected override bool _HasScriptSignal(StringName signal)
    {
        return false;
    }

    protected override bool _IsValid()
    {
        return true;
    }

    protected override bool _HasPropertyDefaultValue(StringName property)
    {
        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool Set_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* value)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            StringName nameManaged = StringName.CreateCopying(*name);
            Variant valueManaged = Variant.CreateCopying(*value);

            instanceObj.Set(nameManaged, valueManaged);
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe bool Get_Native(void* instance, NativeGodotStringName* name, NativeGodotVariant* value)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            using StringName nameManaged = StringName.CreateCopying(*name);

            bool ok = instanceObj.Get(nameManaged, out Variant valueManaged);

            *value = valueManaged.NativeValue.DangerousSelfRef;
            return ok;
        }

        return false;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe GDExtensionPropertyInfo* GetPropertyList_Native(void* instance, uint* outCount)
    {
        if (instance is null)
        {
            if (outCount is not null)
            {
                *outCount = 0;
            }
            return null;
        }

        var gcHandle = GCHandle.FromIntPtr((nint)instance);
        var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

        Debug.Assert(instanceObj is not null);

        var propertyList = instanceObj.GetPropertyListStorage();
        Debug.Assert(propertyList.Count == 0, "Internal error, property list was not freed by engine!");

        instanceObj.GetPropertyList(propertyList);

        GDExtensionPropertyInfo* propertyListPtr = PropertyInfoList.ConvertToNative(propertyList);

        if (outCount is not null)
        {
            *outCount = (uint)propertyList.Count;
        }
        return propertyListPtr;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void FreePropertyList_Native(void* instance, GDExtensionPropertyInfo* propertyListPtr, uint count)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            var propertyList = instanceObj.GetPropertyListStorage();
            propertyList.Clear();

            PropertyInfoList.FreeNative(propertyListPtr, (int)count);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe GDExtensionVariantType GetPropertyType_Native(void* instance, NativeGodotStringName* name, bool* outIsValid)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            using StringName nameManaged = StringName.CreateCopying(*name);

            *outIsValid = instanceObj.TryGetPropertyType(nameManaged, out var variantType);
            return (GDExtensionVariantType)variantType;
        }

        *outIsValid = false;
        return GDExtensionVariantType.GDEXTENSION_VARIANT_TYPE_NIL;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void* GetScript_Native(void* instance)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            return (void*)instanceObj.Script.NativePtr;
        }

        return null;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void Free_Native(void* instance)
    {
        if (instance is not null)
        {
            var gcHandle = GCHandle.FromIntPtr((nint)instance);
            var instanceObj = (CSharpScriptInstance?)gcHandle.Target;

            Debug.Assert(instanceObj is not null);

            CSharpScriptInstance.FreeNative(instanceObj);
        }
    }
}
