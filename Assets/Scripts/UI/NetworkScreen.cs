using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        int port = System.Convert.ToInt32(portInputField.text);
        Debug.Log("ip: " + addressInputField.text);
        // NetworkManager.Instance.StartClient(ipAddress, port);
        ConnectionManager.Instance.ConnectToServer(ipAddress, port, StartConnection);
        // SwitchToChatScreen();
    }

    void StartConnection(bool connect)
    {
        Debug.Log("OnConnect: " + connect);
        SwitchToChatScreen();
    }

    void OnStartServerBtnClick()
    {
        int port = System.Convert.ToInt32(portInputField.text);
        // NetworkManager.Instance.StartServer(port);
        ConnectionManager.Instance.StartServer(port);
        SwitchToChatScreen();
    }

    void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
