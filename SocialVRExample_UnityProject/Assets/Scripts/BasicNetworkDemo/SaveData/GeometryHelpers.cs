using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Social VR projects may typically involve processing of geometry in 3D. While some functions are specific to one context and they may be more suitably noted
 * in the specific script, others may represent more generic operations and can be noted here.
 * by Marius Rubo, 2023
 * */
public static class GeometryHelpers
{
    /// <summary>
    /// Calculate a Ray's deviation in horizontal (y) and vertical (x) direction from perfectly hitting a target position.
    /// This operation is central when analyzing gaze in 3D space with regards to objects of interest and we wish to collect data points in the form of 
    /// "the person was looking 8 degrees to the right of the conspecific's face". While data in screen-based eye-tracking are often returned in a 2D format
    /// by the eye-tracker's API, making it easy to determine how the gazed-at position relates to objects or regions of interest, gaze in 3D space is 
    /// represented as a Ray consisting of the position and looking direction of an eye (both as Vector3s). A quick way to obtain the angular difference between
    /// measured gaze and an object would be Unity's build-in Vector3.Angle, but the result does not tell us if gaze missed the object of interest to its left or right, 
    /// went above or below it. The procecure below transposes 3D data to a representation on a 2D plane where the target is at the coordinates (0,0). These data can
    /// then be analysed analogously as traditional screen-based eye-tracking data (e.g., filtering, aggregation of the two eyes, applying drift corrections). 
    /// </summary>
    /// <param name="ray">Ray consisting of the position and forward (= looking) direction of the object to be analyzed</param>
    /// <param name="target">position of the object of interest with regards to which the ray is to be analyzed.</param>
    public static Vector2 ProjectRayOnTarget(Ray ray, Vector3 target)
    {
        Vector3 directionToTarget = (target - ray.origin).normalized;
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget, Vector3.up);
        Quaternion actualRotation = Quaternion.LookRotation(ray.direction.normalized, Vector3.up);
        Quaternion rotationDeviation = Quaternion.Inverse(rotationToTarget) * actualRotation;
        Vector3 localEulerAngles = rotationDeviation.eulerAngles;

        if (localEulerAngles.x < -180) localEulerAngles.x += 360;
        else if (localEulerAngles.x > 180) localEulerAngles.x -= 360;
        if (localEulerAngles.y < -180) localEulerAngles.y += 360;
        else if (localEulerAngles.y > 180) localEulerAngles.y -= 360;

        return (new Vector2(-localEulerAngles.x, localEulerAngles.y));
    }
}
