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
        // �÷��̾��� ������ �� 1 ���� �õ�
        if (PlayerStatusManager.GetInstance().IncreaseMonkeyStack())
        {
            // �����ϸ� �� ������Ʈ�� ����
            Destroy(gameObject);
        }
        else
        {
            interactionTime = 0.0f;
        }
    }
}
