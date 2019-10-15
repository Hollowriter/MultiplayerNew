using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public struct Client
{
    public enum ClientConnection
    {
        StartingToConnect,
        Connected
    }

    public float _timeStamp;
    public int _id;
    public long _clientSalt;
    public long _serverSalt;
    public IPEndPoint _ipEndPoint;
    public ClientConnection state;

    public Client(IPEndPoint ipEndPoint, int id, long clientSalt, long serverSalt, float timeStamp)
    {
        this._timeStamp = timeStamp;
        this._id = id;
        this._clientSalt = clientSalt;
        this._serverSalt = serverSalt;
        this._ipEndPoint = ipEndPoint;
        this.state = ClientConnection.StartingToConnect;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
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
        get; private set;
    }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    public readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    public int clientId = 0; // This id should be generated during first handshake
    public System.Random rnd = new System.Random();

    public bool StartServer(int port) // esto
    {
        try
        {
            isServer = true;
            this.port = port;
            connection = new UdpConnection(port, this);
        }
        catch (Exception e)
        {
            Debug.Log("Server error " + e);
            return false;
        }
        return true;
    }

    public bool StartClient(IPAddress ip, /*long cSalt, long sSalt,*/ int port) // esto
    {
        try
        {
            isServer = false;
            this.port = port;
            this.ipAddress = ip;
            connection = new UdpConnection(ip, port, this);
        }
        catch (Exception e)
        {
            Debug.Log("Client error " + e);
            return false;
        }
        return true;
        // AddClient(cSalt, sSalt, new IPEndPoint(ip, port));
    }

    public int AddClient(long cSalt, long sSalt, IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Debug.Log("Adding client: " + ip.Address);
            do
            {
                clientId = rnd.Next(99999);
            } while (clients.ContainsKey(clientId));
            ipToId[ip] = clientId;
            clients.Add(clientId, new Client(ip, clientId, cSalt, sSalt, Time.realtimeSinceStartup));
            return clientId;
            // clientId++;
        }
        return -1;
    }

    public void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip) // esto (connection manager tambien)
    {
        // AddClient(ip); // Connection manager

        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
    }

    public void SendToServer(byte[] data) // esto
    {
        connection.Send(data);
    }

    public void SendToClient(byte[] data, IPEndPoint iPEnd)
    {
        connection.Send(data, iPEnd);
    }

    /*public void Broadcast(byte[] data) // packet manager?
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }*/

    void Update() // y esto obviamente
    {
        // We flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
}
