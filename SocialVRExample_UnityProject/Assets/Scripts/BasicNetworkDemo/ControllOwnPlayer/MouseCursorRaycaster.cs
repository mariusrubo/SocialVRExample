using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A simple script which sets the character's look-at target to where the mouse cursor is pointed on the screen, allowing to simulate head movements.
 * by Marius Rubo, 2023
 * */

public class MouseCursorRaycaster : MonoBehaviour
{
    void Update()
    {
        SetToPositionPointedAtByCursor();
    }

    /// <summary>
    /// Mouse position in screen space is used to project a ray from the main camera and to detect where it intersects with an object.
    /// </summary>
    void SetToPositionPointedAtByCursor()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) transform.position = hit.point;
    }
}
