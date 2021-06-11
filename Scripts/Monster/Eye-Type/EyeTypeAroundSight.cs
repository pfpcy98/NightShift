using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTypeAroundSight : MonoBehaviour
{
    private string m_playerTag = "Player";
    public bool is_detectedPlayer { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(m_playerTag))
        {
            is_detectedPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag(m_playerTag))
        {
            is_detectedPlayer = false;
        }
    }
}
