using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusManager : MonoBehaviour
{
    private static PlayerStatusManager m_instance = null;

    [SerializeField]
    private GameObject m_player = null;
    private Transform tr_player = null;
    private PlayerStatus status_player = null;

    private void Awake()
    {
        if(m_instance == null)
        {
            m_instance = this;
        }

        m_player = GameObject.FindGameObjectWithTag("Player");

        if(m_player == null)
        {
            Debug.LogError(name + ": This manager doesn't have player data!");
        }
        else
        {
            tr_player = m_player.GetComponent<Transform>();
            status_player = m_player.GetComponent<PlayerStatus>();

            if(tr_player == null)
            {
                Debug.LogError(name + ": Failed to load player transform data!");
            }

            if (status_player == null)
            {
                Debug.LogError(name + ": Failed to load player status data!");
            }
        }
    }

    public static PlayerStatusManager GetInstance()
    {
        if(m_instance == null)
        {
            return null;
        }

        return m_instance;
    }

    public Vector3 GetPlayerPosition()
    {
        if(tr_player == null)
        {
            return Vector3.zero;
        }

        return tr_player.position;
    }

    public Vector3 GetPlayerForward()
    {
        if(tr_player == null)
        {
            return Vector3.zero;
        }

        return tr_player.forward;
    }

    public bool IsPlayerInLight()
    {
        return status_player.IsLightExposured() || status_player.is_Flashlight_On;
    }

    public void IncreaseBattery()
    {
        status_player.IncreaseBattery();
    }

    public int GetMaxHP()
    {
        return status_player.GetMaxHP();
    }

    public int GetHP()
    {
        return status_player.player_HP;
    }

    public void SetHP(int value)
    {
        status_player.SetHP(value);
    }

    public float GetFear()
    {
        return status_player.fearAmount;
    }

    public void SetFear(float value)
    {
        status_player.SetFear(value);
    }

    public void FearOut()
    {
        status_player.FearOut();
    }

    public void ShowWarningText(string text)
    {
        status_player.ShowWarningText(text);
    }

    public void StageClear(string nextStage)
    {
        status_player.StageClear(nextStage);
    }

    // 1.5.5 추가
    public void PopMessage(string text, Sprite image)
    {
        status_player.PopMessage(text, image);
    }

    // 1.5.5 추가
    public void FadeMessage()
    {
        status_player.FadeMessage();
    }

    // 1.5.7 추가
    public bool IncreaseMonkeyStack()
    {
        return status_player.IncreaseMonkeyStack();
    }

    // 1.6.0 추가
    public void IncreaseChasedStack()
    {
        status_player.IncreaseChasedStack();
    }

    // 1.6.0 추가
    public void DecreaseChasedStack()
    {
        status_player.DecreaseChasedStack();
    }
}
