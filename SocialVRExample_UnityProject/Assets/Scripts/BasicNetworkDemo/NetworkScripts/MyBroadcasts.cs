using UnityEngine;
using FishNet.Broadcast;

/* A collection of basic message types, called broadcasts here, which can be exchanged between clients and the server. Each message type defines what sort of data it can hold,
 * similarly to a form. In this example we only need four types of broadcasts: a message for spawning, a message for despawning, a message to update the reference point, and sent, continuously,
 * a message to update the current player state (position, looking direction etc.).
 * 
 * Only rather basic data types can be included which can be serialized/deserialized by Fish-Net's own serializer/deserializer; for example, we can send floats, Vector3s and
 * strings. More complex structures need to be serialized using external serializers (e.g., Odin) and included as raw bytes in a broadcast to be sent. 
 * by Marius Rubo, 2023
 * */
namespace MyBroadcasts
{
    /// <summary>
    /// A message sent by each client dirctly after connection and authentication by the server, saying "I wand to be part of the simulation, please spawn me here". The
    /// message is relayed to the other clients who will use it to spawn the new player accordingly. If players are to choose between several pre-defined character models
    /// (which are available to the client software on all computers), this message should include a "characterID" so that all clients can spawn the same character model
    /// for a given player. 
    /// </summary>
    public struct playerInfoForSpawn : IBroadcast
    {
        public int id;
        public Vector3 characterPosition;
        public Quaternion characterRotation;
    }

    /// <summary>
    /// A message sent by the server to an individual client, telling it to update its reference point (i.e., sit on a specific side of the table). The server could instead
    /// also message the position and rotation of that reference point, but in this example there is a finite list of reference points (four sides of the same table), so
    /// we can just send its id. 
    /// </summary>
    public struct updateReferencePoint : IBroadcast
    {
        public int referencePointID;
    }

    /// <summary>
    /// The state of a player at each point in time. This may be extended to include different facial expressions, hand and finger movements etc. At some point it may be
    /// more appropriate to use different broadcasts for different aspects of the player state, which may be sent at differing intervals for bandwidth optimization. 
    /// </summary>
    public struct currentPlayerState : IBroadcast
    {
        public int id;
        public Vector3 characterPosition;
        public Quaternion characterRotation;
        public Vector3 headIKPosition;
        public Quaternion headIKRotation;
        public Quaternion lEyeRotation;
        public Quaternion rEyeRotation;
        public float smiling;
    }

    /// <summary>
    /// Simply a message telling others that a client has left and it can be deleted in all other simulations. 
    /// </summary>
    public struct playerInfoForDespawn : IBroadcast
    {
        public int id;
    }


}
