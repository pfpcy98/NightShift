using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : InteractionThings
{
    public override void FirstInteractCheck()
    {
        return;
    }

    public override void Execute()
    {
        // 플레이어의 배터리 값을 1 증가
        PlayerStatusManager.GetInstance().IncreaseBattery();

        // 이후 이 오브젝트를 삭제
        Destroy(gameObject);
    }
}
