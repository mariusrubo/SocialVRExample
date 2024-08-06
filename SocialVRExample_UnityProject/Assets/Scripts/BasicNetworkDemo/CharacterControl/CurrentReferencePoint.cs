using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Here we merely store the current reference point for this character, i.e., the side of the table on which it is sitting.
 * When connecting to the server, the reference point is updated in ClientSetReferencePoint, making sure that only one player sits at each side of the table.
 * The specifics of this script may not be directly relevant when setting up a social VR environment. While it is possible to represent all data relative to 
 * the reference point (e.g., to directly compare when users are leaning forward or backward with respect to their side of the table), in this example data are 
 * generally handled in global space, and for remote players the reference point is not even directly known. 
 * by Marius Rubo, 2023
 * */
public class CurrentReferencePoint : MonoBehaviour
{
    [SerializeField]
    private Transform _tr;
    public Transform Transform
    {
        get { return _tr; }
        set { _tr = value; }
    }
}
