using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.Message
{
    public class NetMessage
    {
        protected OpCode Code { set; get; }

        public virtual void Serialize(ref DataStreamWriter writer) {
            writer.WriteByte((byte)Code);
        }
        public virtual void Deserialize(DataStreamReader reader) {
        
        }

        public virtual void ReceivedOnClient() {

        }
        public virtual void ReceivedOnServer(NetworkConnection nc) {

        }
    }
}
