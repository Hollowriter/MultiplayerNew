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

    void OnReceivePacket(int id, UserPacketType type, Stream stream)
    {
        Debug.Log("ElChatScreenRecibe---: " + type);
        if (type == (byte)UserPacketType.Message)
        {
            Debug.Log("---Y es un mensaje");
            StringPacket packet = new StringPacket();
            packet.Deserialize(stream);
            if (NetworkManager.Instance.isServer)
            {
                // MessageManager manager = new MessageManager();
                MessageManager.Instance.SendMessage(packet.payload.message, 0);
            }
            messages.text += packet.payload.message + System.Environment.NewLine;
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
            // MessageManager manager = new MessageManager();
            if (NetworkManager.Instance.isServer)
            {
                // NetworkManager.Instance.Broadcast(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                MessageManager.Instance.SendMessage(inputMessage.text, 0);
                messages.text += inputMessage.text + System.Environment.NewLine;
            }
            else
            {
                // NetworkManager.Instance.Broadcast(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text)); // Crear packet de texto
                MessageManager.Instance.SendMessage(inputMessage.text, 0);
                messages.text += inputMessage.text + System.Environment.NewLine;
                Debug.Log("soy un cliente");
            }            
            inputMessage.ActivateInputField();
            inputMessage.Select();        
            inputMessage.text = "";
        }
    }
}
