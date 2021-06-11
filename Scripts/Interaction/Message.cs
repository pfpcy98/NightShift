using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message : InteractionThings
{
    [Header ("Message")]
    [SerializeField]
    private Sprite messageImage;
    [SerializeField]
    private string messageText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void FirstInteractCheck()
    {

    }

    public override void Execute()
    {
        if(messageImage != null)
        {
            isFirstInteracted = false;
            isInteracted = true;
            PlayerStatusManager.GetInstance().PopMessage(messageText, messageImage);
        }
    }
}
