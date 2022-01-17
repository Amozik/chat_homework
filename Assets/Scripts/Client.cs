using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    public event Action<string> OnMessageReceive;

    private const int MAX_CONNECTION = 10;

    private int _port = 0;
    private int _serverPort = 5805;

    private int _hostID;

    private int _reliableChannel;
    private int _connectionID;

    private bool _isConnected = false;
    private byte error;

    public void Connect()
    {
        if (_isConnected)
            return;

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();

        _reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
        
        _hostID = NetworkTransport.AddHost(topology, _port);        
        _connectionID = NetworkTransport.Connect(_hostID, "127.0.0.1", _serverPort, 0, out error);

        if ((NetworkError)error == NetworkError.Ok)
            _isConnected = true;
        else
            Debug.Log((NetworkError)error);
    }

    public void Disconnect()
    {
        if (!_isConnected) return;

        NetworkTransport.Disconnect(_hostID, _connectionID, out error);
        _isConnected = false;
    }
        
    private void Update()
    {
        if (!_isConnected)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        var recBuffer = new byte[1024];
        var bufferSize = 1024;
        int dataSize;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

        while (recData != NetworkEventType.Nothing)
        {
            switch (recData)
            {
                case NetworkEventType.ConnectEvent:
                    OnMessageReceive?.Invoke($"You have been connected to server.");
                    Debug.Log($"You have been connected to server.");
                    break;

                case NetworkEventType.DataEvent:
                    string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    OnMessageReceive?.Invoke(message);
                    Debug.Log(message);
                    break;

                case NetworkEventType.DisconnectEvent:
                    _isConnected = false;
                    OnMessageReceive?.Invoke($"You have been disconnected from server.");
                    Debug.Log($"You have been disconnected from server.");
                    break;

                case NetworkEventType.BroadcastEvent:
                    break;
            }

            recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        }
    }

    public void SendMessage(string message)
    {
        var buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(_hostID, _connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out error);
        if ((NetworkError)error != NetworkError.Ok) 
            Debug.Log((NetworkError)error);
    }
}
