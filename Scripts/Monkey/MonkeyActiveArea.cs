using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.5.7 ���� ��ũ��Ʈ : ������ �۵����� ��ũ��Ʈ
public class MonkeyActiveArea : MonoBehaviour
{
    private MonkeyActivation monkeyActivation = null;
    private SphereCollider m_collider = null;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<SphereCollider>();
        monkeyActivation = GetComponentInParent<MonkeyActivation>();

        m_collider.radius = monkeyActivation.monkeyActiveRange;
    }


    private void OnTriggerEnter(Collider other)
    {
        EyeTypeMonster eye = other.GetComponent<EyeTypeMonster>();
        EarTypeMonster ear = other.GetComponent<EarTypeMonster>();

        if(eye != null)
        {
            monkeyActivation.SetPossessedMonster(other.gameObject);
        }

        if(ear != null)
        {
            monkeyActivation.SetPossessedMonster(other.gameObject);
        }
    }
}
