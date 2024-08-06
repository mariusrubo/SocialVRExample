using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Transporting;
using MyBroadcasts;
using System.IO;
using FishNet.Managing.Server;

/* Data describing our own avatar's state are continuously sent to the server which relays them to other clients so that they can update their representation of us accordingly. 
 * by Marius Rubo, 2023
 * */
public class ClientSendOwnInfo : MonoBehaviour
{
    /// <summary>
    /// Reference our own CharacterBehaviorController so that, while we're online and connected to the server, we can send its info along with our network id.
    /// </summary>
    [SerializeField] CharacterBehaviorController MyCharacterBehaviorController;
    bool isOnline;
    int own_id;

    void Start()
    {
        RegisterBroadcasts();
    }

    private void OnDisable()
    {
        UnregisterBroadcasts();
    }

    /// <summary>
    /// The starting point of the whole netcode setup: when the server authenticates our connection, we start the actual communication with it. In particular, 
    /// we first ask it to transmit our existence to other connected clients.
    /// </summary>
    private void ClientManager_OnAuthenticated()
    {
        own_id = InstanceFinder.ClientManager.Connection.ClientId; // id should not change throughout the session

        playerInfoForSpawn msg = new playerInfoForSpawn()
        {
            id = InstanceFinder.ClientManager.Connection.ClientId,
            characterPosition = MyCharacterBehaviorController.CharacterPosition, // we transmit the current position with it already here. May not be needed since
                                                                                 // the server will soon ask us to update our position to a different side of the table.
            characterRotation = MyCharacterBehaviorController.CharacterRotation
        };
        InstanceFinder.ClientManager.Broadcast(msg, Channel.Reliable); // data which have the form of announcements are sent reliably; it is important that they arrive and
        // should they be a victim of packet loss, they must be resent even if that takes some ms. The reliable channel here corresponds somewhat to the TCP standard.
        Debug.Log("Sending spawn request to server");

        isOnline = true;
    }

    private void FixedUpdate()
    {
        if (isOnline)
        {
            SendOwnDataToServer();
        }
    }

    /// <summary>
    /// Keep sending our own data to the server. At this point we do not know what will happen with these data: if there is no other connected client, the server will
    /// not relay it anywhere and just forget about it. If there are other clients, they will use these data to update their represantations (remote players) of us. 
    /// Sending of data may be performed in FixedUpdate to better ensure consistent framerates (or higher framerates compared with the screen update rates) but may also
    /// be performed in Update.
    /// </summary>
    void SendOwnDataToServer()
    {
        // Continuously send own position and rotation to server
        currentPlayerState msg = new currentPlayerState
        {
            id = own_id,
            characterPosition = MyCharacterBehaviorController.CharacterPosition,
            characterRotation = MyCharacterBehaviorController.CharacterRotation,
            headIKPosition = MyCharacterBehaviorController.headIKPosition,
            headIKRotation = MyCharacterBehaviorController.headIKRotation,
            lEyeRotation = MyCharacterBehaviorController.lEyeRotation,
            rEyeRotation = MyCharacterBehaviorController.rEyeRotation,
            smiling = MyCharacterBehaviorController.Smiling
        };
        InstanceFinder.ClientManager.Broadcast(msg, Channel.Unreliable); // data are sent "unreliably" (similar to UDP). This means that they will not be resent in case of packet
        // loss. Since data are sent continuously and the resending of a playerstate would be outdated anyway by the time it arrives, there is no need for reliability measures.
    }


    /// <summary>
    /// When we lose connection, simply stop trying to broadcast our own data.
    /// </summary>
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            isOnline = false;
        }
    }

    #region Register and unregister broadcasts
    /// <summary>
    /// Boilerplate code needed to register/unregister all the broadcasts which should be directed into this script by the client netcode.
    /// </summary>
    void RegisterBroadcasts()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.OnAuthenticated += ClientManager_OnAuthenticated;
            clientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

        }
        else { Debug.Log("Broadcast could not be registered!"); }
    }

    void UnregisterBroadcasts()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.OnAuthenticated -= ClientManager_OnAuthenticated;
            clientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }
    }
    #endregion
}
