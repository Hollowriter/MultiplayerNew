using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    ConnectionRequest,
    ChallengeRequest,
    ChallengeResponse,
    Connected,
    Disconnected,
    User
}

public interface SerializablePacket
{
    PacketType packetType { get; set; }
    UserPacketType userType { get; set; }

    void Serialize(Stream stream);
    void Deserialize(Stream stream);
}
