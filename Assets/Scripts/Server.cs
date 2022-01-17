using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 10;

    private int _port = 5805;

    private int _hostID;
    private int _reliableChannel;

    private bool _isStarted;
    private byte _error;

    Dictionary<int, string> _connectionIDs = new Dictionary<int, string>();

    public void StartServer()
    {        
        if (_isStarted)
            return;
        
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        _reliableChannel = cc.AddChannel(QosType.Reliable);
        
        HostTopology topology = new HostTopology(cc, MAX_CONNECTION);        
        _hostID = NetworkTransport.AddHost(topology, _port);
        
        _isStarted = true;
        Debug.Log($"Server {_hostID} has started");
    }

    void Update()
    {
        if (!_isStarted)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        string playerName;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);

        while (recData != NetworkEventType.Nothing)
        {
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    break;

                case NetworkEventType.ConnectEvent:
                    _connectionIDs.Add(connectionId, "");

                    SendMessageToAll($"Player {connectionId} has connected.");
                    Debug.Log($"Player {connectionId} has connected.");
                    break;

                case NetworkEventType.DataEvent:
                    string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);

                    if (_connectionIDs.TryGetValue(connectionId, out playerName))
                    {
                        if (playerName == "")
                        {
                            _connectionIDs[connectionId] = message == "" ? connectionId.ToString() : message;
                        }
                        else
                        {
                            SendMessageToAll($"{playerName}: {message}");
                            Debug.Log($"{playerName}: {message}");
                        }
                    }
                    
                    break;

                case NetworkEventType.DisconnectEvent:
                    if (_connectionIDs.TryGetValue(connectionId, out playerName))
                    {
                        _connectionIDs.Remove(connectionId);

                        SendMessageToAll($"{playerName} has disconnected.");
                        Debug.Log($"{playerName} has disconnected.");
                    }

                    break;

                case NetworkEventType.BroadcastEvent:
                    break;

            }

            recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
        }
    }

    public void ShutDownServer()
    {
        if (!_isStarted)
            return;

        NetworkTransport.RemoveHost(_hostID);
        NetworkTransport.Shutdown();
        _isStarted = false;
        Debug.Log($"Server {_hostID} has stopped");
    }

    public void SendMessage(string message, int connectionID)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(_hostID, connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);
        if ((NetworkError)_error != NetworkError.Ok)
            Debug.Log((NetworkError)_error);
    }

    public void SendMessageToAll(string message)
    {
        foreach (var connectionID in _connectionIDs.Keys)
        {
            SendMessage(message, connectionID);
        }
    }
}

    public interface Message
    {
        short messageId { get; }
    }

    public class MyMessage: Message
    {
        public string data;
        public int number;

        public short messageId => 0;
    }

    public class TheirMessage: Message
    {
        public bool success;
        public int number;

        public short messageId => 2;
    }
