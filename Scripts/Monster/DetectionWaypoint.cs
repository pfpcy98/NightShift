using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionWaypoint : MonoBehaviour
{
    [SerializeField]
    private bool is_Mother_Waypoint = false;
    [SerializeField]
    private DetectionWaypoint next_Waypoint = null;

    public bool IsMotherWaypoint()
    {
        return is_Mother_Waypoint;
    }

    public DetectionWaypoint GetNextWaypoint()
    {
        return next_Waypoint;
    }
}
