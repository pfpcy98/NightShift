using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInLightCheck : MonoBehaviour
{
    private Transform m_transform = null;
    private PlayerStatus m_status = null;

    private List<Light> m_lightList = new List<Light>();

    private const string m_lightTag = "Light";
    private const string m_playerTag = "Player";

    private int m_monsterLayerMask = -1;
    private int m_sightLayerMask = -1;
    private int m_noiseLayerMask = -1;

    // Start is called before the first frame update
    private void Start()
    {
        m_transform = GetComponent<Transform>();
        m_status = GetComponent<PlayerStatus>();

        m_monsterLayerMask = LayerMask.NameToLayer("Monster");
        m_sightLayerMask = LayerMask.NameToLayer("Monster_Sight");
        m_noiseLayerMask = LayerMask.NameToLayer("Noise");
        if (m_monsterLayerMask == -1 ||
            m_sightLayerMask == -1 ||
            m_noiseLayerMask == -1)
        {
            Debug.LogError(name + ": Failed to load layermask!");
            Destroy(this);
        }

        Light[] lights = FindObjectsOfType<Light>();
        if(lights != null)
        {
            for(int i = 0; i < lights.Length; i++)
            {
                if (lights[i].CompareTag(m_lightTag))
                {
                    m_lightList.Add(lights[i]);
                }
            }
        }
    }

    private void Update()
    {
        if (m_lightList.Count > 0)
        {
            bool is_Exposured = false;

            for (int i = 0; i < m_lightList.Count; i++)
            {
                if((Vector3.Distance(transform.position, m_lightList[i].transform.position) <= m_lightList[i].range)
                    && m_lightList[i].enabled)
                {
                    RaycastHit hitInfo;
                    int layerMask = ~((1 << m_monsterLayerMask) | (1 << m_sightLayerMask) | (1 << m_noiseLayerMask));
                    Vector3 toDir = (m_transform.position - m_lightList[i].transform.position).normalized;

                    if (Physics.Raycast(m_lightList[i].transform.position, toDir, out hitInfo, m_lightList[i].range, layerMask))
                    {
                        if (hitInfo.transform.CompareTag(m_playerTag))
                        {
                            is_Exposured = true;
                        }
                    }
                }
            }

            m_status.SetLightExposured(is_Exposured);
        }
        else
        {
            m_status.SetLightExposured(false);
        }
    }
}
