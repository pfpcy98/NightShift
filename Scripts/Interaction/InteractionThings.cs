using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractionThings : MonoBehaviour
{
    [Header ("Interaction")]
    // 상호작용 완료에 드는 시간
    [SerializeField]
    protected float interactionTime = 0.0f;
    // 상호작용을 다시 할 수 있는지에 대한 여부
    [SerializeField]
    private bool isReinteractable = false;
    // 1.5.3 추가, 상호작용 완료 사운드
    [SerializeField]
    private AudioClip completeSound = null;

    protected static bool isFirstInteracted = true;
    public bool isInteracted { get; protected set; } = false;

    public abstract void FirstInteractCheck();
    public abstract void Execute();

    public float GetInteractionTime()
    {
        return interactionTime;
    }
    public bool IsReinteractable()
    {
        return isReinteractable;
    }
    public bool IsInteracted()
    {
        return isInteracted;
    }

    // 1.5.3 추가, 상호작용 완료 사운드를 반환
    public AudioClip GetCompleteSound()
    {
        if(completeSound != null)
        {
            return completeSound;
        }

        return null;
    }
}
