using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    private static NoiseManager m_instance = null;

    [SerializeField]
    private GameObject prefab_Noise = null;

    [SerializeField]
    private GameObject m_player = null;
    private Transform tr_player = null;

    private GameObject noise_Player = null;
    private GameObject noise_Object = null;

    // Start is called before the first frame update
    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }

        m_player = GameObject.FindGameObjectWithTag("Player");

        if (m_player == null)
        {
            Debug.LogError(name + ": This manager doesn't have player data!");
        }
        else
        {
            tr_player = m_player.GetComponent<Transform>();

            if (tr_player == null)
            {
                Debug.LogError(name + ": Failed to load player transform data!");
            }
        }
    }

    public static NoiseManager GetInstance()
    {
        if (m_instance == null)
        {
            return null;
        }

        return m_instance;
    }

    public void CreatePlayerNoise(float range, float duration)
    {
        if(noise_Player != null)
        {
            Destroy(noise_Player);
        }

        if(prefab_Noise != null)
        {
            noise_Player = Instantiate(prefab_Noise, tr_player.position, Quaternion.identity);
            Noise noise = noise_Player.GetComponent<Noise>();

            if(noise != null)
            {
                noise.SetRange(range);
                noise.SetCount(duration);
            }
            else
            {
                Destroy(noise_Player);
            }
        }
    }

    public void CreateObjectNoise(Vector3 pos, float range, float duration)
    {
        if(noise_Object != null)
        {
            Destroy(noise_Object);
        }

        if(prefab_Noise != null)
        {
            noise_Object = Instantiate(prefab_Noise, pos, Quaternion.identity);
            Noise noise = noise_Object.GetComponent<Noise>();

            if(noise != null)
            {
                noise.SetRange(range);
                noise.SetCount(duration);
            }
            else
            {
                Destroy(noise_Object);
            }
        }
    }
}
