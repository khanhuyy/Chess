using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.Message
{
    public sealed class NetKeepAlive : NetMessage
    {
        public NetKeepAlive() { // Making pack
            Code = OpCode.KeepAlive;
        }

        public NetKeepAlive(DataStreamReader reader) { // Receive package
            Code = OpCode.KeepAlive;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer) {
            writer.WriteByte((byte)Code);
        }

        public override void Deserialize(DataStreamReader reader) {

        }

        public override void ReceivedOnClient() {
            NetUtility.CKeepAlive?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection nc) {
            NetUtility.SKeepAlive?.Invoke(this, nc);
        }
    }
}
