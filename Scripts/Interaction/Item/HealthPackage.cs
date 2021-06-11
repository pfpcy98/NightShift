using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPackage : InteractionThings
{
    public override void FirstInteractCheck()
    {
        return;
    }

    public override void Execute()
    {
        // 전체 체력의 1/2만큼 회복
        if(PlayerStatusManager.GetInstance().GetHP() + (PlayerStatusManager.GetInstance().GetMaxHP() / 2) >= PlayerStatusManager.GetInstance().GetMaxHP())
        {
            PlayerStatusManager.GetInstance().SetHP(PlayerStatusManager.GetInstance().GetMaxHP());
        }
        else
        {
            PlayerStatusManager.GetInstance().SetHP(PlayerStatusManager.GetInstance().GetHP() + (PlayerStatusManager.GetInstance().GetMaxHP() / 2));
        }

        // 이후 이 게임 오브젝트를 삭제(1회용)
        Destroy(gameObject);
    }
}
