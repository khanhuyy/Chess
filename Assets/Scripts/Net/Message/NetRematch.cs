using ChessPiece;
using Unity.Collections;
using Unity.Networking.Transport;

namespace Net.Message
{
    public sealed class NetRematch : NetMessage
    {
        public ChessPieceTeam Team;
        public byte WantRematch;

        public ChessPieceTeam AssignedTeam { set; get; }

        public NetRematch()
        {
            Code = OpCode.Rematch;
        }
        public NetRematch(DataStreamReader reader)
        {
            Code = OpCode.Rematch;
            Deserialize(reader);
        }

        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte((byte)Code);
            // writer.WriteByte((byte)team);
            writer.WriteInt(Team == ChessPieceTeam.White ? 0 : 1);
            writer.WriteByte(WantRematch);
        }

        public override void Deserialize(DataStreamReader reader)
        {
            Team = reader.ReadInt() == 0 ? ChessPieceTeam.White : ChessPieceTeam.Black;
            WantRematch = reader.ReadByte();
        }

        public override void ReceivedOnClient()
        {
            NetUtility.CRematch?.Invoke(this);
        }

        public override void ReceivedOnServer(NetworkConnection nc)
        {
            NetUtility.SRematch?.Invoke(this, nc);
        }
    }
}
