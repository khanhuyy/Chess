using ChessPiece;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.Message
{
    public sealed class NetWelcome : NetMessage
    {
        public ChessPieceTeam AssignedTeam { set; get; }

        public NetWelcome() {
            Code = OpCode.Welcome;
        }
        public NetWelcome(DataStreamReader reader) {
            Code = OpCode.Welcome;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer) {
            writer.WriteByte((byte)Code);
            // writer.WriteByte((byte)AssignedTeam);
        
            // Define that 0 for ChessPieceTeam.White and 1 for ChessPieceTeam.Black
            int definedTeam = ChessPieceTeam.White == AssignedTeam ? 0 : 1;
            writer.WriteInt(definedTeam);
        }

        public override void Deserialize(DataStreamReader reader) {
            // Define that 0 for ChessPieceTeam.White and 1 for ChessPieceTeam.Black
            AssignedTeam = reader.ReadInt() == 0 ? ChessPieceTeam.White : ChessPieceTeam.Black;
        }

        public override void ReceivedOnClient() {
            NetUtility.CWelcome?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection nc) {
            NetUtility.SWelcome?.Invoke(this, nc);
        }
    }
}
