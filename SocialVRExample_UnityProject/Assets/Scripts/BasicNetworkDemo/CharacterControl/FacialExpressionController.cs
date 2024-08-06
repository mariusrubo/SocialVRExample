using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* This script implements facial expression definitions (e.g., a "smiling" value of 0.4 on a scale from 0 to 1) onto the specific character.
 * In this example using MakeHuman characters, smiling is realized based on a range of hypothetical joints (somewhat misleadingly named 'bones' in 
 * computer graphics lingo) inside the skull. Other character models such as those by iClone Character Creator instead use 'blendshapes', which 
 * are definitions for how the character's skin is warped and more closely mimic the effects of facial muscle activity. Smiling may be implemented 
 * using a single blendshape or a combination of more fine-grained blendshapes. In a typical social VR setup, a larger number of facial expressions
 * may typically be implemented along the same logic as in this example.
 * by Marius Rubo, 2023
 * */

public class FacialExpressionController : MonoBehaviour
{
    [SerializeField] Transform jaw;
    [SerializeField] Transform levator02_L;
    [SerializeField] Transform levator02_R;
    [SerializeField] Transform orbicularis03_L;
    [SerializeField] Transform orbicularis03_R;
    [SerializeField] Transform temporalis01_L;
    [SerializeField] Transform temporalis01_R;
    [SerializeField] Transform risorius02_L;
    [SerializeField] Transform risorius02_R;

    Vector3 jaw_Smile0 = new Vector3(147.789f, 0, 0); Vector3 jaw_Smile1 = new Vector3(155.1f, 0, 0);
    Vector3 levator02_L_Smile0 = new Vector3(63.832f, 22.937f, 157.858f); Vector3 levator02_L_Smile1 = new Vector3(81.9f, 23.4f, 151.6f);
    Vector3 levator02_R_Smile0 = new Vector3(63.832f, -22.937f, -157.858f); Vector3 levator02_R_Smile1 = new Vector3(81.9f, -23.4f, -151.6f);
    Vector3 orbicularis03_L_Smile0 = new Vector3(96.27499f, 47.252f, -120.742f); Vector3 orbicularis03_L_Smile1 = new Vector3(86.5f, 47.252f, -120.742f);
    Vector3 orbicularis03_R_Smile0 = new Vector3(96.27499f, -47.25201f, 120.742f); Vector3 orbicularis03_R_Smile1 = new Vector3(86.5f, -47.25201f, 120.742f);
    Vector3 temporalis01_L_Smile0 = new Vector3(-56.622f, 107.709f, 99.129f); Vector3 temporalis01_L_Smile1 = new Vector3(-71.3f, 107.709f, 99.129f);
    Vector3 temporalis01_R_Smile0 = new Vector3(-56.622f, -107.709f, -99.129f); Vector3 temporalis01_R_Smile1 = new Vector3(-71.3f, -107.709f, -99.129f);
    Vector3 risorius02_L_Smile0 = new Vector3(-89.271f, -20.855f, 21.48f); Vector3 risorius02_L_Smile1 = new Vector3(-62f, -20.855f, 21.48f);
    Vector3 risorius02_R_Smile0 = new Vector3(-89.271f, 20.855f, -21.48f); Vector3 risorius02_R_Smile1 = new Vector3(-62f, 20.855f, -21.48f);

    /// <summary>
    /// A single float defining the extent to which the character is smiling, typically directly passed from the character's CharacterBehaviorController.
    /// </summary>
    float smiling;
    public float Smiling
    {
        get => smiling;
        set
        {
            smiling = value;
            SetSmiling(value);
        }
    }

    /// <summary>
    /// Implementing smiling onto the specific character.
    /// In this example using MakeHuman characters, smiling is constructed based on 9 hypothetical facial joints. Implementing smiling on character models which
    /// use blendshapes may be simpler as blendshape definitions may more directly conform to the face-tracking API's output variables. 
    /// </summary>
    /// <param name="SmilingIntensity">Current intensity of the smile from 0 to 1</param>
    void SetSmiling(float SmilingIntensity)
    {
        jaw.localEulerAngles = Vector3.Lerp(jaw_Smile0, jaw_Smile1, SmilingIntensity);
        levator02_L.localEulerAngles = Vector3.Lerp(levator02_L_Smile0, levator02_L_Smile1, SmilingIntensity);
        levator02_R.localEulerAngles = Vector3.Lerp(levator02_R_Smile0, levator02_R_Smile1, SmilingIntensity);
        orbicularis03_L.localEulerAngles = Vector3.Lerp(orbicularis03_L_Smile0, orbicularis03_L_Smile1, SmilingIntensity);
        orbicularis03_R.localEulerAngles = Vector3.Lerp(orbicularis03_R_Smile0, orbicularis03_R_Smile1, SmilingIntensity);
        temporalis01_L.localEulerAngles = Vector3.Lerp(temporalis01_L_Smile0, temporalis01_L_Smile1, SmilingIntensity);
        temporalis01_R.localEulerAngles = Vector3.Lerp(temporalis01_R_Smile0, temporalis01_R_Smile1, SmilingIntensity);
        risorius02_L.localEulerAngles = Vector3.Lerp(risorius02_L_Smile0, risorius02_L_Smile1, SmilingIntensity);
        risorius02_R.localEulerAngles = Vector3.Lerp(risorius02_R_Smile0, risorius02_R_Smile1, SmilingIntensity);
    }
}
