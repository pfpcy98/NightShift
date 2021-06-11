using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallAnswer : InteractionThings
{
    [SerializeField]
    private AudioClip callAnswerSound = null;

    private AudioSource m_audioSource = null;
    private Jumpscare_Phone m_motherObject = null;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_motherObject = GetComponentInParent<Jumpscare_Phone>();   
    }

    public override void FirstInteractCheck()
    {
        return;
    }

    public override void Execute()
    {
        if (!isInteracted)
        {
            isInteracted = true;

            if (m_motherObject != null)
            {
                m_motherObject.CallAnswered();
            }

            if (m_audioSource != null &&
                callAnswerSound != null)
            {
                m_audioSource.PlayOneShot(callAnswerSound);
            }
        }
    }
}
