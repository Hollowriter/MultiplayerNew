using System.Net;
using System.IO;

public class UserPacketHeader // RECORDATORIO completar el packet header como corresponde
{
    public int id;
    public int clientId;
    public int objectId;
    // public PacketType type;

    public void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        bw.Write(id);
        bw.Write(clientId);
        bw.Write(objectId);
    }

    public void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        id = br.ReadInt32();
        clientId = br.ReadInt32();
        objectId = br.ReadInt32();
    }
}

public class PacketHeader
{
    public int protocolId;
    public PacketType type;

    public void Serialize(Stream stream)
    {
        BinaryWriter bw = new BinaryWriter(stream);

        bw.Write(protocolId);
        bw.Write((byte)type);
    }

    public void Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        protocolId = br.ReadInt32();
        type = (PacketType)br.ReadInt32();
    }
}

public abstract class NetworkPacket<P> : SerializablePacket
{
    public PacketType packetType { get; set; }
    // public int clientId;
    // public IPEndPoint ipEndPoint;
    // public float timeStamp;
    public P payload;

    public virtual void Serialize(Stream stream)
    {
        /*BinaryWriter bw = new BinaryWriter(stream);
        bw.Write((int)packetType);*/
    }

    public virtual void Deserialize(Stream stream)
    {
        /*BinaryReader br = new BinaryReader(stream);
        packetType = (PacketType)br.ReadInt32();*/
    }

    public NetworkPacket(PacketType type/*, P data*/ /*float timeStamp,int clientId = -1*//*, IPEndPoint ipEndPoint = null*/)
    {
        this.packetType = type;
        // this.timeStamp = timeStamp;
        /*this.clientId = clientId;*/
        // this.ipEndPoint = ipEndPoint;
        // this.payload = data;
    }
}