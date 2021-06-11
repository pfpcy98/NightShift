using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour
{
    private SphereCollider sphereCollider = null;
    private float remain_Time = 0.0f;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();

        if(sphereCollider == null)
        {
            Debug.LogError(name + " : This noise object can't take own collider!");
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        remain_Time -= Time.deltaTime;

        if(remain_Time <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetCount(float time)
    {
        remain_Time = time;
    }

    public void SetRange(float range)
    {
        if(sphereCollider != null)
        {
            sphereCollider.radius = range;
        }
    }
}
