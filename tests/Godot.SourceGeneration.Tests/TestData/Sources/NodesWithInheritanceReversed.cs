using Godot;
using Godot.Bridge;

namespace NS;

[GodotClass]
public partial class Bat : Enemy
{
    public new static void BindMembers(ClassRegistrationContext context) { }
}

[GodotClass]
public partial class Enemy : Mob
{
    public new static void BindMembers(ClassRegistrationContext context) { }
}

[GodotClass]
public partial class Mob : Node
{
    public static void BindMembers(ClassRegistrationContext context) { }
}
