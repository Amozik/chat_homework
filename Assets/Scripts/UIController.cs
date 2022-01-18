using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Button _buttonStartServer;
    [SerializeField]
    private Button _buttonShutDownServer;
    [SerializeField]
    private Button _buttonConnectClient;
    [SerializeField]
    private Button _buttonDisconnectClient;
    [SerializeField]
    private Button _buttonSendMessage;

    [SerializeField]
    private TMP_InputField _inputField;
    
    [SerializeField]
    private TMP_InputField _nameField;
    
    [SerializeField]
    private TextField _textField;

    [SerializeField]
    private Server _server;
    [SerializeField]
    private Client _client;

    private void Start()
    {
        _buttonStartServer.onClick.AddListener(StartServer);
        _buttonShutDownServer.onClick.AddListener(ShutDownServer);
        _buttonConnectClient.onClick.AddListener(Connect);
        _buttonDisconnectClient.onClick.AddListener(Disconnect);
        _buttonSendMessage.onClick.AddListener(SendMessage);
        _inputField.onEndEdit.AddListener(text => SendMessage());
        _client.OnMessageReceive += ReceiveMessage;
    }

    private void StartServer() =>    
        _server.StartServer();
    
    private void ShutDownServer() =>    
        _server.ShutDownServer();
    
    private void Connect() =>    
        _client.Connect(_nameField.text);    

    private void Disconnect() =>    
        _client.Disconnect();    

    private void SendMessage()
    {
        if (_inputField.text == "")
            return;
        
        _client.SendMessage(_inputField.text);
        _inputField.text = "";
    }

    private void ReceiveMessage(object message) =>    
        _textField.ReceiveMessage(message);
    
    private void OnDestroy()
    {
        _buttonStartServer.onClick.RemoveAllListeners();
        _buttonShutDownServer.onClick.RemoveAllListeners();
        _buttonConnectClient.onClick.RemoveAllListeners();
        _buttonDisconnectClient.onClick.RemoveAllListeners();
        _buttonSendMessage.onClick.RemoveAllListeners();
        _inputField.onEndEdit.RemoveAllListeners();
        _client.OnMessageReceive -= ReceiveMessage;
    }
}
