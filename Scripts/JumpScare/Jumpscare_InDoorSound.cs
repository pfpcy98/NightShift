using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumpscare_InDoorSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip inDoorSound = null;

    private const string playerTag = "Player";
    private AudioSource m_audioSource = null;

    private bool is_activated = false;

    // Start is called before the first frame update
    void Start()
    {
        m_audioSource = GetComponentInChildren<AudioSource>();

        if(m_audioSource != null)
        {
            AnimationCurve ac = m_audioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            Keyframe[] keys = new Keyframe[1];

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value = 1f;
            }

            ac.keys = keys;
            m_audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, ac);
        }
        else
        {
            Debug.LogError(name + " : This object cannot play own sound!");
            is_activated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!is_activated)
        {
            if (other.gameObject.CompareTag(playerTag))
            {
                is_activated = true;

                if (inDoorSound != null)
                {
                    m_audioSource.PlayOneShot(inDoorSound);
                }
            }
        }
    }
}
