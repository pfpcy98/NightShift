using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.6.0 ���� : ���� �������ɾ� �ҽ��ڵ�
public class Jumpscare_Radio : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField]
    [Tooltip("����� ��� �߿� ���� ����� �����Դϴ�.")]
    private AudioClip audioSound = null;
    [SerializeField]
    [Tooltip("����� ��� �߿� ���� ���鿡 ���� ��Ƽ�����Դϴ�.")]
    private Material turnOnMaterial = null;

    [Space(10f)]
    [SerializeField]
    [Tooltip("���� ���� Mesh Renderer ������Ʈ�Դϴ�.")]
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
