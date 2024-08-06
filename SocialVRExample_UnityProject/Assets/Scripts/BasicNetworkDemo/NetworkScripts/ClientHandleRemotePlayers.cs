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

/* Data regarding other networked users, which are continuously relayed on the server, are being processed here. A new remote player (the local representation of others)
 * is instantiated when the server announces its connection. The instantiated remote player consists of the actual character model along with an IK setup and scripts which 
 * allow to control character behavior in the same fashion as the character representing the self, in particular a CharacterBehaviorController with which this script directly
 * communicates. A dictionary is kept to store what remote players have been  instantiated and to what network id they are assigned. This dictionary allows to move all 
 * remote players continuously as updated data is being relayed here from the server. Remote players are destroyed when the server announces their disconnecting. 
 * by Marius Rubo, 2023
 * */

public class ClientHandleRemotePlayers : MonoBehaviour
{
    /// <summary>
    /// A prefab used for each remote player. Note that if the same prefab is used for all players, everyone will have the same appearance. Differing appearances can be created
    /// by using different prefabs or by directly transmitting the appearance of other characters. Importantly, the prefab must come with all required components, including
    /// an IK setup and components to control its behavior. 
    /// </summary>
    [SerializeField] GameObject PlayerPrefab; // a prefab of the other players as they will be spawned here

    /// <summary>
    /// A dictionary referencing each remeote player's CharacterBehaviorController along with their assigned network id. 
    /// </summary>
    private Dictionary<int, CharacterBehaviorController> OtherPlayers { get; set; } = new Dictionary<int, CharacterBehaviorController>(); // references of the agents themselves (needed by ReceiveCharacter.cs)

    public Dictionary<int, CharacterBehaviorController> GetOtherPlayers()
    {
        return OtherPlayers;
    }

    private void Start()
    {
        RegisterBroadcasts();
    }
    private void OnDisable()
    {
        UnregisterBroadcasts();
    }

    #region Sending and Receiving of character position and rotation 


    /// <summary>
    /// Character state updates are sent and received in custom-made forms ("broadcasts") which here contain data for character and head position and rotation
    /// along with eye rotations and smiling. This may be extended to transfer other data (e.g., hand and finger movements, blinking, frowning). Note that 
    /// character states need not necessarily be sent in a single broadcast. Instead, for optimization purposes, one may sent gaze data with a high update rate (e.g., 100Hz) 
    /// and facial expression with a lower update rate (e.g., 20Hz) so save bandwidth. 
    /// </summary>
    /// <param name="msg">The message in the form of a currentPlayerState which contains the relevant data</param>
    /// <param name="channel">Info on whether this message was sent using the Reliable (but possibly slower) or Unreliable (faster) channel. 
    /// Here we don't do anything with this information but the function nonetheless requires this parameter.</param>
    private void OnPlayerStateUpdate(currentPlayerState msg, Channel channel)
    {
        if (OtherPlayers.TryGetValue(msg.id, out CharacterBehaviorController characterBehaviorController))
        {
            characterBehaviorController.CharacterPosition = msg.characterPosition;
            characterBehaviorController.CharacterRotation = msg.characterRotation;
            characterBehaviorController.headIKPosition = msg.headIKPosition;
            characterBehaviorController.headIKRotation = msg.headIKRotation;
            characterBehaviorController.lEyeRotation = msg.lEyeRotation;
            characterBehaviorController.rEyeRotation = msg.rEyeRotation;
            characterBehaviorController.Smiling = msg.smiling;
        }
    }
    #endregion

    #region Spawning and Despawning of others

    // The playerInfoForSpawn broadcast sent either by us or another client in ClientManager_OnAuthenticated is returned from the server

    /// <summary>
    /// When a new player connects to the server, the server notifies us with a playerInfoForSpawn message. We instantiate a new player and update the
    /// OtherPlayers dictionary accordingly. 
    /// </summary>
    /// <param name="msg">The message in the form of a playerInfoForSpawn</param>
    /// <param name="channel">Info on whether this message was sent reliably or unreliably.</param>
    public void OnSpawnBroadcast(playerInfoForSpawn msg, Channel channel)
    {
        if (msg.id == InstanceFinder.ClientManager.Connection.ClientId)
        {
            Debug.Log("Server sending back own spawn request with id " + msg.id);
        }
        else
        {
            if (!OtherPlayers.ContainsKey(msg.id)) // just double-check that other was not already set (in rare cases, a message could be received twice)
            {
                GameObject go = Instantiate(PlayerPrefab);
                go.name = "Player" + msg.id;
                go.transform.position = msg.characterPosition;
                go.transform.rotation = msg.characterRotation;
                OtherPlayers.Add(msg.id, go.GetComponent<CharacterBehaviorController>());

            }
        }
    }

    /// <summary>
    /// When we lose connection to the server ourselves, all remote players are destroyed in the scene. 
    /// </summary>
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            DestroyAllOtherPlayers();
        }
    }
    void DestroyAllOtherPlayers()
    {
        foreach (KeyValuePair<int, CharacterBehaviorController> entry in OtherPlayers)
        {
            StartCoroutine(DestroyAfterWait(entry.Value.gameObject)); // destroy all the players
        }
        OtherPlayers = new Dictionary<int, CharacterBehaviorController>(); // and also erase the dictionary holding them
    }

    /// <summary>
    /// When another client has disconnected, the server notifies us. We destroy the respective remote player and remove its entry from the dictionary. 
    /// </summary>
    private void OnRemotePlayerDisconnected(playerInfoForDespawn msg, Channel channel)
    {
        if (OtherPlayers.ContainsKey(msg.id))
        {
            GameObject clientToRemove = OtherPlayers[msg.id].gameObject;
            StartCoroutine(DestroyAfterWait(clientToRemove));
            OtherPlayers.Remove(msg.id);
        }
    }

    IEnumerator DestroyAfterWait(GameObject go)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(go);
    }
    #endregion

    #region Register and unregister broadcasts
    /// <summary>
    /// All the broadcasts which should be directed into this script by the client netcode need to be registered (and typically deregistered when disconnecting).
    /// This may be seen as boilerplate code; there are not active code design decisions to make here. 
    /// </summary>
    void RegisterBroadcasts()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.RegisterBroadcast<playerInfoForSpawn>(OnSpawnBroadcast);
            clientManager.RegisterBroadcast<playerInfoForDespawn>(OnRemotePlayerDisconnected);
            clientManager.RegisterBroadcast<currentPlayerState>(OnPlayerStateUpdate);
            clientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

        }
        else { Debug.Log("Broadcast could not be registered!"); }
    }

    void UnregisterBroadcasts()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.UnregisterBroadcast<playerInfoForSpawn>(OnSpawnBroadcast);
            clientManager.UnregisterBroadcast<playerInfoForDespawn>(OnRemotePlayerDisconnected);
            clientManager.UnregisterBroadcast<currentPlayerState>(OnPlayerStateUpdate);
            clientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }
    }
    #endregion

}