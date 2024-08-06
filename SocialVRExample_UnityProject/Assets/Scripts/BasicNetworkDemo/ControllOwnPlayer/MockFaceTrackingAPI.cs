using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* This script controls the character's facial expression to mimic a VR setups. It can return its current smiling on a scale from 0 to 1 much like a face-tracking API does.
 * The specifics of how the facial expression is being controlled in this example may not be directly relevant when setting up a social VR environment but serves as
 * an example. Here, the character smiles when the space bar is being pressed. 
 * by Marius Rubo, 2023
 * */

public class MockFaceTrackingAPI : MonoBehaviour
{
    /// <summary>
    /// PlayerControls1 is used to detect whether the smile button (space bar) is being pressed and to manipulate the current smiling accordingly.
    /// </summary>
    private PlayerControls1 playerControls1;
    bool isSmileButtonPressed;
    float smiling;
    public float Smiling { get { return smiling; } }


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

    private void Update()
    {
        UpdateFacialExpression();
    }

    /// <summary>
    /// Smiling is enhanced when the respective button is pressed untill it reaches it maximum value of 1. When the button is released, smiling smoothly decreases down to 0.
    /// </summary>
    void UpdateFacialExpression()
    {
        if (isSmileButtonPressed)
        {
            smiling += Time.deltaTime * 14f;
            if (smiling > 1) smiling = 1;
        }
        float decayFactor = 0.94f; // Mathf.Pow(0.5f, Time.deltaTime / 0.2f); // how head moves back to original position; use commented code to adjust to fps, but not really needed for this example
        smiling = smiling * decayFactor;
    }

    #region PlayerInput
    /// <summary>
    /// Subscribing and unsubscribing to player control events. See MockEyeTrackingAPI for some more details on how Unity's new input system is used.
    /// </summary>
    private void EnableControls()
    {
        playerControls1.Player.Enable();
        playerControls1.Player.Smile.performed += OnSmilebuttonPressed;
        playerControls1.Player.Smile.canceled += OnSmilebuttonReleased;
    }
    private void DisableControls()
    {
        playerControls1.Player.Smile.performed -= OnSmilebuttonPressed;
        playerControls1.Player.Smile.canceled -= OnSmilebuttonReleased;
        playerControls1.Player.Disable();
    }
    private void OnSmilebuttonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSmileButtonPressed = true;
        }
    }
    private void OnSmilebuttonReleased(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            isSmileButtonPressed = false;
        }
    }
    #endregion
}
