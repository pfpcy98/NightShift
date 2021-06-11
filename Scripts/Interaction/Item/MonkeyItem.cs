using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyItem : InteractionThings
{
    public override void FirstInteractCheck()
    {
        return;
    }

    public override void Execute()
    {
        // 플레이어의 원숭이 값 1 증가 시도
        if (PlayerStatusManager.GetInstance().IncreaseMonkeyStack())
        {
            // 성공하면 이 오브젝트를 삭제
            Destroy(gameObject);
        }
        else
        {
            interactionTime = 0.0f;
        }
    }
}
