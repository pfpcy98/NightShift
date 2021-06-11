using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EndingSceneEnder : MonoBehaviour
{
    private const string mainMenuSceneName = "MainMenu";

    private void OnEnable()
    {
        SceneLoadManager.GetInstance().LoadScene(mainMenuSceneName);
    }
}
