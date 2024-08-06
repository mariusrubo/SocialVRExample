using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;


/* The client starts an attempt to connect to the server, and automatically tries to reconnect if the connection is lost. This script represents the case where the server's
 * ip address is known to the client. When running all instances of the software (server and at least one client) on the same computer, the connection can be established 
 * by merely using "localhost" as server ip address. When the server and the client are on different computers but are connected in the same local network, the server's ip
 * address must be inserted here. It can be obtained simply by opening the cmd and typing "ipconfig". The IPv4 address should work fine. However, note that automatically
 * assigned ip addresses can change from time to time. The simplest way to ensure a correct connection between the clients and the server throughout a research study is to
 * assign a static ip address for the server computer. 
 * */
public class StartNetworkClient : MonoBehaviour
{
    [Tooltip("IP address of the server's computer.")] // to get private ip: open cmd and type ipconfig
    [SerializeField]
    private string serverIP = "localhost";


    /// <summary>
    /// The "connection has stopped" event must be subscribed to. 
    /// </summary>
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;

    void Start()
    {
        Connect();

        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }
    }

    private void OnDisable()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }
    }

    /// <summary>
    /// If the client loses connection, it tries to reconnect.
    /// </summary>
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Client has stopped, trying to reconnect"); // called when server is not found or disconnected (not when client ends itself)
            Connect(); // connect again
        }
    }

    /// <summary>
    /// Initiates the connection to the server, which is handled under the hood by Fish-Net. 
    /// </summary>
    void Connect()
    {
        NetworkManager _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager != null)
        {
            if (_clientState != LocalConnectionState.Stopped)
                _networkManager.ClientManager.StopConnection();
            else
            {
                _networkManager.ClientManager.StartConnection(serverIP);
            }
        }
    }

}
