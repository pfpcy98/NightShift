using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Player_Condition
{
    Idle,
    Move,
    Run,
    Dead,
    Interact,
    Hit,
    Message // 1.5.5 추가 변수, 쪽지 시스템 상태
}

public class PlayerStatus : MonoBehaviour
{
    // 1.5.5 변경 사항 : Insepctor 정리
    // Inspector 수정 가능 항목
    [Header("Player Stats")]
    [SerializeField]
    private int player_MaxHP = 100;

    [Space(10f)]
    [SerializeField]
    private float movement_Speed = 3.0f;

    [Space(10f)]
    [SerializeField]
    private float duration_Run = 5.0f;
    [SerializeField]
    private float cooldown_Run = 5.0f;

    [Space(10f)]
    [SerializeField]
    private float delay_walkSound = 3.0f;
    [SerializeField]
    private float delay_runSound = 3.0f;
    [SerializeField]
    private float delay_screamSound = 5.0f;
    [SerializeField]
    private float range_walkSound = 5.0f;
    [SerializeField]
    private float range_runSound = 15.0f;
    [SerializeField]
    private float range_screamSound = 15.0f;
    [SerializeField]
    private float duration_walkSound = 1.0f;
    [SerializeField]
    private float duration_runSound = 1.0f;
    [SerializeField]
    private float duration_screamSound = 1.0f;

    [Space(10f)]
    [SerializeField]
    private float duration_flashLight = 100.0f;

    [Space(10f)]
    [SerializeField]
    private float fearIncreaseAmount = 5.0f;
    [SerializeField]
    private float fearDecreaseAmount = 2.0f;

    [Space(10f)]
    // 1.6.0 추가
    [SerializeField]
    [Tooltip("이 시간동안은 추격중인 몬스터가 없더라도 추격 중 BGM을 재생합니다.")]
    private float chaseBGMMaintainTime = 3.0f;

    // 1.5.7 추가 변수 : 원숭이 아이템 투척 관련
    [Space(10f)]
    [SerializeField]
    private int monkeyMaxStack = 3;
    public int monkeyRemainStack = 1;
    public float monkeyThrowForce = 100; 

    // UI 연계 변수
    [Header("UI")]
    [SerializeField]
    private Image run_Gauge;

    [Space(10f)]
    [SerializeField]
    private Image flashlight_Gauge;
    [SerializeField]
    private Text flashlight_Text;

    [Space(10f)]
    [SerializeField]
    private Image health_Gauge;
    [SerializeField]
    private Text health_Text;

    [Space(10f)]
    [SerializeField]
    private GameObject deathScreen;
    [SerializeField]
    private Image deathScreenPanel;
    [SerializeField]
    private Text deathScreenText;

    [Space(10f)]
    [SerializeField]
    private Text monkey_baseText;
    private string count_bastText = "× ";

    // 1.5.5 추가 변수들 : 쪽지 시스템 관련
    [Space(10f)]
    [SerializeField]
    private GameObject messageUI;
    [SerializeField]
    private Image messageImage;
    [SerializeField]
    private Text messageText;

    [Space(10f)]
    [SerializeField]
    private Text warningText;

    [Space(10f)]
    [SerializeField]
    private Image fadeOutPanel;
    [SerializeField]
    private Image fear_Gauge;

    private const float fadeInMaxCount = 30;

    // 기타 상태 관련 변수
    public bool is_Flashlight_On { get; private set; } = false;
    public bool is_Run { get; private set; } = false;
    public bool is_Run_Cooldown { get; private set; } = false;
    public float battery_count { get; private set; } = 1;
    public int player_HP { get; private set; }
    private float remain_Duration_Run;
    private float remain_cooldown_Run;
    private float remain_delay_WalkSound;
    private float remain_delay_RunSound;
    public float remain_Duration_Flashlight { get; private set; }

    // 1.6.0 추가 : 추격 상태 BGM에서 일반 상태 BGM으로 전환까지 남은 시간
    private float chaseBGMRemainTime;

    public Player_Condition player_Condition { get; private set; } = Player_Condition.Idle;

    private bool is_exposuredByLight = false;

    //* 공포 관련 변수
    public float fearAmount { get; private set; } = 0.0f;
    private bool is_InFear = false;

