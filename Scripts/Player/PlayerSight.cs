using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSight : MonoBehaviour
{
    [SerializeField] private Transform tr_Player;
    [SerializeField] private Transform tr_Light;

    // Update is called once per frame
    void Update()
    {
        tr_Light.position = new Vector3(tr_Player.position.x, tr_Light.position.y, tr_Player.position.z);
    }
}
