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

/* Our own avatar's reference point is reset along a command from the server which ensures that every reference point (i.e., side of the table) is only used by one client. 
 * In social VR, this procedure ensures that interaction partners are really located on different sides of the table, also agreeing on who is where and consistently
 * placing everyone in the same location in the scene in all simulations. While the server dictates which client is to use which reference point, clients themselves
 * are responsible for positioning themselves relative to this reference point. An alignment of the physical world with the designated reference point in VR is implemented
 * elsewhere, in VV_Calibration. 
 * by Marius Rubo, 2023
 * */

public class ClientSetReferencePoint : MonoBehaviour
{
    /// <summary>
    /// Reference the self so that we can replace it in the scene when the reference point is changed. 
    /// </summary>
    [SerializeField] Transform Self;

    /// <summary>
    /// We must also store a list of possible reference points, which is known to all clients, so that the server can easily assign a point with only its index. In more 
    /// complex setups (i.e., not sitting on a particular table but moving freely in a scene), the server may instead directly send the position and rotation of generic
    /// reference points. 
    /// </summary>
    [SerializeField] List<Transform> referencePoints;

    /// <summary>
    /// CurrentReferencePoint is a script which only holds a reference to whatever reference point was decided to be used. It exists for the purpose of modularity
    /// (e.g., being able to switch networking on and off without compromising unrelated functionalities) and must be updated when the reference point is changed.
    /// </summary>
    [SerializeField] CurrentReferencePoint CurrentReferencePoint;

    private void Start()
    {
        RegisterBroadcasts();
    }
    private void OnDisable()
    {
        UnregisterBroadcasts();
    }

    /// <summary>
    /// A message from the server is received telling us to update our reference point. Only the server may command this in this setup.
    /// </summary>
    void OnReferencePointUpdateReceived(updateReferencePoint msg, Channel channel)
    {
        SetReferencePoint(msg.referencePointID);
    }

    /// <summary>
    /// The self is reset to be positioned towards the new reference point in the same way as it was towards the previous reference point.
    /// </summary>
    void SetReferencePoint(int pointIndex)
    {
        if (pointIndex >= 0 & pointIndex <= referencePoints.Count)
        {
            Transform currentReferencePoint = CurrentReferencePoint.Transform;
            Vector3 positionLocalToOldReference = currentReferencePoint.InverseTransformPoint(Self.position);
            Quaternion rotationLocalToOldReference = Quaternion.Inverse(currentReferencePoint.rotation) * Self.rotation;

            Transform newReference = referencePoints[pointIndex];
            Self.position = newReference.TransformPoint(positionLocalToOldReference);
            Self.rotation = newReference.rotation * rotationLocalToOldReference;

            CurrentReferencePoint.Transform = newReference;
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
            clientManager.RegisterBroadcast<updateReferencePoint>(OnReferencePointUpdateReceived);
        }
        else { Debug.Log("Broadcast could not be registered!"); }
    }

    void UnregisterBroadcasts()
    {
        var clientManager = InstanceFinder.ClientManager;
        if (clientManager != null)
        {
            clientManager.UnregisterBroadcast<updateReferencePoint>(OnReferencePointUpdateReceived);
        }
    }
    #endregion
}
