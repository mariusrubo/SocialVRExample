using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script sets and returns the eyes' rotation and returns their positions. It adjusts the transfer of gaze data to a specific character model. The example below handles the case where (as in MakeHuman characters) the eyes' looking
 * direction is indicated by their local up vector. Above the particular implementation of eye rotations in this script, looking directions are consistently viewed as being
 * represented by the eyes' forward directions, and the definition need not be changed if this particular script is adapted for other character models. 
 * by Marius Rubo, 2023
 * */

public class EyeController : MonoBehaviour
{
    [Tooltip("The character model's left eye (the object in the character's hierarchy, not the object holding the eyes' renderer)")]
    [SerializeField] Transform lEye;

    [Tooltip("The character model's right eye (the object in the character's hierarchy, not the object holding the eyes' renderer)")]
    [SerializeField] Transform rEye;

    /// <summary>
    /// Position and rotation of the character's eyes.
    /// Position can only be read, not set as it is defined by the head. Rotation can be read and set, adapting the definition of eyes' looking direction ("forward" in
    /// other scripts in this project) to the particular character model's definition.
    /// </summary>
    public Vector3 lEyePosition
    {
        get => lEye.position;
    }
    public Quaternion lEyeRotation
    {
        get => lEye.rotation * Quaternion.Inverse(Quaternion.FromToRotation(-Vector3.forward, Vector3.up)); // adapt depending on character model
        set => lEye.rotation = value * Quaternion.FromToRotation(-Vector3.forward, Vector3.up);
    }
    public Vector3 rEyePosition
    {
        get => rEye.position;
    }
    public Quaternion rEyeRotation
    {
        get => rEye.rotation * Quaternion.Inverse(Quaternion.FromToRotation(-Vector3.forward, Vector3.up));
        set => rEye.rotation = value * Quaternion.FromToRotation(-Vector3.forward, Vector3.up);
    }
}
