using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlashLight : MonoBehaviour
{
    [SerializeField]
    private Light flashlight = null;

    private PlayerStatus m_status = null;
    private PlayerSoundController m_soundController = null;

    void Awake()
    {
        m_status = GetComponent<PlayerStatus>();
        if(m_status == null)
        {
            Debug.Log(name + " : Cannot find player status!");
        }

        m_soundController = GetComponent<PlayerSoundController>();
        if(m_soundController == null)
        {
            Debug.Log(name + " : Cannot find player sound controller!");
        }

        if(flashlight == null)
        {
            Debug.LogError(name + " : Cannot find flashlight obejct!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 1.4.1 변경 사항 : 발동 키 왼쪽 Ctrl키 -> 마우스 좌클릭
        if(Input.GetMouseButtonDown(0))
        {
            if (!m_status.IsCannotControl())
            {
                if (m_status.battery_count > 0
                    || (m_status.battery_count == 0 && m_status.remain_Duration_Flashlight > 0))
                {
                    m_status.SetFlashlightStatus(!m_status.is_Flashlight_On);
                    m_soundController.StartFXSound(PlayerFXIndex.FlashLight);
                }
            }
        }

        flashlight.enabled = m_status.is_Flashlight_On;
    }
}
