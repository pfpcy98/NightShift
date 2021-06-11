using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.6.0 �ۼ� : ����ŷ �������ɾ� ��ũ��Ʈ
public class Jumpscare_Mannequin : MonoBehaviour
{
    [SerializeField]
    [Tooltip("�������ɾ� �ߵ� �� ���� ���� �����Դϴ�.")]
    private AudioClip jumpscareSound = null;

    private const string playerTag = "Player";
    private AudioSource m_audioSource = null;

    private bool is_activated = false;

    // Start is called before the first frame update
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!is_activated)
        {
            if (other.gameObject.CompareTag(playerTag))
            {
                is_activated = true;

                if (m_audioSource != null &&
                    jumpscareSound != null)
                {
                    m_audioSource.PlayOneShot(jumpscareSound);
                }

                Vector3 newPos = PlayerStatusManager.GetInstance().GetPlayerPosition();
                newPos += PlayerStatusManager.GetInstance().GetPlayerForward();
                newPos.y += 1.75f;

                transform.position = newPos;
                transform.forward = -(PlayerStatusManager.GetInstance().GetPlayerForward());
            }
        }
    }
}
