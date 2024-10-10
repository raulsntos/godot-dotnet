using Godot;
using Godot.Bridge;

namespace NS;

[GodotClass]
public partial class DerivedType : BaseType
{
    public new static void BindMethods(ClassRegistrationContext context) { }
}

[GodotClass]
public partial class DerivedType2 : BaseType
{
    public new static void BindMethods(ClassRegistrationContext context) { }
}

[GodotClass]
public partial class BaseType : Node
{
    public static void BindMethods(ClassRegistrationContext context) { }
}

[GodotClass]
public partial class HighlyDerivedType : DerivedType3
{
    public new static void BindMethods(ClassRegistrationContext context) { }
}

[GodotClass]
public partial class DerivedType3 : BaseType
{
    public new static void BindMethods(ClassRegistrationContext context) { }
}
