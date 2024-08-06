using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;

/* The server starts its networking activity. This process does not need any parameters; the server merely becomes reachable to other clients who have its ip address.
 * Authentication is granted to any client who manages to reach the server. This scenario is sufficient for a setup where server and client are located on the same local
 * network. If communication is instead carried out over the internet, additional authentication measures should be taken to avoid potential attacks.
 * */
public class StartNetworkServer : MonoBehaviour
{
    private NetworkManager _networkManager;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;

    void Start()
    {
        Connect();
    }

    /// <summary>
    /// Start the networking activity, becoming reachable by clients.
    /// </summary>
    void Connect()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager != null)
        {
            if (_serverState != LocalConnectionState.Stopped)
                _networkManager.ServerManager.StopConnection(true);
            else
            {
                _networkManager.ServerManager.StartConnection();
            }
        }
    }
}
