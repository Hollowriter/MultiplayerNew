using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConnectionRequestInformation
{
    public long clientSalt;
}

public class ConnectionRequestPacket : NetworkPacket<ConnectionRequestInformation>
{
    public ConnectionRequestPacket() : base (PacketType.ConnectionRequest)
    {
    }

    public override void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload.clientSalt);
    }

    public override void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload.clientSalt = br.ReadInt64();
    }
}

public struct ChallengeRequestInformation
{
    public int clientId;
    public long clientSalt;
    public long serverSalt;
}

public class ChallengeRequestPacket : NetworkPacket<ChallengeRequestInformation>
{
    public ChallengeRequestPacket() : base(PacketType.ChallengeRequest)
    {
    }

    public override void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload.clientId);
        bw.Write(payload.clientSalt);
        bw.Write(payload.serverSalt);
    }

    public override void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload.clientId = br.ReadInt32();
        payload.clientSalt = br.ReadInt64();
        payload.serverSalt = br.ReadInt64();
    }
}

public struct ChallengeResponseInformation
{
    public long result;
}

public class ChallengeResponsePacket : NetworkPacket<ChallengeResponseInformation>
{
    public ChallengeResponsePacket() : base(PacketType.ChallengeResponse)
    {
    }

    public override void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload.result);
    }

    public override void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload.result = br.ReadInt64();
    }
}

public struct ConnectionInformation
{
    public int clientId;
}

public class ConnectionPacket : NetworkPacket<ConnectionInformation>
{
    public ConnectionPacket() : base(PacketType.Connected)
    {
    }

    public override void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload.clientId);
    }

    public override void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload.clientId = br.ReadInt32();
    }
}
