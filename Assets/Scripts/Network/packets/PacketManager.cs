﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;

public class PacketManager : MonoBehaviourSingleton<PacketManager>, IReceiveData
{
    private Dictionary<int, Action<int, PacketType, Stream>> packetReceived = new Dictionary<int, System.Action<int, PacketType, Stream>>();
    private int currentPacketId = 0;

    public Action<int, PacketType, Stream> ReceivingPacket;
    public Action<PacketType, Stream, IPEndPoint> ReceivingInternalPacket;

    protected override void Initialize()
    {
        base.Initialize();
        NetworkManager.Instance.OnReceiveEvent += OnReceiveData;
    }

    public void SendPacket(SerializablePacket packet, int objectId)
    {
        byte[] bytes = Serialize(packet, objectId);
        if (NetworkManager.Instance.isServer)
        {
            Broadcast(bytes);
        }
        else
        {
            NetworkManager.Instance.SendToServer(bytes);
        }
    }

    public void SendPacketToClient(SerializablePacket packet, IPEndPoint iPEnd)
    {
        byte[] bytes = Serialize(packet);
        NetworkManager.Instance.SendToClient(bytes, iPEnd);
    }

    public void SendPacketToServer(SerializablePacket packet)
    {
        byte[] bytes = Serialize(packet);
        NetworkManager.Instance.SendToServer(bytes);
    }

    public void Broadcast(byte[] bytes)
    {
        using (var iterator = NetworkManager.Instance.clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                NetworkManager.Instance.SendToClient(bytes, iterator.Current.Value._ipEndPoint);
            }
        }
    }

    private byte[] Serialize(SerializablePacket packet, int objectId = 0)
    {
        PacketHeader header = new PacketHeader();
        MemoryStream memoryStream = new MemoryStream();
        header.protocolId = currentPacketId;
        header.type = (PacketType)packet.packetType;
        Debug.Log(header.type);

        if (!NetworkManager.Instance.isServer)
        {
            UserPacketHeader userHeader = new UserPacketHeader();
            userHeader.id = currentPacketId;
            userHeader.clientId = NetworkManager.Instance.clientId;
            userHeader.objectId = objectId;
            userHeader.Serialize(memoryStream);
        }
        header.Serialize(memoryStream);
        packet.Serialize(memoryStream);
        memoryStream.Close();
        return memoryStream.ToArray();
    }

    public void OnReceiveData(byte[] data, IPEndPoint endPoint)
    {
        PacketHeader header = new PacketHeader();
        MemoryStream memoryStream = new MemoryStream(data);
        header.Deserialize(memoryStream);
        if ((PacketType)header.type == PacketType.User)
        {
            UserPacketHeader userHeader = new UserPacketHeader();
            userHeader.Deserialize(memoryStream);
            InvokeCallback(userHeader.objectId, header.type, memoryStream);
        }
        else
        {
            ReceivingInternalPacket.Invoke(header.type, memoryStream, endPoint);
        }
        memoryStream.Close();
    }

    private void InvokeCallback(int objectId, PacketType type, Stream stream)
    {
        if (packetReceived.ContainsKey(objectId))
        {
            packetReceived[objectId].Invoke(objectId, type, stream);
        }
    } 
}