using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;

public class NetMakeMove : NetMessage
{
    public int originalColumn;
    public int originalRow;
    public int destinationColumn;
    public int destinationRow;
    public ChessmanTeam team;

    public ChessmanTeam AssignedTeam { set; get; }

    public NetMakeMove() {
        Code = OpCode.MAKE_MOVE;
    }
    public NetMakeMove(DataStreamReader reader) {
        Code = OpCode.MAKE_MOVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
        writer.WriteInt(originalColumn);
        writer.WriteInt(originalRow);
        writer.WriteInt(destinationColumn);
        writer.WriteInt(destinationRow);
        // writer.WriteByte((byte)team);
        writer.WriteInt(team == ChessmanTeam.White ? 0 : 1);
    }

    public override void Deserialize(DataStreamReader reader) {
        originalColumn = reader.ReadInt();
        originalRow = reader.ReadInt();
        destinationColumn = reader.ReadInt();
        destinationRow = reader.ReadInt();
        team = reader.ReadInt() == 0 ? ChessmanTeam.White : ChessmanTeam.Black;
    }

    public override void ReceivedOnClient() {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection nc) {
        NetUtility.S_MAKE_MOVE?.Invoke(this, nc);
    }
}
