using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* This script controls the headset to mimic a VR setups. It can return the headset's local up direction much like a motion tracking API used in VR does. In this setup
 * the headset's orientation is sufficient to define its position as well since the head is centered to match the tracked eyes as defined by the eye-tracking API.
 * The specifics of how the mock headset and eyes are being controlled in this example may not be directly relevant when setting up a social VR environment but serve 
 * to provide a functional example which can be directly tested even on a single computer without putting into operation VR systems and local networks. In short, 
 * the character is controlled by (1) directing its eyes towards a look-at target which is being towards the location pointed at by the mouse cursor in another 
 * script (MouseCursorRaycaster), (2) slightly rotate the head towards that direction as well and (3) moving the head laterally by pressing the arrow keys.
 * by Marius Rubo, 2023
 * */

public class MockMotionTrackingAPI : MonoBehaviour
{
    /// <summary>
    /// Referencing the the mock headset, the eyes and the lookAtTarget (towards which headset and eyes are being rotated here). These are required for this script's
    /// core functionality, which is to allow to exemplarily control the character's gaze and head movements.
    /// </summary>
    [SerializeField] Transform mockHeadset;
    [SerializeField] Transform lookAtTarget;

    /// <summary>
    /// The headset's up vector as well as the positions and rotations of the two eyes can be returned here. 
    /// </summary>
    public Vector3 MockHeadsetUpDirection { get { return mockHeadset.up; } }

    /// <summary>
    /// In addition, in order to implement lateral movements of the head along arrow key presses, we need to know the character's current reference point,
    /// store its initial (default) position, implement player controls which detect button presses and keep track of how the head should be moved at each point
    /// in time. This functionality could be removed from a really minimal example project, but is kept here to display how work with the new input system in structured.
    /// </summary>
    [SerializeField] CurrentReferencePoint currentReferencePoint;
    Vector3 mockHeadsetInitialPosition;
    private PlayerControls1 playerControls1;
    float leaningLeft, leaningRight, leaningForward, leaningBackward;
    Vector2 Leaning;


    void Start()
    {
        mockHeadsetInitialPosition = currentReferencePoint.Transform.InverseTransformPoint(mockHeadset.position);
    }


    private void Awake()
    {
        playerControls1 = new PlayerControls1();
    }


    private void OnEnable()
    {
        EnableControls();
    }

    private void OnDisable()
    {
        DisableControls();
    }


    void Update()
    {
        ControlLeaning();
        LookAtTarget();
    }

    /// <summary>
    /// The eyes are rotated to look at a target. The head is likewise rotated in that direction, but only slightly so.
    /// </summary>
    void LookAtTarget()
    {
        Vector3 directionToTarget = lookAtTarget.position - mockHeadset.position;
        Vector3 directionToTargetPartial = Vector3.Lerp(transform.parent.forward, directionToTarget, 0.25f); // assume that head does not fully rotate towards target but only partly
        mockHeadset.rotation = Quaternion.LookRotation(directionToTargetPartial);
    }

    /// <summary>
    /// The head moves laterally along arrow key presses. It moves back to its original position when the key is let go. This movements considers the current reference point.
    /// </summary>
    void ControlLeaning()
    {
        float decayFactor = 0.98f; // Mathf.Pow(0.5f, Time.deltaTime / 1); // how head moves back to original position; use commented code to adjust to fps, but not really needed for this example
        Leaning = Leaning * decayFactor;
        Vector2 addToLeaning = new Vector2(leaningBackward - leaningForward, leaningRight - leaningLeft);
        Leaning = Leaning + addToLeaning * 0.0004f;
        Vector3 mockHeadSetLocalPosition = mockHeadsetInitialPosition + new Vector3(Leaning.x, 0, Leaning.y);
        mockHeadset.position = currentReferencePoint.Transform.TransformPoint(mockHeadSetLocalPosition);
    }


    #region PlayerInput

    /// <summary>
    /// Player input, here mere arrow key presses, are detected in this example using Unity's new input system. While detecting button presses required less code
    /// in the old input system (a mere "if (Input.GetKeyDown("up"))"), the new input system features improved timing and modularity (e.g., assigning components from different
    /// hardware to the same functionality). I use it in this example as the type of setup is often used when interacting with VR hardware or APIs and often causes confusion
    /// for developers who are unfamiliar with it. To start with the new input system in a different project, follow these steps: (1) install unity input system from the 
    /// package manager, (2) activate it in Edit->Project Settings->Player->Active Input Handling, (3) manually create new inputactions and link the corresponding bars to 
    /// the corresponding actions, and create a C# script from it, (4) subscribe and unsubscribe as seen here in this sript.
    /// </summary>
    void EnableControls()
    {
        playerControls1.Player.Enable();
        playerControls1.Player.LeanLeft.performed += OnLeanLeftButtonPressed;
        playerControls1.Player.LeanRight.performed += OnLeanRightButtonPressed;
        playerControls1.Player.LeanForward.performed += OnLeanForwardButtonPressed;
        playerControls1.Player.LeanBackward.performed += OnLeanBackwardButtonPressed;

        playerControls1.Player.LeanLeft.canceled += OnLeanLeftButtonReleased;
        playerControls1.Player.LeanRight.canceled += OnLeanRightButtonReleased;
        playerControls1.Player.LeanForward.canceled += OnLeanForwardButtonReleased;
        playerControls1.Player.LeanBackward.canceled += OnLeanBackwardButtonReleased;
    }

    void DisableControls()
    {
        playerControls1.Player.LeanLeft.performed -= OnLeanLeftButtonPressed;
        playerControls1.Player.LeanRight.performed -= OnLeanRightButtonPressed;
        playerControls1.Player.LeanForward.performed -= OnLeanForwardButtonPressed;
        playerControls1.Player.LeanBackward.performed -= OnLeanBackwardButtonPressed;

        playerControls1.Player.LeanLeft.canceled -= OnLeanLeftButtonReleased;
        playerControls1.Player.LeanRight.canceled -= OnLeanRightButtonReleased;
        playerControls1.Player.LeanForward.canceled -= OnLeanForwardButtonReleased;
        playerControls1.Player.LeanBackward.canceled -= OnLeanBackwardButtonReleased;
        playerControls1.Player.Disable();
    }

    void OnLeanLeftButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leaningLeft = 1;
        }
    }
    void OnLeanRightButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leaningRight = 1;
        }
    }
    void OnLeanForwardButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leaningForward = 1;
        }
    }
    void OnLeanBackwardButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leaningBackward = 1;
        }
    }
    void OnLeanLeftButtonReleased(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            leaningLeft = 0;
        }
    }
    void OnLeanRightButtonReleased(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            leaningRight = 0;
        }
    }
    void OnLeanForwardButtonReleased(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            leaningForward = 0;
        }
    }
    void OnLeanBackwardButtonReleased(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            leaningBackward = 0;
        }
    }

    #endregion
}
