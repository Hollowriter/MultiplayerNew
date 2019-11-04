using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UserPacketType
{
    Message
}

public abstract class GamePacket<P> : NetworkPacket<P>
{
    public GamePacket(UserPacketType uType) : base(PacketType.User)
    {
        this.userType = uType;
    }
}

public struct StringMessage
{
    public string message;
}

public class StringPacket : GamePacket<StringMessage>
{
    public StringPacket() : base(UserPacketType.Message)
    {
    }

    public override void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write(payload.message);
    }

    public override void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);
        payload.message = br.ReadString();
    }
}
