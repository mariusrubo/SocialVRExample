using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script mimics an eye-tracking API in that it returns both eyes' positions and rotations. Here we also orient the eyes towards a lookAtTarget. Note that when 
 * communicating with an actual eye-tracking API, the process by which it determines each eye's position and orientation is typically not of interest.
 * by Marius Rubo, 2023
 * */

public class MockEyeTrackingAPI : MonoBehaviour
{
    /// <summary>
    /// Referencing the eyes as "tracked" by the eye-tracking API and the lookAtTarget (towards which headset and eyes are being rotated here).
    /// </summary>
    [SerializeField] Transform lEyeTracked;
    [SerializeField] Transform rEyeTracked;
    [SerializeField] Transform lookAtTarget;

    /// <summary>
    /// The headset's up vector as well as the positions and rotations of the two eyes can be returned here. 
    /// </summary>
    public Vector3 lEyePosition { get { return lEyeTracked.position; } }
    public Vector3 rEyePosition { get { return rEyeTracked.position; } }
    public Quaternion lEyeRotation { get { return lEyeTracked.rotation; } }
    public Quaternion rEyeRotation { get { return rEyeTracked.rotation; } }

    void Update()
    {
        LookAtTarget();
    }

    /// <summary>
    /// The eyes are rotated to look at a target. The head is likewise rotated in that direction, but only slightly so.
    /// </summary>
    void LookAtTarget()
    {
        lEyeTracked.LookAt(lookAtTarget);
        rEyeTracked.LookAt(lookAtTarget);
    }

}
