using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.5.7 추가 스크립트 : 원숭이 투척
public class PlayerMonkeyThrow : MonoBehaviour
{
    private PlayerStatus m_status = null;
    private Vector3 throwPos = new Vector3(0.0f, 1.5f, 1.0f);

    [SerializeField]
    private GameObject monkeyPrefab = null;

    // Start is called before the first frame update
    void Start()
    {
        m_status = GetComponent<PlayerStatus>();
        if(m_status == null ||
            monkeyPrefab == null)
        {
            Debug.LogError("Player cannot throw monkeys!");
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(!m_status.IsCannotControl() &&
                m_status.monkeyRemainStack > 0)
            {
                m_status.DecreaseMonkeyStack();
                Rigidbody throwObject = Instantiate(monkeyPrefab, transform.position + throwPos, Quaternion.identity).GetComponent<Rigidbody>();
                throwObject.AddForce(Camera.main.transform.forward * m_status.monkeyThrowForce);
            }
        }
    }
}
