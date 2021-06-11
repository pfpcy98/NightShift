using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.6.1 작성 스크립트 : 엔딩 씬 스킵용 스크립트
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
