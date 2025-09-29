using System;
using Godot;

public partial class MyNode : Node
{
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void RemoteMethod() { }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void RemoteSyncMethod() { }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SyncMethod() { }

    [Rpc]
    public void SlaveMethod() { }

    [Rpc]
    public void PuppetMethod() { }

    [Rpc(CallLocal = true)]
    public void PuppetSyncMethod() { }

    [{|CS0246:{|CS0246:Master|}|}]
    public void MasterMethod() { }

    [{|CS0246:{|CS0246:MasterSync|}|}]
    public void MasterSyncMethod() { }

    [OtherAttribute, Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void MultipleAttributesMethod1() { }

    [OtherAttribute]
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void MultipleAttributesMethod2() { }
}

[AttributeUsage(AttributeTargets.Method)]
internal class OtherAttribute : Attribute { }
