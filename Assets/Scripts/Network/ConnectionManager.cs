using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System;

public class ConnectionManager : MonoBehaviourSingleton<ConnectionManager>
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get { return NetworkManager.Instance.isServer; }
    }

    public enum ConnectionState
    {
        Disconnected,
        RequestingConnection,
        ChallengeComplying,
        Connected
    }

    /*public readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();*/

    /*public Action<byte[], IPEndPoint> OnReceiveEvent;
    private UdpConnection connection;*/
    private System.Action<bool> whenConnected;
    public long clientSalt = 0;
    public long serverSalt = 0;
    public long challengeResult = 0;
    ConnectionState state = ConnectionState.Disconnected;
    private const float RESENDERATE = 0.5f;

    protected override void Initialize()
    {
        base.Initialize();
        clientSalt = serverSalt = -1;
        state = ConnectionState.Disconnected;
        PacketManager.Instance.ReceivingInternalPacket += ReceivingData;
    }

    public void StartServer(int port)
    {
        /*isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);*/
        if (NetworkManager.Instance.StartServer(port))
        {
            state = ConnectionState.Connected;
        }
    }

    /*public void StartClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);

        NetworkManager.Instance.AddClient(new IPEndPoint(ip, port));
    }*/ // Reemplazarlo con otra cosa

    public void ConnectToServer(IPAddress iPAddress, int port, System.Action<bool> connectionCallback)
    {
        if (!NetworkManager.Instance.StartClient(iPAddress, port))
        {
            if (connectionCallback != null)
            {
                connectionCallback(false);
            }
            return;
        }
        if (connectionCallback != null)
        {
            whenConnected += connectionCallback;
        }
        clientSalt = NetworkManager.Instance.rnd.Next(999999999);
        state = ConnectionState.RequestingConnection;
        SendConnectionRequest();
    }

    private void SendToServer<T>(NetworkPacket<T> packet)
    {
        PacketManager.Instance.SendPacketToServer(packet);
    }

    private void SendToClient<T>(NetworkPacket<T> packet, IPEndPoint ipEndPoint)
    {
        PacketManager.Instance.SendPacketToClient(packet, ipEndPoint);
    }

    private void SendConnectionRequest()
    {
        ConnectionRequestPacket requestPacket = new ConnectionRequestPacket();
        requestPacket.payload.clientSalt = clientSalt;
        SendToServer(requestPacket);
    }

    private void SendChallengeRequest(ConnectionRequestInformation data, int cId, long cSalt, long sSalt, IPEndPoint iPEnd)
    {
        ChallengeRequestPacket requestPacket = new ChallengeRequestPacket();
        requestPacket.payload.clientId = cId;
        requestPacket.payload.clientSalt = cSalt;
        requestPacket.payload.serverSalt = sSalt;
        SendToClient(requestPacket, iPEnd);
    }

    private void SendChallengeResponse(long cSalt, long sSalt)
    {
        ChallengeResponsePacket responsePacket = new ChallengeResponsePacket();
        responsePacket.payload.result = cSalt ^ sSalt;
        SendToServer(responsePacket);
    }

    private void SendConnected(int cId, IPEndPoint iPEnd)
    {
        ConnectionPacket packet = new ConnectionPacket();
        packet.payload.clientId = cId;
        SendToClient(packet, iPEnd);
    }

    public void ReceivingData(PacketType packet, Stream stream, IPEndPoint iPEnd)
    {
        switch (packet)
        {
            case PacketType.ConnectionRequest:
                ReceivingConnectionRequest(stream, iPEnd);
                break;
            case PacketType.ChallengeRequest:
                ReceivingChallengeRequest(stream, iPEnd);
                break;
            case PacketType.ChallengeResponse:
                ReceivingChallengeResponse(stream, iPEnd);
                break;
            case PacketType.Connected:
                ReceivingConnection(stream, iPEnd);
                break;
        }
    }

    public void ReceivingConnection(Stream stream, IPEndPoint iPEnd)
    {
        if (!NetworkManager.Instance.isServer && state != ConnectionState.Connected)
        {
            ConnectionPacket packet = new ConnectionPacket();
            packet.Deserialize(stream);
            if (packet.payload.clientId == NetworkManager.Instance.clientId)
            {
                state = ConnectionState.Connected;
                if (whenConnected != null)
                {
                    whenConnected(true);
                    whenConnected = null;
                }
            }
        }
    }

    public void ReceivingConnectionRequest(Stream stream, IPEndPoint iPEnd)
    {
        if (NetworkManager.Instance.isServer)
        {
            ConnectionRequestPacket requestPacket = new ConnectionRequestPacket();
            requestPacket.Deserialize(stream);
            long cSalt = requestPacket.payload.clientSalt;
            long sSalt = -1;
            int cId = -1;
            if (NetworkManager.Instance.ipToId.ContainsKey(iPEnd))
            {
                cId = NetworkManager.Instance.ipToId[iPEnd];
                Client client = NetworkManager.Instance.clients[cId];
                sSalt = NetworkManager.Instance.clients[cId]._serverSalt;
            }
            else
            {
                sSalt = NetworkManager.Instance.rnd.Next(999999999);
                cId = NetworkManager.Instance.AddClient(cSalt, sSalt, iPEnd);
            }
            SendChallengeRequest(requestPacket.payload, cId, cSalt, sSalt, iPEnd);
        }
    }

    public void ReceivingChallengeRequest(Stream stream, IPEndPoint iPEnd)
    {
        if (!NetworkManager.Instance.isServer)
        {
            state = ConnectionState.ChallengeComplying;
            ChallengeRequestPacket challengePacket = new ChallengeRequestPacket();
            challengePacket.Deserialize(stream);
            NetworkManager.Instance.clientId = challengePacket.payload.clientId;
            serverSalt = challengePacket.payload.serverSalt;
            SendChallengeResponse(challengePacket.payload.clientSalt, challengePacket.payload.serverSalt);
        }
    }

    public void ReceivingChallengeResponse(Stream stream, IPEndPoint iPEnd)
    {
        if (NetworkManager.Instance.isServer)
        {
            ChallengeResponsePacket responsePacket = new ChallengeResponsePacket();
            responsePacket.Deserialize(stream);
            if (NetworkManager.Instance.ipToId.ContainsKey(iPEnd))
            {
                Client client = NetworkManager.Instance.clients[NetworkManager.Instance.ipToId[iPEnd]];
                challengeResult = client._clientSalt ^ client._serverSalt;
                if (responsePacket.payload.result == challengeResult)
                {
                    client.state = Client.ClientConnection.Connected;
                    SendConnected(client._id, iPEnd);
                }
            }
        }
    }

    float lastTimeMessaged;
    private bool PleaseResendRequest()
    {
        return state != ConnectionState.Connected && state != ConnectionState.Disconnected && (Time.realtimeSinceStartup - lastTimeMessaged >= RESENDERATE);
    }
    private void Update()
    {
        if (!NetworkManager.Instance.isServer)
        {
            if (PleaseResendRequest())
            {
                lastTimeMessaged = Time.realtimeSinceStartup;
                switch (state)
                {
                    case ConnectionState.RequestingConnection:
                        SendConnectionRequest();
                        break;
                    case ConnectionState.ChallengeComplying:
                        SendChallengeResponse(clientSalt, serverSalt);
                        break;
                }
            }
        }
    }
}
