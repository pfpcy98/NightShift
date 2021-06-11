using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.6.1 �ۼ� ��ũ��Ʈ : ���� �� ��ŵ�� ��ũ��Ʈ
public class EndingSceneSkipper : MonoBehaviour
{
    private const string mainMenuSceneName = "MainMenu";

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneLoadManager.GetInstance().LoadScene(mainMenuSceneName);
        }
    }
}
