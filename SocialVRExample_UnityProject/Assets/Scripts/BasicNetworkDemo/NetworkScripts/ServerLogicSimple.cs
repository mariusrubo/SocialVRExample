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

/* Server functionality to organize relaying of information between clients. The server has only few responsibilities in this example: (1) relay information of a new client's
 * spawning to existing players, (2) inform new player of the existence of players who have already been connected, (3) tell each new player where to sit on the table, i.e., its
 * reference point, (4) relaying player states between all active players and (5) inform remaining players when a player has disconnected. 
 * 
 * The script is called ServerLogicSimple because this functionality represents the core of a networked VR setup. The server software could therefore be a small and lightweight
 * program which may easily be hosted on a server structure, although in most laboratory use cases it is even easier to run the server along with one of the clients. 
 * Such a relatively simple server software is entirely sufficient to carry out a range of social VR experiments in a laboratory environment, but can be extended in several ways. 
 * For example, a server may (1) not only relay data, but also represent the scene which the clients are seeing, thus creating a spectator view of the social situation, 
 * (2) note down data to disk, which is currently done on the client side or (3) manage the synchronization of additional objects which can be moved in the scene, including an
 * authority management, i.e., organizing who is able to move which object. With increasing complexity the server software will typically be spread across several scripts. 
 * by Marius Rubo, 2023
 * */

public class ServerLogicSimple : MonoBehaviour
{
    /// <summary>
    /// The server keeps a dictionary of all connected players and some basic info of them (where they are). Currently it does not itself track more detailed data (e.g., 
    /// where each character is looking) but merely relays these data among the clients. 
    /// </summary>
    private Dictionary<int, ClientBasicData> Players { get; set; } = new Dictionary<int, ClientBasicData>();

    struct ClientBasicData
    {
        public Vector3 position;
        public Quaternion rotation;
        public int whichSeat;
    }

    /// <summary>
    /// The server tracks which reference points (here available seats on the table) are occupied so that it can assign an available seat to newly connected players, 
    /// or prevent the addition of a new client to the table when no seat is available. 
    /// </summary>
    private List<int> availableSeats = new List<int>() { 0, 1, 2, 3 };


    void Start()
    {
        RegisterBroadcasts();
    }

    private void OnDisable()
    {
        UnregisterBroadcasts();
    }

    /// <summary>
    /// When a client connects, the server has several tasks: (1) add it to its own dictionary, (2) relay spawn info to existing clients, (3) inform new client about what
    /// other clients are already on the table and (4) tell new client where to sit on the table, i.e., its reference point. 
    /// </summary>
    /// <param name="conn">The client's address on the server's table of addresses (handled automatically by Fish-Net). Note that on the server, the sender's address
    /// is often an important piece of information while the clients only receive direct messages from the server, so do not need to considers its address.</param>
    /// <param name="msg">The actual message containing the client's id and some basic position data.</param>
    /// <param name="channel">Whether the message came in on the reliable or unreliable channel. Spawn broadcasts are always sent reliably.</param>
    public void OnSpawnBroadcast(NetworkConnection conn, playerInfoForSpawn msg, Channel channel)
    {
        if (!Players.ContainsKey(msg.id) && availableSeats.Count > 0) // double-check that player id has not connected before (unless it has disconnected in between), and only continue if there is a seat
        {
            Debug.Log("Server: Spawn broadcast received");

            int seatForThisID = availableSeats[0];
            availableSeats.Remove(seatForThisID);

            // STEP 1: keep track of players right here on server (at least pivot position, rotation, and seat)
            ClientBasicData clientBasicData = new ClientBasicData();
            clientBasicData.position = msg.characterPosition;
            clientBasicData.rotation = msg.characterRotation;
            clientBasicData.whichSeat = seatForThisID;
            Players.Add(msg.id, clientBasicData);

            // STEP 2: relay info of new client to all existing clients
            foreach (var client in InstanceFinder.ServerManager.Clients)
            {
                InstanceFinder.ServerManager.Broadcast(client.Value, msg, true, Channel.Reliable);
            }

            // STEP 3: send info of all existing clients to new client, except self
            foreach (KeyValuePair<int, ClientBasicData> entry in Players)
            {
                if (entry.Key != msg.id) // except self. important: otherwise things are spawned twice
                {
                    playerInfoForSpawn infoExisting = new playerInfoForSpawn()
                    {
                        id = entry.Key,
                        characterPosition = entry.Value.position,
                        characterRotation = entry.Value.rotation
                    };

                    InstanceFinder.ServerManager.Broadcast(conn, infoExisting, true, Channel.Reliable);
                }
            }

            // STEP 4: tell client its assigned reference point (which seat on the table to sit on)
            updateReferencePoint updateReferencePoint = new updateReferencePoint()
            {
                referencePointID = seatForThisID
            };
            InstanceFinder.ServerManager.Broadcast(conn, updateReferencePoint, true, Channel.Reliable);
        }
    }

    /// <summary>
    /// When a client disconnects, remove from own dictionary and tell remaining clients to remove it as well. 
    /// </summary>
    private void Transport_OnRemoteConnectionState(RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            int id = args.ConnectionId;

            if (Players.TryGetValue(id, out ClientBasicData player))
            {
                availableSeats.Add(player.whichSeat); // seat becomes available again
            }
            Players.Remove(id);

            playerInfoForDespawn info = new playerInfoForDespawn() { id = id };
            foreach (var client in InstanceFinder.ServerManager.Clients)
            {
                InstanceFinder.ServerManager.Broadcast(client.Value, info, true, Channel.Reliable);
            }
        }
    }


    /// <summary>
    /// When a client provides an update on its own state, just relay that information to all other clients.
    /// </summary>
    private void OnPlayerStateUpdateReceived(NetworkConnection conn, currentPlayerState msg, Channel channel)
    {
        // track position and rotation here on server too (although server never displays anything)
        if (Players.TryGetValue(msg.id, out ClientBasicData player))
        {
            player.position = msg.characterPosition;
            player.rotation = msg.characterRotation;
        }

        // relay to other players
        foreach (var client in InstanceFinder.ServerManager.Clients)
        {
            if (client.Value != conn && client.Value.IsAuthenticated) InstanceFinder.ServerManager.Broadcast(client.Value, msg, true, Channel.Unreliable); // relay to all others but not self

        }
    }

    #region Register and unregister broadcasts
    /// <summary>
    /// Boilerplate code needed to register/unregister all the broadcasts which should be directed into this script by the client netcode.
    /// </summary>
    void RegisterBroadcasts()
    {
        var serverManager = InstanceFinder.ServerManager;
        if (serverManager != null)
        {
            serverManager.RegisterBroadcast<playerInfoForSpawn>(OnSpawnBroadcast);
            serverManager.RegisterBroadcast<currentPlayerState>(OnPlayerStateUpdateReceived);
            InstanceFinder.NetworkManager.TransportManager.Transport.OnRemoteConnectionState += Transport_OnRemoteConnectionState;
        }
        else { Debug.Log("Broadcast could not be registered!"); }
    }

    void UnregisterBroadcasts()
    {
        var serverManager = InstanceFinder.ServerManager;
        if (serverManager != null)
        {
            serverManager.UnregisterBroadcast<playerInfoForSpawn>(OnSpawnBroadcast);
            serverManager.UnregisterBroadcast<currentPlayerState>(OnPlayerStateUpdateReceived);
            InstanceFinder.NetworkManager.TransportManager.Transport.OnRemoteConnectionState -= Transport_OnRemoteConnectionState;
        }
    }
    #endregion

}
