using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainPacketType
{
    Message
}

public abstract class GamePacket<P> : NetworkPacket<P>
{
    public GamePacket(MainPacketType type) : base(PacketType.User)
    {
    }
}

public class StringPacket : GamePacket<string>
{
    public StringPacket() : base(MainPacketType.Message)
    {
    }

    public override void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload);
    }

    public override void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload = br.ReadString();
    }
}
