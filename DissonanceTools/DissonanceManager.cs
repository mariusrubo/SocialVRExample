using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using Dissonance;
using FishNet.Transporting;
using FishNet.Connection;
using System;
using MyBroadcasts;

/* Here we keep references of each player's representation in Dissonance voice chat (VoicePlayerState) in order to be able to further process these data (e.g., write them 
 * to disk) elsewhere. Importantly, Dissonance is started manually here which allows to link each player's representation with Fish-Nets own id; we would otherwise create
 * the counter-intuitive situation where behavioral and voice data are correctly displayed on each character in the scene, but the logfiles do not allow to relate data
 * streams to each other (e.g., to tell if a specific id represents the self or another person). 
 * 
 * To implement this script follow these steps: (1) download Dissonance Voice Chat (https://placeholder-software.co.uk/) and its Fish-Net integration 
 * (https://github.com/LambdaTheDev/DissonanceVoiceForFishNet), (2) add all objects to the scene as described in the respective manuals (3) add this script to the object
 * with the DissonanceComms on it, (4) disable DissonanceComms, (5) in VoicePlayback.cs, manually delete "UpdatePositionalPlayback();" (which otherwise sets spatial blend to 0
 * in this setup, corrupting spatial sound). 
 * 
 * While other scripts in this project are organized along whether they execute logic on the client or the server, this script mostly describes client logic but in one instance
 * describes server logic as well. Nothing needs to be changed if you use this script in a host or a client, but see "ServerManager_OnServerConnectionState" if you plan to build
 * a pure server (with no client running in the same program). 
 * by Marius Rubo, 2023
 * */
public class DissonanceManager : MonoBehaviour
{
    /// <summary>
    /// A dictionary of player's VoicePlayerStates which are being updated by Dissonance.
    /// </summary>
    Dictionary<string, VoicePlayerState> players = new Dictionary<string, VoicePlayerState>();
    public Dictionary<string, VoicePlayerState> GetPlayersVoiceData()
    {
        return players;
    }

    /// <summary>
    /// We additionally move each player's audio component in the scene to realize spatial audio, placing each player's audio component to where its head is.
    /// This way, a person speaking to you from the left should also be heard to be speaking from that direction). Dissonance has functions to implement this 
    /// automatically, but since we chose to interfer with Dissonance's automatic setup, here we transparently implement spatial tracking ourselves. 
    /// </summary>
    Dictionary<int, Transform> playerObjects = new Dictionary<int, Transform>();

    bool isDissonanceOnline = false;
    int own_id;

    private void Start()
    {
        RegisterBroadcasts();
    }

    private void OnDisable()
    {
        UnregisterBroadcasts();
    }

    #region Fishnet starts Dissonance

    /// <summary>
    /// Since Dissonance was disabled in the Editor, we enable it manually after Fish-Net, in its role as client, has connected to the server. This allows to
    /// hand over Fish-Net's ids to Dissonance. Wait a second to do this to make sure that ids are available at this time.
    /// </summary>
    private void ClientManager_OnAuthenticated()
    {
        if (!isDissonanceOnline) StartCoroutine(StartupDissonanceAfterWait(1));
    }
    IEnumerator StartupDissonanceAfterWait(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);

        // a key function here: when Fishnet has started, also set Dissonance id manually to the same id and start Dissonance
        own_id = InstanceFinder.ClientManager.Connection.ClientId;
        DissonanceComms _dissComms = FindObjectOfType<DissonanceComms>();
        _dissComms.LocalPlayerName = own_id.ToString();

        _dissComms.enabled = true;
        _dissComms.IsMuted = false;
        _dissComms.IsDeafened = false;

        // subscribe to events which will allow Dissonance to inform us of other players' joining or leaving
        _dissComms.OnPlayerJoinedSession += ReferenceNewPlayer;
        _dissComms.OnPlayerLeftSession += RemovePlayer;

