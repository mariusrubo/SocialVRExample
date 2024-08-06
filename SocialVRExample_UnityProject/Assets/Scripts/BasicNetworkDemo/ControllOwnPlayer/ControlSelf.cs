using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* Data from the headset, the eyes, and facial expression (smiling) are transferred to the character's CharacterBehaviorController which implements data on the character
 * model. For this example the headset, eyes and facial expression are being controlled without VR equipment in the scripts MockEyeTrackingAPI and
 * MockFaceTrackingAPI, but data transfer to the CharacterBehaviorController are identical when data are instead streamed from actual eye-tracking and face-tracking APIs.
 * by Marius Rubo, 2023
 * */

public class ControlSelf : MonoBehaviour
{
    [SerializeField] MockEyeTrackingAPI mockEyeTrackingAPI;
    [SerializeField] MockFaceTrackingAPI mockFaceTrackingAPI;
    [SerializeField] MockMotionTrackingAPI mockMotionTrackingAPI;
    [SerializeField] CharacterBehaviorController characterBehaviorController;

    void Update()
    {
        SetHeadPositionAndRotation();
        TransferEyeGaze();
        TransferFacialExpression();
    }


    /// <summary>
    /// Data are transferred to the CharacterBehaviorController one by one. In principle, the eye-tracking and face-tracking APIs could directly transfer their data
    /// but adding this layer where data transfer is bundled may enhance readability and adaptability. 
    /// </summary>
    void SetHeadPositionAndRotation()
    {
        characterBehaviorController.AdjustHeadToEyesPosition(mockEyeTrackingAPI.lEyePosition, mockEyeTrackingAPI.rEyePosition, mockMotionTrackingAPI.MockHeadsetUpDirection);
    }

    void TransferEyeGaze()
    {
        characterBehaviorController.lEyeRotation = mockEyeTrackingAPI.lEyeRotation;
        characterBehaviorController.rEyeRotation = mockEyeTrackingAPI.rEyeRotation;
    }
    void TransferFacialExpression()
    {
        characterBehaviorController.Smiling = mockFaceTrackingAPI.Smiling;
    }


}
