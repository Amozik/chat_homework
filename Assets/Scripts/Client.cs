using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messages;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    public event Action<string> OnMessageReceive;
    public event Action<IMessage> OnTypedMessageReceive;

    private const int MAX_CONNECTION = 10;

    private int _port = 0;
    private int _serverPort = 5805;

    private int _hostID;

    private int _reliableChannel;
    private int _connectionID;

    private bool _isConnected = false;
    private string _playerName;
    private byte _error;

    public void Connect(string playerName)
    {
        if (_isConnected)
            return;

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();

        _reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
        
        _hostID = NetworkTransport.AddHost(topology, _port);        
        _connectionID = NetworkTransport.Connect(_hostID, "127.0.0.1", _serverPort, 0, out _error);

        if ((NetworkError) _error == NetworkError.Ok)
        {
            _isConnected = true;
            _playerName = playerName == "" ? _connectionID.ToString() : playerName;
        }
        else
            Debug.Log((NetworkError)_error);
    }

    public void Disconnect()
    {
        if (!_isConnected) return;

        NetworkTransport.Disconnect(_hostID, _connectionID, out _error);
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
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);

        while (recData != NetworkEventType.Nothing)
        {
            switch (recData)
            {
                case NetworkEventType.ConnectEvent:
                    SendMessage(_playerName);
                    OnMessageReceive?.Invoke($"You have been connected to server.");
                    Debug.Log($"You have been connected to server.");
                    break;

                case NetworkEventType.DataEvent:
                    // if (TryDeserializeMessage(recBuffer, dataSize, out var typedMessage))
                    // {
                    //     OnTypedMessageReceive?.Invoke(typedMessage);
                    //     break;
                    // }
                    
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

            recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out _error);
        }
    }

    private bool TryDeserializeMessage(byte[] buffer, int size, out IMessage message)
    {
        var messageIdBytes = new byte[2];
        var messageBytes = new byte[buffer.Length - 2];
        Array.Copy(buffer, 0, messageIdBytes, 0, messageIdBytes.Length);
        Array.Copy(messageBytes, 2, messageBytes, 0, messageIdBytes.Length);
        short messageId = BitConverter.ToInt16(messageIdBytes, 0);
        var messageString = Encoding.Unicode.GetString(messageBytes, 0, size - sizeof(short));
        
        switch (messageId)
        {
            case 2: 
                message = JsonUtility.FromJson<ResponseMessage>(messageString);
                return true;
            case 3:
                message = JsonUtility.FromJson<PlayerMessage>(messageString);
                return true;
            default:
                message = null;
                return false;
        }
    }

    public void SendMessage(string message)
    {
        var buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(_hostID, _connectionID, _reliableChannel, buffer, message.Length * sizeof(char), out _error);
        if ((NetworkError)_error != NetworkError.Ok) 
            Debug.Log((NetworkError)_error);
    }
}
