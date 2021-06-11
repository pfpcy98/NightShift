using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackJudge : MonoBehaviour
{
    private string m_playerTag = "Player";
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(m_playerTag))
        {
            
        }
    }
}
