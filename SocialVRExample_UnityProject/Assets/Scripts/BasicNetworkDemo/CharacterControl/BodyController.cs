using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script controls and returns the headIK target. Although it does not control the eyes, it needs to also reference them in this setup since the head IK is set
 * so that the character's eyes match the eyes' positions as defined by the eye-tracking API. To achieve this, this script furthermore implements an auxiliary construction 
 * to avoid feedback loops between the AdjustHeadToEyesPosition()-function and the head's IK processes. In particular, additional representations of the eyes are 
 * instantiated and parented to the headIK object; AdjustHeadToEyesPosition() then obtains the positions of these eye representations which are modulated directly as 
 * children of the headIK objects and unaffected by IK algorithms. Without this construction, the interaction between AdjustHeadToEyesPosition() and IK may result in 
 * self-reinforcing movement drifts.
 * by Marius Rubo, 2023
 * */

public class BodyController : MonoBehaviour
{
    [Tooltip("The character model's head object (not the IK target)")]
    [SerializeField] Transform head;

    [Tooltip("The character model's left eye (the object in the character's hierarchy, not the object holding the eyes' renderer)")]
    [SerializeField] Transform lEye;

    [Tooltip("The character model's right eye (the object in the character's hierarchy, not the object holding the eyes' renderer)")]
    [SerializeField] Transform rEye;

    [Tooltip("The IK target object for the head")]
    [SerializeField] Transform headIK;
    Transform lEyeIK;
    Transform rEyeIK;

    private void Awake()
    {
        InitializeIKSetup();
    }

    /// <summary>
    /// Additional representations for both eyes are created, aligned with the character's eyes and parented to the headIK object.
    /// </summary>
    void InitializeIKSetup()
    {
        headIK.position = head.position;
        headIK.rotation = head.rotation;

        lEyeIK = new GameObject().transform;
        lEyeIK.gameObject.name = "lEyeIK";
        lEyeIK.transform.parent = headIK;
        lEyeIK.transform.position = lEye.position;
        lEyeIK.transform.rotation = lEye.rotation;

        rEyeIK = new GameObject().transform;
        rEyeIK.gameObject.name = "rEyeIK";
        rEyeIK.transform.parent = headIK;
        rEyeIK.transform.position = rEye.position;
        rEyeIK.transform.rotation = rEye.rotation;
    }

    /// <summary>
    /// Position and rotation of the headIK object. 
    /// </summary>
    public Vector3 HeadIKPosition
    {
        get => headIK.position;
        set => headIK.position = value;
    }
    public Quaternion HeadIKRotation
    {
        get => headIK.rotation;
        set => headIK.rotation = value;
    }

    /// <summary>
    /// Positions of the additional eye representations which are parented to the head IK target.
    /// They are positioned once when this script is enabled. From that point forward, they are only repositioned in their role as headIK target's children,
    /// and other scripts only need to read their positions. 
    /// </summary>
    public Vector3 lEyeIKPosition
    {
        get => lEyeIK.position;
    }
    public Vector3 rEyeIKPosition
    {
        get => rEyeIK.position;
    }

    /// <summary>
    /// Function to adjust the character's head so that its eyes are aligned with the participant's eyes.
    /// Previous single-user body illusion setups as well as social VR environents may more typically align the character's head with the participant's head directly using 
    /// head tracking, but note that in social VR setups where users are intended to be able to engage in mutual eye contact, such a procedure may not be accurate enough: 
    /// If a character's head differs in size or shape from the participant's head, an alignment based on head tracking will results in the character's eyes being located 
    /// above or below the points from which the participant is actually viewing the scene. When a person looks directly into her interaction partner's (character's) eyes 
    /// in such a setup, the partner will perceive her gaze to be directed above or below her own eyes, thus not experiencing eye-contact. By centering characters' head 
    /// positions on participants' eyes, eye contact can function normally. The function may typically only be called on each user's own avatar.
    /// 
    /// Note that eye positions alone are not sufficient to define the head's orientation (with the two eyes being in a defined position, the head may still be facing more 
    /// downwards or upwards). Therefore, the head's current local up vector is used to be able to fully define head position and orientation.  
    /// </summary>
    /// <param name="lEyePosition">The position of the left eye as obtained from the eye-tracking API, in world space.</param>
    /// <param name="rEyePosition">The position of the right eye as obtained from the eye-tracking API, in world space.</param>
    /// <param name="headUp">The head's local up vector, in world space</param>
    public void AdjustHeadToEyesPosition(Vector3 lEyePosition, Vector3 rEyePosition, Vector3 headUp)
    {
        Vector3 centroidEyes = (lEyePosition + rEyePosition) / 2; // centroid between measured eyes where character's eyes should be

        Vector3 lEyeCharacter = lEyeIKPosition; // here we use an additional construction to avoid feedback loops with the IK; see GazeController for details
        Vector3 rEyeCharacter = rEyeIKPosition;
        Vector3 centroidCharacterEyes = (lEyeCharacter + rEyeCharacter) / 2;

        Quaternion eyesCentroidRotation = Quaternion.LookRotation(CalculateHeadForward(lEyePosition, rEyePosition, headUp));
        Quaternion eyesCharacterCentroidRotation = Quaternion.LookRotation(CalculateHeadForward(lEyeCharacter, rEyeCharacter, HeadIKRotation * Vector3.up)); // last is local up of head ik

        Vector3 targetPosition = HeadIKPosition + (centroidEyes - centroidCharacterEyes);
        Quaternion targetRotation = HeadIKRotation * Quaternion.Inverse(eyesCharacterCentroidRotation) * eyesCentroidRotation;

        HeadIKPosition = targetPosition;
        HeadIKRotation = targetRotation;
    }


    /// <summary>
    /// Compute the head's forward vector to conform with the eye positions and the head's up vector.
    /// The function could be understood as a general geometry helper, but imo is so specific to this use case that it may be most appropriately located right here. 
    /// </summary>
    /// <param name="lEyePosition">The position of the left eye as obtained from the eye-tracking API, in world space.</param>
    /// <param name="rEyePosition">The position of the right eye as obtained from the eye-tracking API, in world space.</param>
    /// <param name="headUp">The head's local up vector, in world space</param>
    static Vector3 CalculateHeadForward(Vector3 leftEyePosition, Vector3 rightEyePosition, Vector3 headUp)
    {
        Vector3 rightVector = (rightEyePosition - leftEyePosition).normalized;
        Vector3 headForward = -Vector3.Cross(headUp, rightVector).normalized;
        return headForward;
    }
}
