using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;

public class NetRematch : NetMessage
{
    public ChessmanTeam team;
    public byte wantRematch;

    public ChessmanTeam AssignedTeam { set; get; }

    public NetRematch()
    {
        Code = OpCode.REMATCH;
    }
    public NetRematch(DataStreamReader reader)
    {
        Code = OpCode.REMATCH;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        // writer.WriteByte((byte)team);
        writer.WriteInt(team == ChessmanTeam.White ? 0 : 1);
        writer.WriteByte(wantRematch);
    }

    public override void Deserialize(DataStreamReader reader)
    {
        team = reader.ReadInt() == 0 ? ChessmanTeam.White : ChessmanTeam.Black;
        wantRematch = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_REMATCH?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection nc)
    {
        NetUtility.S_REMATCH?.Invoke(this, nc);
    }
}
