using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MentalCare_Test : InteractionThings
{
    public override void FirstInteractCheck()
    {
        return;
    }

    public override void Execute()
    {
        PlayerStatusManager.GetInstance().FearOut();
        PlayerStatusManager.GetInstance().SetFear(PlayerStatusManager.GetInstance().GetFear() - 50.0f);

        Destroy(gameObject);
    }
}
