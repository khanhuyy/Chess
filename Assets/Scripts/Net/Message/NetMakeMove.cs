using ChessPiece;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.Message
{
    public sealed class NetMakeMove : NetMessage
    {
        public int OriginalColumn;
        public int OriginalRow;
        public int DestinationColumn;
        public int DestinationRow;
        public ChessPieceTeam Team;

        public ChessPieceTeam AssignedTeam { set; get; }

        public NetMakeMove() {
            Code = OpCode.MakeMove;
        }
        public NetMakeMove(DataStreamReader reader) {
            Code = OpCode.MakeMove;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer) {
            writer.WriteByte((byte)Code);
            writer.WriteInt(OriginalColumn);
            writer.WriteInt(OriginalRow);
            writer.WriteInt(DestinationColumn);
            writer.WriteInt(DestinationRow);
            // writer.WriteByte((byte)team);
            writer.WriteInt(Team == ChessPieceTeam.White ? 0 : 1);
        }

        public override void Deserialize(DataStreamReader reader) {
            OriginalColumn = reader.ReadInt();
            OriginalRow = reader.ReadInt();
            DestinationColumn = reader.ReadInt();
            DestinationRow = reader.ReadInt();
            Team = reader.ReadInt() == 0 ? ChessPieceTeam.White : ChessPieceTeam.Black;
        }

        public override void ReceivedOnClient() {
            NetUtility.CMakeMove?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection nc) {
            NetUtility.SMakeMove?.Invoke(this, nc);
        }
    }
}