    private float fearIncreaseDelay = 1.0f;
    private float fearDecreaseDelay = 1.0f;
    private float remain_Fear_Increase_Delay;
    private float remain_Fear_Decrease_Delay;

    private float remain_Delay_Scream;

    // 1.6.0 추가 : 추격 중인 몬스터 체크 변수
    private int chasingMobCount = 0;

    // 연계 컴포넌트 관련 변수
    private Animator m_animator = null;
    private PlayerInteraction m_interaction = null;
    private PlayerSoundController m_soundController = null;

    private void Start()
    {
        m_animator = GetComponent<Animator>();
        m_interaction = GetComponent<PlayerInteraction>();
        m_soundController = GetComponent<PlayerSoundController>();

        remain_Duration_Run = duration_Run;
        remain_cooldown_Run = cooldown_Run;
        remain_delay_WalkSound = 0.0f;
        remain_delay_RunSound = 0.0f;
        remain_Duration_Flashlight = duration_flashLight;
        flashlight_Text.text = count_bastText + battery_count;
        monkey_baseText.text = count_bastText + monkeyRemainStack;
        player_HP = player_MaxHP;

        remain_Delay_Scream = 0.0f;
        remain_Fear_Decrease_Delay = fearIncreaseDelay;
        remain_Fear_Decrease_Delay = fearDecreaseDelay;

        // 1.6.0 추가 : BGM 재생 시작
        m_soundController.StartBGMSound(PlayerBGMIndex.Normal, true);

        chaseBGMRemainTime = chaseBGMMaintainTime;
    }

