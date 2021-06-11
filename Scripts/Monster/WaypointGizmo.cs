using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGizmo : MonoBehaviour
{
    [SerializeField]
    private Color gizmoColor = Color.white;
    [SerializeField]
    private float gizmoSize = 0.25f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}
