using System;
using Godot;

public partial class MyNode : Node
{
    [{|GUA1005:Remote|}]
    public void RemoteMethod() { }

    [{|GUA1005:RemoteSync|}]
    public void RemoteSyncMethod() { }

    [{|GUA1005:Sync|}]
    public void SyncMethod() { }

    [{|GUA1005:Slave|}]
    public void SlaveMethod() { }

    [{|GUA1005:Puppet|}]
    public void PuppetMethod() { }

    [{|GUA1005:PuppetSync|}]
    public void PuppetSyncMethod() { }

    [Master]
    public void MasterMethod() { }

    [MasterSync]
    public void MasterSyncMethod() { }

    [OtherAttribute, {|GUA1005:Remote|}]
    public void MultipleAttributesMethod1() { }

    [OtherAttribute]
    [{|GUA1005:Remote|}]
    public void MultipleAttributesMethod2() { }
}

[AttributeUsage(AttributeTargets.Method)]
internal class OtherAttribute : Attribute { }
