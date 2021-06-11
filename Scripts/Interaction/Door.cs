using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractionThings
{
    // 문이 열리기 위해 필요한 스위치
    [SerializeField]
    private InteractionThings[] neededSwitches = null;

    // 문 열리는 사운드
    [SerializeField]
    private AudioClip interactionSound = null;

    // 이 문을 염으로서 넘어가는 씬의 이름
    [SerializeField]
    private string nextSceneName = null;

    [SerializeField]
    private float noiseRange = 20.0f;
    [SerializeField]
    private float noiseDuration = 5.0f;

    private const int openFrame = 60;
    private float openDistance = 0;

    private AudioSource m_audioSource = null;
    private Transform m_parentTransform = null;
    private BoxCollider m_parentCollider = null;

    private void Start()
    {
        m_parentTransform = GetComponentInParent<Transform>();
        m_parentCollider = GetComponentInParent<BoxCollider>();
        m_audioSource = GetComponentInParent<AudioSource>();
        openDistance = m_parentTransform.localScale.y;
    }

    public override void FirstInteractCheck()
    {
        return;
    }

    public override void Execute()
    {
        if(neededSwitches == null)
        {
            DoorOpen();
        }

        else
        {
            bool isAllSwitchOn = true;

            for(int i = 0; i < neededSwitches.Length; i++)
            {
                if(!neededSwitches[i].isInteracted)
                {
                    isAllSwitchOn = false;
                }
            }

            if(isAllSwitchOn)
            {
                DoorOpen();
            }
            else
            {
                PlayerStatusManager.GetInstance().ShowWarningText("전기가 들어와야 열릴 것 같아");
            }
        }
    }

    private void DoorOpen()
    {
        if (nextSceneName != null)
        {
            PlayerStatusManager.GetInstance().StageClear(nextSceneName);
        }

        if (m_audioSource != null &&
        interactionSound != null)
        {
            m_audioSource.PlayOneShot(interactionSound);
        }

        NoiseManager.GetInstance().CreateObjectNoise(m_parentTransform.position, noiseRange, noiseDuration);

        isInteracted = !isInteracted;
    }
}
