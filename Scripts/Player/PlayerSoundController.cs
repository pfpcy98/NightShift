using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.6.0 변경 : 열거형 이름 변경(PlayerSoundIndex -> PlayerFXIndex), 사운드 이원화를 위함
public enum PlayerFXIndex
{
    Walk,
    GetDamage,
    Interact,
    FlashLight,
    FearShout,
}

// 1.6.0 추가 : 사운드 이원화
public enum PlayerBGMIndex
{
    Normal,
    Chased,
}

public class PlayerSoundController : MonoBehaviour
{
    // 1.6.0 변경 : 변수 이름 변경(m_audioSource -> m_FXAudioSource)
    [SerializeField]
    private AudioSource m_FXAudioSource = null;
    // 1.6.0 추가
    [SerializeField]
    private AudioSource m_BGMAudioSource = null;

    // 1.6.0 추가
    [Header("BGM")]
    [SerializeField]
    private AudioClip normalBGM = null;
    [SerializeField]
    private AudioClip chasedBGM = null;

    [Header("FX")]
    [SerializeField]
    private AudioClip[] walkSound = null;
    public int walkSoundIndex = 0;

    [Space(10f)]
    [SerializeField]
    private AudioClip getDamageSound = null;
    [SerializeField]
    private AudioClip interactSound = null;
    [SerializeField]
    private AudioClip flashLightSound = null;
    // 1.6.0 추가
    [SerializeField]
    private AudioClip fearShoutSound = null;

    // 1.6.0 추가
    private bool is_BGM_Changing = false;

    private void Start()
    {
        if(m_FXAudioSource == null ||
            m_BGMAudioSource == null)
        {
            Debug.LogError(name + " : Player cannont play own sounds!");
        }
    }

    // 1.6.0 변경 사항 : 함수 이름 변경(StartSound -> StartFXSound)
    public void StartFXSound(PlayerFXIndex index)
    {
        if(m_FXAudioSource == null)
        {
            return;
        }

        switch (index)
        {
            case PlayerFXIndex.Walk:
                if(walkSound != null)
                {
                    m_FXAudioSource.PlayOneShot(walkSound[walkSoundIndex]);
                    if(walkSoundIndex + 1 >= walkSound.Length)
                    {
                        walkSoundIndex = 0;
                    }
                    else
                    {
                        walkSoundIndex++;
                    }
                }
                break;

            case PlayerFXIndex.GetDamage:
                if(getDamageSound != null)
                {
                    m_FXAudioSource.PlayOneShot(getDamageSound);
                }
                break;

            case PlayerFXIndex.Interact:
                if(interactSound != null)
                {
                    m_FXAudioSource.PlayOneShot(interactSound);
                }
                break;

            case PlayerFXIndex.FlashLight:
                if(flashLightSound != null)
                {
                    m_FXAudioSource.PlayOneShot(flashLightSound);
                }
                break;

            case PlayerFXIndex.FearShout:
                if(fearShoutSound != null)
                {
                    m_FXAudioSource.PlayOneShot(fearShoutSound);
                }
                break;

            default:
                break;
        }
    }

    // 1.6.0 추가 : BGM 재생 함수
    public void StartBGMSound(PlayerBGMIndex index, bool playImmediately = false)
    {
        if(m_BGMAudioSource == null)
        {
            return;
        }

        if (playImmediately)
        {
            SetBGM(index);
        }
        else
        {
            StartCoroutine(BGMChange(index));
        }

        if(!m_BGMAudioSource.isPlaying)
        {
            m_BGMAudioSource.Play();
        }
    }

    // 1.6.0 추가 : BGM 페이드인-아웃 코루틴
    private IEnumerator BGMChange(PlayerBGMIndex index)
    {
        if (!is_BGM_Changing)
        {
            is_BGM_Changing = true;
            while (m_BGMAudioSource.volume > 0.0f)
            {
                m_BGMAudioSource.volume -= 0.01f;
                yield return new WaitForSeconds(0.02f);
            }

            m_BGMAudioSource.volume = 0.0f;
            SetBGM(index);

            while (m_BGMAudioSource.volume < 1.0f)
            {
                m_BGMAudioSource.volume += 0.01f;
                yield return new WaitForSeconds(0.02f);
            }

            m_BGMAudioSource.volume = 1.0f;
            is_BGM_Changing = false;
        }
    }

    // 1.6.0 추가 : BGM 설정 분기가 갈림에 따라 코드 일원화
    private void SetBGM(PlayerBGMIndex index)
    {
        switch (index)
        {
            case PlayerBGMIndex.Normal:
                if (normalBGM != null)
                {
                    m_BGMAudioSource.clip = normalBGM;
                }
                break;

            case PlayerBGMIndex.Chased:
                if (chasedBGM != null)
                {
                    m_BGMAudioSource.clip = chasedBGM;
                }
                break;

            default:
                break;
        }

        m_BGMAudioSource.Play();
    }

    // 1.6.0 추가 : 추격 상태 BGM 재생중인지 반환
    public bool IsPlayingChased()
    {
        return (m_BGMAudioSource.clip == chasedBGM) && m_BGMAudioSource.isPlaying;
    }
}
