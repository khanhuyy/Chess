using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive() { // Making pack
        Code = OpCode.KEEP_ALIVE;
    }

    public NetKeepAlive(DataStreamReader reader) { // Receive package
        Code = OpCode.KEEP_ALIVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
    }

    public override void Deserialize(DataStreamReader reader) {

    }

    public override void ReceivedOnClient() {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection nc) {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, nc);
    }
}