    private void Update()
    {
        // 달리기 쿨다운 적용
        if (is_Run_Cooldown)
        {
            remain_cooldown_Run -= Time.deltaTime;
            if (remain_cooldown_Run <= 0)
            {
                is_Run_Cooldown = false;
                remain_Duration_Run = duration_Run;
                remain_cooldown_Run = cooldown_Run;

                run_Gauge.fillAmount = 1.0f;
                run_Gauge.enabled = true;
            }
        }

        // 달리기 지속중일 경우 지속시간 적용
        if (is_Run)
        {
            run_Gauge.enabled = false;
            remain_Duration_Run -= Time.deltaTime;

            // 지속시간이 0 이하가 되거나 이동상태가 아닐 경우 달리기 해제
            if (remain_Duration_Run <= 0 || player_Condition != Player_Condition.Move)
            {
                CancelRun();
                remain_Duration_Run = 0;
            }
        }

        // 이동 중 소음 발생
        if (player_Condition == Player_Condition.Move)
        {
            if (is_Run)
            {
                remain_delay_WalkSound = 0.0f;
                remain_delay_RunSound -= Time.deltaTime;

                if (remain_delay_RunSound <= 0)
                {
                    NoiseManager.GetInstance().CreatePlayerNoise(range_runSound, duration_runSound);
                    remain_delay_RunSound = delay_runSound;
                }
            }
            else
            {
                remain_delay_RunSound = 0.0f;
                remain_delay_WalkSound -= Time.deltaTime;

                if (remain_delay_WalkSound <= 0)
                {
                    NoiseManager.GetInstance().CreatePlayerNoise(range_walkSound, duration_walkSound);
                    remain_delay_WalkSound = delay_walkSound;
                }
            }
        }
        else
        {
            remain_delay_RunSound = 0.0f;
            remain_delay_WalkSound = 0.0f;
        }

        // 손전등 지속시간 적용
        if (is_Flashlight_On)
        {
            remain_Duration_Flashlight -= Time.deltaTime;

            if (remain_Duration_Flashlight <= 0)
            {
                if (battery_count <= 0)
                {
                    is_Flashlight_On = false;
                }
                else
                {
                    battery_count--;
                    remain_Duration_Flashlight = duration_flashLight;
                }
            }

            flashlight_Text.text = count_bastText + battery_count;
            flashlight_Gauge.fillAmount = remain_Duration_Flashlight / duration_flashLight;
        }

        // 공포 체크
        //* 조건에 따라 공포 게이지 상승 및 저하
        //* 1.6.0 변경 : 추격 중인 몬스터 카운트 체크 추가
        if ((!is_exposuredByLight &&
            !is_Flashlight_On) ||
            chasingMobCount > 0)
        {
            remain_Fear_Decrease_Delay = fearDecreaseDelay;
            remain_Fear_Increase_Delay -= Time.deltaTime;

            if (remain_Fear_Increase_Delay <= 0.0f &&
                fearAmount < 100.0f)
            {
                fearAmount += fearIncreaseAmount;
                remain_Fear_Increase_Delay = fearIncreaseDelay;
            }

            if (fearAmount >= 100.0f)
            {
                fearAmount = 100.0f;
                is_InFear = true;
            }
        }
        else
        {
            remain_Fear_Increase_Delay = fearIncreaseDelay;

            if (!is_InFear)
            {
                remain_Fear_Decrease_Delay -= Time.deltaTime;

                if (remain_Fear_Decrease_Delay <= 0.0f &&
                    fearAmount > 0.0f)
                {
                    fearAmount -= fearDecreaseAmount;
                    remain_Fear_Decrease_Delay = fearDecreaseDelay;
                }

                if (fearAmount < 0.0f)
                {
                    fearAmount = 0.0f;
                }
            }
        }
        if (fear_Gauge != null)
        {
            fear_Gauge.fillAmount = fearAmount / 100.0f;
        }

        //* 공포 상태가 되었을 경우 주기마다 비명 생성
        if (is_InFear)
        {
            remain_Delay_Scream -= Time.deltaTime;

            if (remain_Delay_Scream <= 0.0f)
            {
                m_soundController.StartFXSound(PlayerFXIndex.FearShout);
                NoiseManager.GetInstance().CreatePlayerNoise(range_screamSound, duration_screamSound);
                remain_Delay_Scream = delay_screamSound;
            }
        }
        else
        {
            remain_Delay_Scream = 0.0f;
        }

        // 1.6.0 추가 : 추격 중인 몬스터 갯수에 따라 BGM 변경
        if(chasingMobCount > 0 &&
            !m_soundController.IsPlayingChased())
        {
            chaseBGMRemainTime = chaseBGMMaintainTime;
            m_soundController.StartBGMSound(PlayerBGMIndex.Chased);
        }

        if(chasingMobCount <= 0 &&
            m_soundController.IsPlayingChased())
        {
            chaseBGMRemainTime -= Time.deltaTime;

            if (chaseBGMRemainTime <= 0.0f)
            {
                chaseBGMRemainTime = chaseBGMMaintainTime;
                m_soundController.StartBGMSound(PlayerBGMIndex.Normal);
            }
        }
    }

    public int GetPlayerHP()
    {
        return player_HP;
    }

    public void Run()
    {
        is_Run = true;
    }

    public void CancelRun()
    {
        is_Run = false;
        is_Run_Cooldown = true;
    }

    public float GetMovementSpeed()
    {
        return movement_Speed;
    }

    public bool IsCannotControl()
    {
        if (player_Condition == Player_Condition.Dead ||
            player_Condition == Player_Condition.Hit ||
            player_Condition == Player_Condition.Interact ||
            player_Condition == Player_Condition.Message)
        {
            return true;
        }

        return false;
    }

    public void SetPlayerCondition(Player_Condition condition)
    {
        player_Condition = condition;
    }

    public void SetLightExposured(bool value)
    {
        is_exposuredByLight = value;
    }

    public bool IsLightExposured()

    {
        return is_exposuredByLight;
    }

    public void SetFlashlightStatus(bool value)
    {
        is_Flashlight_On = value;
    }

    public void IncreaseBattery()
    {
        battery_count++;
        flashlight_Text.text = count_bastText + battery_count;
    }

    public int GetMaxHP()
    {
        return player_MaxHP;
    }

    public void SetHP(int value)
    {
        if (value < player_HP)
        {
            m_soundController.StartFXSound(PlayerFXIndex.GetDamage);
            if (player_Condition == Player_Condition.Interact)
            {
                m_interaction.CancelInteraction();
            }
        }

        // 1.6.1 추가 : 체력의 -화 방지
        if (value < 0)
        {
            value = 0;
        }

        player_HP = value;

        health_Gauge.fillAmount = (float)player_HP / (float)player_MaxHP;
        health_Text.text = player_HP.ToString();

        if (player_HP <= 0 && player_Condition != Player_Condition.Dead)
        {
            // 사망 처리
            player_Condition = Player_Condition.Dead;
            m_animator.SetTrigger("is_Dead");
            StartCoroutine(DeathScreenFadeIn());
        }
    }

