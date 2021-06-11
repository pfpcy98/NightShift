using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumpscare_Phone : MonoBehaviour
{
    [SerializeField]
    private AudioClip phoneRingSound = null;
    [SerializeField]
    private float phoneRingTime = 30.0f;

    private const string playerTag = "Player";
    private AudioSource m_audioSource = null;
    private CallAnswer m_callAnswer = null;

    private bool is_activated = false;
    private bool is_ringing = false;
    private float remain_ringing = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        if(m_audioSource != null &&
            phoneRingSound != null)
        {
            m_audioSource.loop = true;
            m_audioSource.clip = phoneRingSound;

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
            Debug.LogError(name + " : This object not contained any sound!");
            Destroy(gameObject);
        }

        m_callAnswer = GetComponentInChildren<CallAnswer>();
        if(m_callAnswer != null)
        {
            m_callAnswer.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(is_ringing)
        {
            remain_ringing -= Time.deltaTime;

            if(remain_ringing <= 0.0f)
            {
                m_audioSource.Stop();
                m_callAnswer.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!is_activated)
        {
            if (other.gameObject.CompareTag(playerTag))
            {
                is_activated = true;
                remain_ringing = phoneRingTime;
                is_ringing = true;

                m_audioSource.Play();

                if(m_callAnswer != null)
                {
                    m_callAnswer.gameObject.SetActive(true);
                }
            }
        }
    }

    public void CallAnswered()
    {
        is_ringing = false;
        m_audioSource.Stop();
    }
}
