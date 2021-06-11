using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewThroughWall : MonoBehaviour
{
    [SerializeField] private Transform tr_Camera;
    [SerializeField] private Transform tr_Player;

    private Vector3 heading;
    private float distance;
    private Vector3 direction;

    private RaycastHit[] raycastHits;
    private List<MeshRenderer> betweenWalls;
    private bool is_StillBlocks = false;
    private Color color;

    void Start()
    {
        betweenWalls = new List<MeshRenderer>();
    }

    void Update()
    {
        heading = tr_Player.position - tr_Camera.position;
        distance = Vector3.Magnitude(heading);
        direction = Vector3.Normalize(heading);

        // 카메라와 플레이어 사이에 벽이 있으면 해당 벽을 리스트에 추가
        raycastHits = Physics.RaycastAll(tr_Camera.position, direction, distance);
        foreach(RaycastHit hit in raycastHits)
        {
            if(hit.transform.tag == "Wall")
            {
                if(!betweenWalls.Contains(hit.transform.GetComponent<MeshRenderer>()) &&
                    hit.transform.GetComponent<MeshRenderer>() != null)
                {
                    betweenWalls.Add(hit.transform.GetComponent<MeshRenderer>());
                }
            }
        }
        
        for(int i = 0; i < betweenWalls.Count; i++)
        {   
            // 리스트에 있던 벽이 더 이상 시야를 가로막지 않으면 투명도를 되돌리고 리스트에서 제거
            is_StillBlocks = false;
            foreach(RaycastHit hit in raycastHits)
            {
                if(betweenWalls[i] == hit.transform.GetComponent<MeshRenderer>()) { is_StillBlocks = true; }
            }

            if(!is_StillBlocks) 
            {
                betweenWalls[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                betweenWalls.RemoveAt(i);
            }
            else 
            {
                betweenWalls[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