    public void FearOut()
    {
        is_InFear = false;
    }
    public void SetFear(float value)
    {
        fearAmount = value;
        if (fearAmount > 100.0f)
        {
            fearAmount = 100.0f;
            is_InFear = true;
        }
        else if (fearAmount < 0.0f)
        {
            fearAmount = 0.0f;
        }
    }

    private IEnumerator DeathScreenFadeIn()
    {
        deathScreen.SetActive(true);

        float fadeInCount = 0;
        bool is_work_done = false;

        while (!is_work_done)
        {
            deathScreenPanel.color = new Color(deathScreenPanel.color.r, deathScreenPanel.color.g, deathScreenPanel.color.b, 0.66f * (fadeInCount / fadeInMaxCount));
            deathScreenText.color = new Color(deathScreenPanel.color.r, deathScreenPanel.color.g, deathScreenPanel.color.b, (fadeInCount / fadeInMaxCount));

            if (fadeInCount >= fadeInMaxCount)
            {
                is_work_done = true;
            }

            fadeInCount++;

            yield return new WaitForSeconds(0.1f);
        }

        // 1.6.1 추가 : 버튼 삭제, 페이드인 완료 후 이동
        SceneLoadManager.GetInstance().LoadScene("MainMenu");
    }

    public void ShowWarningText(string text)
    {
        warningText.text = text;
        StartCoroutine(HideWarningText());
    }

    private IEnumerator HideWarningText()
    {
        yield return new WaitForSeconds(2.0f);

        warningText.text = string.Empty;
    }

    public void StageClear(string nextStage)
    {
        StartCoroutine(StageFadeOut(nextStage));
    }

    private IEnumerator StageFadeOut(string nextStage)
    {
        fadeOutPanel.gameObject.SetActive(true);

        float fadeInCount = 0;
        bool is_work_done = false;

        while (!is_work_done)
        {
            fadeOutPanel.color = new Color(fadeOutPanel.color.r, fadeOutPanel.color.g, fadeOutPanel.color.b, (fadeInCount / fadeInMaxCount));

            if (fadeInCount >= fadeInMaxCount)
            {
                is_work_done = true;
            }

            fadeInCount++;

            yield return new WaitForSeconds(0.1f);
        }

        SceneLoadManager.GetInstance().LoadScene(nextStage);
    }

    // 1.5.5 추가 함수 : 쪽지 UI 활성화
    public void PopMessage(string text, Sprite image)
    {
        if (messageText != null &&
            messageImage != null)
        {
            messageImage.sprite = image;
            messageText.text = text;

            if (messageUI != null)
            {
                messageUI.SetActive(true);
                player_Condition = Player_Condition.Message;
            }
        }
    }

    // 1.5.5 추가 함수 : 쪽지 UI 비활성화
    public void FadeMessage()
    {
        if (messageUI != null)
        {
            messageUI.SetActive(false);
            player_Condition = Player_Condition.Idle;
        }
    }

    // 1.5.7 추가 함수 : 원숭이 스택 증가 / 감소 함수
    public bool IncreaseMonkeyStack()
    {
        if(monkeyRemainStack + 1 > monkeyMaxStack)
        {
            return false;
        }

        monkeyRemainStack++;
        monkey_baseText.text = count_bastText + monkeyRemainStack;
        return true;
    }

    public void DecreaseMonkeyStack()
    {
        monkeyRemainStack--;
        monkey_baseText.text = count_bastText + monkeyRemainStack;
    }

    // 1.6.0 추가 함수 : 추격 몬스터 스택 증가 / 감소 함수
    public void IncreaseChasedStack()
    {
        chasingMobCount++;
    }

    public void DecreaseChasedStack()
    {
        if(chasingMobCount > 0)
        {
            chasingMobCount--;
        }
    }
}
