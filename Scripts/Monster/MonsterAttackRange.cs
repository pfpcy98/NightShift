using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.5.7 변경 사항 : 원숭이로 인해 변수 및 OnTriggerEnter/Exit 함수 구조 변경(유혹 상태 체크)
public class MonsterAttackRange : MonoBehaviour
{
    private EyeTypeMonster m_eye = null; // 1.5.7 추가 변수 : Runner 스크립트
    private EarTypeMonster m_ear = null; // 1.5.7 추가 변수 : Hunter 스크립트
    private string m_playerTag = "Player";
    private string m_monkeyTag = "Monkey"; // 1.5.7 추가 변수 : 원숭이 태그
    public bool is_playerInRange = false;

    private void Start()
    {
        m_eye = GetComponentInParent<EyeTypeMonster>();
        m_ear = GetComponentInParent<EarTypeMonster>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(m_ear != null)
        {
            if(m_ear.m_status != AgentStatus.Possessed)
            {
                if (other.CompareTag(m_playerTag))
                {
                    is_playerInRange = true;
                }
            }
            else
            {
                if(other.CompareTag(m_monkeyTag))
                {
                    is_playerInRange = true;
                }
            }
        }

        if(m_eye != null)
        {
            if (m_eye.m_status != AgentStatus.Possessed)
            {
                if (other.CompareTag(m_playerTag))
                {
                    is_playerInRange = true;
                }
            }
            else
            {
                if (other.CompareTag(m_monkeyTag))
                {
                    is_playerInRange = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_ear != null)
        {
            if (m_ear.m_status != AgentStatus.Possessed)
            {
                if (other.CompareTag(m_playerTag))
                {
                    is_playerInRange = false;
                }
            }
            else
            {
                if (other.CompareTag(m_monkeyTag))
                {
                    is_playerInRange = false;
                }
            }
        }

        if (m_eye != null)
        {
            if (m_eye.m_status != AgentStatus.Possessed)
            {
                if (other.CompareTag(m_playerTag))
                {
                    is_playerInRange = false;
                }
            }
            else
            {
                if (other.CompareTag(m_monkeyTag))
                {
                    is_playerInRange = false;
                }
            }
        }
    }
}
