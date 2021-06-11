using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 1.5.5 변경 사항
 * 충돌 즉시 삭제가 아니라 사운드 재생을 시작
 * 1회 재생이 이루어진 이후 객체를 삭제
*/

public class Jumpscare_MadTeddy : MonoBehaviour
{
    [SerializeField]
    private AudioClip onFadeoutSound = null;

    private const string playerTag = "Player";
    private AudioSource m_audioSource = null;
    private MeshRenderer m_meshRenderer = null;

    private bool is_Hit = false;

    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        if(m_audioSource != null)
        {
            m_audioSource.loop = false;
            m_audioSource.clip = onFadeoutSound;

            AnimationCurve ac = m_audioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            Keyframe[] keys = new Keyframe[1];

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value = 1f;
            }

            ac.keys = keys;
            m_audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, ac);
        }

        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if(is_Hit &&
            !m_audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!is_Hit)
        {
            if (other.CompareTag(playerTag))
            {
                is_Hit = true;

                if (m_audioSource != null &&
                    onFadeoutSound != null)
                {
                    m_audioSource.Play();
                }

                if(m_meshRenderer !=null)
                {
                    m_meshRenderer.enabled = false;
                }
            }
        }
    }
}
