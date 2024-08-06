using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script serves as the center piece in controlling each character model's behavior in the scene. The same script is used for the character which
 * represents the self (where behavior is typically driven by information from the headset's sensors) and for all other characters in the scene,
 * representing other participants (behavior of these characters, called 'remote players' in netcode jargon, are driven by data received from the server).
 * 
 * Using the same center script for controlling both the user's own avatar and the remote players is not strictly necessary when setting up a social VR software. Instead, 
 * other solutions may directly drive a user's own avatar by the headset sensors using classes which more strongly focus on modularity and encapsulation (i.e., one module
 * only focussing on transferring head movements, one module only focussing on controlling eye movements etc.). However, by using a center script - the same for the self
 * and for remote players - data can be more easily integrated by subsequent logfile writers.
 * 
 * Note also that this script does not control each component of the character directly, but calls other, specialized scripts such as the character's 
 * GazeController and FacialExpressionController. This more modular arrangement allows to more easily switch between different character models. For example, while some
 * character models define the direction in which an eye is looking as its "forward" vector, others define it as its "up" vector. Such idiosyncracies are handled in the
 * specific GazeController (which may need to be adapted for other character models) but are hidden from this center script.
 * by Marius Rubo, 2023
 * */

public class CharacterBehaviorController : MonoBehaviour
{
    /// <summary>
    /// This character's BodyController (the component which controlles the head). This remains constant and must be assigned manually. 
    /// </summary>
    [SerializeField] BodyController bodyController;

    /// <summary>
    /// This character's EyeController (the component which rotates its eyes). This remains constant and must be assigned manually. 
    /// </summary>
    [SerializeField] EyeController eyeController;


    /// <summary>
    /// This character's FacialExpressionController (the component which has direct access to its facial expressions). Also assign manually. 
    /// </summary>
    [SerializeField] FacialExpressionController facialExpressionController;

    /// <summary>
    /// Position of and rotation of the character transform. 
    /// Note that depending on how IK is set up, character position and rotation may or may not be altered as a character moves around, so ensure to track what
    /// you really need to track.
    /// </summary>
    public Vector3 CharacterPosition
    {
        get => transform.position;
        set => transform.position = value;
    }
    public Quaternion CharacterRotation
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }

    /// <summary>
    /// Position of and rotation of the head IK target.
    /// The headIK target represents directly how the head is positioned and oriented if the head IK weight is set to 1, as it will typically be the case in social VR. 
    /// If in any case the head IK weight is not set to 1, note that the headIK does not directly represent the head, and it may be more appropriate to 
    /// directly track the head instead. 
    /// </summary>
    public Vector3 headIKPosition
    {
        get => bodyController.HeadIKPosition;
        set => bodyController.HeadIKPosition = value;
    }
    public Quaternion headIKRotation
    {
        get => bodyController.HeadIKRotation;
        set => bodyController.HeadIKRotation = value;
    }

    /// <summary>
    /// Positions, rotations and forward directions of the character's eyes.
    /// Positions of the eyes can be read but can never be set diretly. Instead, eye positions are entirely defined by the head's movements, and eyes can only be 
    /// rotated inside their eye sockets. Note further that all data here represent the character's eyes, not directly the participant's eyes as they are registered 
    /// by the eye-tracker. These can typically be equated when using the below function "AdjustHeadToEyesPosition", which closely aligns the character to have its 
    /// eyes where the participant's eyes are. However, there may be situations (such as when embodying an avatar with substantially more narrow or wider eye distance) 
    /// when the character's eye positions may not be taken to directly represent the participant's eye positions, and eye-tracking analyses may be more appropriately 
    /// conducted based on data coming directly from the eye-tracker.
    /// </summary>
    public Vector3 lEyePosition
    {
        get => eyeController.lEyePosition;
    }
    public Vector3 rEyePosition
    {
        get => eyeController.rEyePosition;
    }
    public Quaternion lEyeRotation
    {
        get => eyeController.lEyeRotation;
        set => eyeController.lEyeRotation = value;
    }
    public Vector3 lEyeForward
    {
        get => lEyeRotation * Vector3.forward;
    }
    public Quaternion rEyeRotation
    {
        get => eyeController.rEyeRotation;
        set => eyeController.rEyeRotation = value;
    }
    public Vector3 rEyeForward
    {
        get => rEyeRotation * Vector3.forward;
    }

    /// <summary>
    /// A single float defining the extent to which the character is smiling from 0 (no smiling) to 1 (maximum possible smile).
    /// The character's FacialExpressionController deals with implementing the smile on the specific character model.
    /// </summary>
    public float Smiling
    {
        get => facialExpressionController.Smiling;
        set => facialExpressionController.Smiling = value;
    }

    /// <summary>
    /// Function to adjust the character's head so that its eyes are aligned with the participant's eyes. Forwarded to BodyController.
    /// </summary>
    /// <param name="lEyePosition">The position of the left eye as obtained from the eye-tracking API, in world space.</param>
    /// <param name="rEyePosition">The position of the right eye as obtained from the eye-tracking API, in world space.</param>
    /// <param name="headUp">The head's local up vector, in world space</param>
    public void AdjustHeadToEyesPosition(Vector3 lEyePosition, Vector3 rEyePosition, Vector3 headUp)
    {
        bodyController.AdjustHeadToEyesPosition(lEyePosition, rEyePosition, headUp);
    }
}
