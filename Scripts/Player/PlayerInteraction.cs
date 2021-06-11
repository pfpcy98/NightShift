using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private AudioSource FXAudioSource; // 1.6.0 수정 : SerializeField 추가

    [SerializeField]
    private GameObject interaction_Image;
    [SerializeField]
    private Image press_Button;
    [SerializeField]
    private Image progress_Gauge;

    [SerializeField]
    private Sprite nonPress_Sprite = null;
    [SerializeField]
    private Sprite onPress_Sprite = null;

    private float progress_Time = 0.0f;
    private bool is_interactionStart = false;

    private Animator m_animator;

    private PlayerStatus player_Status;
    private InteractionThings interaction_Object;

    private void Start()
    {
        m_animator = GetComponent<Animator>();
        player_Status = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) &&
            interaction_Object != null &&
            !is_interactionStart)
        {
            is_interactionStart = true;
            if (player_Status.player_Condition != Player_Condition.Interact)
            {
                // 상호작용 상태 활성화
                player_Status.SetPlayerCondition(Player_Condition.Interact);

                if (onPress_Sprite != null)
                {
                    press_Button.sprite = onPress_Sprite;
                }

                m_animator.SetBool("is_Interact", true);
            }
        }

        // 상호작용 중일 때
        if (is_interactionStart &&
            Input.GetKey(KeyCode.Space))
        {
            if (interaction_Object != null)
            {
                // 플레이어가 상호작용 중이거나 조작 불가 상태가 아니고, 상호작용 물체가 재사용 가능하거나 사용한 적이 없을때만 로직을 수행함
                if ((!player_Status.IsCannotControl() || player_Status.player_Condition == Player_Condition.Interact) &&
                    (!interaction_Object.IsInteracted() || interaction_Object.IsReinteractable()))
                {
                    // 첫 접촉 이벤트 호출
                    interaction_Object.FirstInteractCheck();

                    // 즉시발동형인 경우 바로 상호작용 발동
                    if (interaction_Object.GetInteractionTime() <= 0)
                    {
                        progress_Gauge.fillAmount = 1.0f;

                        CompleteInteraction();
                    }
                    else
                    {
                        // 상호작용 게이지 상승
                        progress_Time += Time.deltaTime;

                        if ((progress_Time / interaction_Object.GetInteractionTime()) >= 1.0f)
                        {
                            progress_Gauge.fillAmount = 1.0f;
                        }
                        else
                        {
                            progress_Gauge.fillAmount = progress_Time / interaction_Object.GetInteractionTime();
                        }

                        // 게이지 완충 시 상호작용 발동
                        if (progress_Time >= interaction_Object.GetInteractionTime())
                        {
                            CompleteInteraction();
                        }
                    }
                }
            }
        }

        // 입력 취소 시 상호작용 초기화
        if(Input.GetKeyUp(KeyCode.Space) && player_Status.player_Condition == Player_Condition.Interact)
        {
            CancelInteraction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<InteractionThings>() != null)
        {
            interaction_Object = other.gameObject.GetComponent<InteractionThings>();

            if (interaction_Object.IsReinteractable() || !interaction_Object.IsInteracted())
            {
                interaction_Image.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<InteractionThings>() != null)
        {
            if (other.gameObject.GetComponent<InteractionThings>() == interaction_Object)
            {
                interaction_Image.SetActive(false);

                progress_Time = 0.0f;
                progress_Gauge.fillAmount = 0.0f;

                interaction_Object = null;
            }
        }
    }

    // 1.5.3 추가, 상호작용 완료 로직 중 공통 부분을 묶어놓는 함수
    private void CompleteInteraction()
    {
        if(FXAudioSource != null)
        {
            if(interaction_Object.GetCompleteSound() != null)
            {
                FXAudioSource.PlayOneShot(interaction_Object.GetCompleteSound());
            }
        }

        if (interaction_Object != null)
        {
            interaction_Object.Execute();
        }

        if (nonPress_Sprite != null)
        {
            press_Button.sprite = nonPress_Sprite;
        }
        progress_Gauge.fillAmount = 0.0f;
        progress_Time = 0.0f; // 1.6.1 수정 : 상호작용 완료 후 누적 시간이 초기화되지 않았던 현상
        interaction_Image.SetActive(false);

        // 1.5.5 추가
        m_animator.SetBool("is_Interact", false);

        // 1.6.0 수정 : 플레이어가 메시지를 읽는 경우 상태를 그대로 유지
        if (player_Status.player_Condition != Player_Condition.Message)
        {
            player_Status.SetPlayerCondition(Player_Condition.Idle);
        }

        // 1.6.0 수정 : 코드 추가1
        is_interactionStart = false;
        interaction_Object = null;
    }

    public void CancelInteraction()
    {
        is_interactionStart = false;

        progress_Time = 0.0f;
        progress_Gauge.fillAmount = 0.0f;

        if (nonPress_Sprite != null)
        {
            press_Button.sprite = nonPress_Sprite;
        }

        // 1.5.5 추가
        m_animator.SetBool("is_Interact", false);

        player_Status.SetPlayerCondition(Player_Condition.Idle);
    }
}
