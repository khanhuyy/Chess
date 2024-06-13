using ChessPiece;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.Message
{
    public sealed class NetStartGame : NetMessage
    {
        public ChessPieceTeam AssignedTeam { set; get; }

        public NetStartGame() {
            Code = OpCode.StartGame;
        }
        public NetStartGame(DataStreamReader reader) {
            Code = OpCode.StartGame;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer) {
            writer.WriteByte((byte)Code);
        }

        public override void Deserialize(DataStreamReader reader) {

        }

        public override void ReceivedOnClient() {
            NetUtility.CStartGame?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection nc) {
            NetUtility.SStartGame?.Invoke(this, nc);
        }
    }
}
