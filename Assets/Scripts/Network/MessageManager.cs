using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviourSingleton<MessageManager>
{
    public void SendMessage(string message, int objectId)
    {
        StringPacket packet = new StringPacket();
        packet.payload.message = message;
        // PacketManager pManager = new PacketManager();
        PacketManager.Instance.SendPacket(packet, objectId);
    }
}
