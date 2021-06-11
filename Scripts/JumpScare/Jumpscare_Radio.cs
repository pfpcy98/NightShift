using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.6.0 생성 : 라디오 점프스케어 소스코드
public class Jumpscare_Radio : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField]
    [Tooltip("오디오 재생 중에 사용될 오디오 파일입니다.")]
    private AudioClip audioSound = null;
    [SerializeField]
    [Tooltip("오디오 재생 중에 라디오 전면에 사용될 머티리얼입니다.")]
    private Material turnOnMaterial = null;

    [Space(10f)]
    [SerializeField]
    [Tooltip("라디오 전면 Mesh Renderer 컴포넌트입니다.")]
    private MeshRenderer radioMeshRenderer = null;

    private const string playerTag = "Player";
    private AudioSource m_audioSource = null;

    private bool is_activated = false;

    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource != null &&
            audioSound != null)
        {
            m_audioSource.loop = true;
            m_audioSource.clip = audioSound;

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
            Debug.LogError(name + " : This object can't play any sound!");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!is_activated)
        {
            if (other.gameObject.CompareTag(playerTag))
            {
                is_activated = true;

                if(radioMeshRenderer != null &&
                    turnOnMaterial != null)
                {
                    radioMeshRenderer.material = turnOnMaterial;
                }

                m_audioSource.Play();
            }
        }
    }
}
