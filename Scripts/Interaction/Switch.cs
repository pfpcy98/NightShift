using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : InteractionThings
{
    // 스위치 상호작용 사운드
    [SerializeField]
    private AudioClip interactionSound = null;

    // 상호작용 기능을 적용할 객체 배열
    [SerializeField]
    private Light[] connectedLights = null;

    [SerializeField]
    private float noiseRange = 50.0f;
    [SerializeField]
    private float noiseDuration = 5.0f;

    private AudioSource m_audioSource = null;
    private MeshRenderer m_meshRenderer = null;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void FirstInteractCheck()
    {
        if(isFirstInteracted)
        {
            isFirstInteracted = false;
            PlayerStatusManager.GetInstance().ShowWarningText("이걸 작동하면 경고음과 함께 주변에 불이 들어올꺼야 그렇지만 어쩔수 없지");
        }
    }

    public override void Execute()
    {
        if(connectedLights != null)
        {
            for(int i = 0; i < connectedLights.Length; i++)
            {
                connectedLights[i].enabled = !connectedLights[i].enabled;

                SphereCollider col = connectedLights[i].GetComponent<SphereCollider>();
                if(col != null)
                {
                    col.enabled = !col.enabled;
                }
            }
        }

        m_meshRenderer.material.SetColor("_EmissionColor", Color.blue);

        if (m_audioSource != null &&
            interactionSound != null)
        {
            m_audioSource.PlayOneShot(interactionSound);
        }

        NoiseManager.GetInstance().CreateObjectNoise(transform.position, noiseRange, noiseDuration);

        isInteracted = true;
    }
}