        // add self to the dictionary of players (OnPlayerJoinedSession only tracks others)
        StartCoroutine(AddOwnPlayerAfterWait(1)); // again just wait a second since VoicePlayerState may need a moment to load
    }

    /// <summary>
    /// Add own player to the dictionaries.
    /// </summary>
    IEnumerator AddOwnPlayerAfterWait(float waitingtime)
    {
        yield return new WaitForSeconds(waitingtime);
        DissonanceComms _dissComms = FindObjectOfType<DissonanceComms>();
        VoicePlayerState ownplayer = _dissComms.FindPlayer(_dissComms.LocalPlayerName);
        if (ownplayer == null) Debug.LogWarning("Problem with own VoicePlayerState");
        ReferenceNewPlayer(ownplayer);
        isDissonanceOnline = true;
    }

    // as server: directly go online (for server I could just manually set _dissComms to enabled in Editor, but I'd like as much as possible identical between server and client)
    // actually since we only use a server in conjunction with a client (i.e., as host), the Fishnet server never needs to enable Disscomms
    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        // if this program is intended so only run as server (not as host, combining server and client), use the below code to start Dissonance manually
        // otherwise leave starting to the client, which will start voice a second later to ensure ids can be set correctly
        /*
        if (obj.ConnectionState == LocalConnectionState.Started)
        {
            FishNet.Managing.NetworkManager _networkManager = FindObjectOfType<FishNet.Managing.NetworkManager>();

            if (_networkManager.ClientManager == null && obj.ConnectionState == LocalConnectionState.Started)
            {
                DissonanceComms _dissComms = FindObjectOfType<DissonanceComms>();
                _dissComms.enabled = true;
            }
        }
        */
    }
    #endregion

    #region Dissonance tracks players

    /// <summary>
    /// Function to add a VoicePlayerState (either one's own or from a remote player) to the dictionary of VoicePlayerStates, and to also reference the transform
    /// which holds a player's audio source.
    /// </summary>
    /// <param name="player">The VoicePlayerState object to be added.</param>
    private void ReferenceNewPlayer(VoicePlayerState player)
    {
        players.Add(player.Name, player);
        // Debug.Log("Adding speaker " + player.Name);

        /*
        // for only tracking when someone is speaking, one can also use these events.
        player.OnStartedSpeaking += player =>
        {
            //Debug.Log("Player " + player.Name + " Started Speaking");
        };

        player.OnStoppedSpeaking += player =>
        {
            //Debug.Log("Player " + player.Name + " Stopped Speaking");
        };
        */


        // for positional tracking, additionally track gameObjects with AudioSource on
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name.Contains(player.Name)) // Dissonance will have added a child object with the remote player's audio source
            {
                Debug.Log("Adding transform of " + player.Name);
                playerObjects.Add(int.Parse(player.Name), children[i]);

                AudioSource audioSource = children[i].gameObject.GetComponent<AudioSource>();
                audioSource.spatialBlend = 1;
                audioSource.minDistance = 0.5f;
                audioSource.maxDistance = 10f;

                // Here we can also add all sorts of filtering to each voice
                /*
                AudioHighPassFilter audioHigh = children[i].gameObject.AddComponent<AudioHighPassFilter>();
                audioHigh.cutoffFrequency = 500;
                audioHigh.highpassResonanceQ = 4;

                AudioLowPassFilter audioLow = children[i].gameObject.AddComponent<AudioLowPassFilter>();
                audioLow.cutoffFrequency = 2500;
                audioLow.lowpassResonanceQ = 4;
                */
            }
        }

    }

    // Receive position data from other players, move transform with AudioSource on it accordingly

    /// <summary>
    /// Update position of remote player's audio source when new position of their heads arrives.
    /// </summary>
    /// <param name="msg">The currentPlayerState message, which is used elsewhere to update the character itself.</param>
    /// <param name="channel">Whether the message was received as reliable or unreliable message; positional data is sent unreliably.</param>
    private void OnPlayerStateUpdate(currentPlayerState msg, Channel channel)
    {
        if (playerObjects.TryGetValue(msg.id, out Transform _tr))
        {
            _tr.position = msg.headIKPosition;
        }
    }
    #endregion


    /// <summary>
    /// When a player leaves, which is detected and communicated by Dissonance, we also remove it from our own dictionaries here.
    /// </summary>
    /// <param name="player">The VoicePlayerState object to be removed.</param>
    private void RemovePlayer(VoicePlayerState player)
    {
        if (player.Name == own_id.ToString()) isDissonanceOnline = false;

        players.Remove(player.Name);
        playerObjects.Remove(int.Parse(player.Name));

        //player.OnStartedSpeaking -= player => {};
        //player.OnStoppedSpeaking -= player => {};
    }

    /// <summary>
    /// If we lose connection to server, also reset our own dictionaries.
    /// </summary>
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            players = new Dictionary<string, VoicePlayerState>();
            playerObjects = new Dictionary<int, Transform>();
            isDissonanceOnline = false;
        }
    }

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
            clientManager.OnAuthenticated += ClientManager_OnAuthenticated;
            //clientManager.OnRemoteConnectionState += this.OnRemoteConnectionState; // registration of others' connection in Fish-Net is not needed here; only Dissonance's registration
            clientManager.RegisterBroadcast<currentPlayerState>(OnPlayerStateUpdate);
            //clientManager.RegisterBroadcast<playerInfoForDespawn>(OnPlayerDisconnected); // not be needed because Dissonance itself will notice when a player leaves
            clientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }

        var serverManager = InstanceFinder.ServerManager;
        if (serverManager != null) serverManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
    }

    void UnregisterBroadcasts()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            //clientManager.OnRemoteConnectionState -= this.OnRemoteConnectionState;
            clientManager.OnAuthenticated -= ClientManager_OnAuthenticated;
            clientManager.UnregisterBroadcast<currentPlayerState>(OnPlayerStateUpdate);
            //clientManager.UnregisterBroadcast<playerInfoForDespawn>(OnPlayerDisconnected);
            clientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }

        var serverManager = InstanceFinder.ServerManager;
        if (serverManager != null) serverManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;

        // the following aren't technically broadcasts but Dissonance actions
        DissonanceComms _dissComms = FindObjectOfType<DissonanceComms>();
        if (_dissComms != null)
        {
            _dissComms.OnPlayerJoinedSession -= ReferenceNewPlayer;
            _dissComms.OnPlayerLeftSession -= RemovePlayer;
        }
    }
    #endregion

}
