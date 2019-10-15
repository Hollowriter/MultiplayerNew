using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;
    // public PacketManager pManager = new PacketManager();

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);
        this.gameObject.SetActive(false);
        PacketManager.Instance.ReceivingPacket += OnReceivePacket;
    }

    void OnReceivePacket(int id, PacketType type, Stream stream)
    {
        if (type == (int)MainPacketType.Message)
        {
            StringPacket packet = new StringPacket();
            packet.Deserialize(stream);
            if (NetworkManager.Instance.isServer)
            {
                MessageManager manager = new MessageManager();
                manager.SendMessage(packet.payload, 0);
            }
            messages.text += packet.payload + System.Environment.NewLine;
        }
    }

    void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {
        if (NetworkManager.Instance.isServer)
        {
            // NetworkManager.Instance.Broadcast(data);
            PacketManager.Instance.Broadcast(data);
        }
        messages.text += System.Text.ASCIIEncoding.UTF8.GetString(data) + System.Environment.NewLine;
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            MessageManager manager = new MessageManager();
            if (NetworkManager.Instance.isServer)
            {
                // NetworkManager.Instance.Broadcast(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                manager.SendMessage(inputMessage.text, 0);
                messages.text += inputMessage.text + System.Environment.NewLine;
            }
            else
            {
                // NetworkManager.Instance.Broadcast(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text)); // Crear packet de texto
                manager.SendMessage(inputMessage.text, 0);
            }            
            inputMessage.ActivateInputField();
            inputMessage.Select();        
            inputMessage.text = "";
        }
    }
}
