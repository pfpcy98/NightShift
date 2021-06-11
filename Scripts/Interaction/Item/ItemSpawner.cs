using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] spawnItemList = null;

    // Start is called before the first frame update
    void Start()
    {
        if(spawnItemList == null)
        {
            Debug.Log(name + " : this ItemSpawner doesn't have any spawnable items!");
            Destroy(gameObject);
        }
        else
        {
            int index = Random.Range(0, spawnItemList.Length);
            Instantiate(spawnItemList[index], transform);
        }
    }
}
