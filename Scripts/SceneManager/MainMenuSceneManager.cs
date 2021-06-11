using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 1.6.0 스크립트 : 메인 메뉴 씬 관리 스크립트
public class MainMenuSceneManager : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField]
    private AudioClip clickEffectSound = null;

    [Space(10f)]
    [SerializeField]
    private AudioSource m_audioSource = null;
    [SerializeField]
    private GameObject helpImage = null;

    private const string gameSceneName = "NightShift";

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(helpImage.activeInHierarchy)
            {
                helpImage.SetActive(false);
            }
        }
    }

    public void StartButton()
    {
        ClickSound();
        if(SceneLoadManager.GetInstance() != null)
        {
            SceneLoadManager.GetInstance().LoadScene(gameSceneName);
        }
    }
    
    public void HelpButton()
    {
        ClickSound();
        if(helpImage != null &&
            !helpImage.activeInHierarchy)
        {
            helpImage.SetActive(true);
        }
    }

    public void ExitButton()
    {
        ClickSound();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void ClickSound()
    {
        if(m_audioSource != null &&
            clickEffectSound != null)
        {
            m_audioSource.PlayOneShot(clickEffectSound);
        }
    }
}
