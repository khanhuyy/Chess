using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine;

public class NetWelcome : NetMessage
{
    public ChessmanTeam AssignedTeam { set; get; }

    public NetWelcome() {
        Code = OpCode.WELCOME;
    }
    public NetWelcome(DataStreamReader reader) {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)Code);
        // writer.WriteByte((byte)AssignedTeam);
        
        // Define that 0 for ChessmanTeam.White and 1 for ChessmanTeam.Black
        int definedTeam = ChessmanTeam.White == AssignedTeam ? 0 : 1;
        writer.WriteInt(definedTeam);
    }

    public override void Deserialize(DataStreamReader reader) {
        // Define that 0 for ChessmanTeam.White and 1 for ChessmanTeam.Black
        AssignedTeam = reader.ReadInt() == 0 ? ChessmanTeam.White : ChessmanTeam.Black;
    }

    public override void ReceivedOnClient() {
        NetUtility.C_WELCOME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection nc) {
        NetUtility.S_WELCOME?.Invoke(this, nc);
    }
}
