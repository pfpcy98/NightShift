using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    private static SceneLoadManager m_instance = null;

    private const string loadingSceneName = "OnLoading";
    private static string nextSceneName = string.Empty;
    public float loadProgress { get; private set; } = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(m_instance == null)
        {
            m_instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public static SceneLoadManager GetInstance()
    {
        if(m_instance == null)
        {
            return null;
        }

        return m_instance;
    }

    public void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene(loadingSceneName);
    }

    public void StartLoad()
    {
        StartCoroutine(AsyncLoadScene());
    }

    private IEnumerator AsyncLoadScene()
    {
        yield return new WaitForSeconds(1.0f);
        AsyncOperation loadOP = SceneManager.LoadSceneAsync(nextSceneName);
        loadOP.allowSceneActivation = false;
        while (!loadOP.isDone)
        {
            yield return null;
            loadProgress = loadOP.progress;

            if(loadProgress >= 0.9f)
            {
                loadOP.allowSceneActivation = true;
            }
        }

        loadProgress = 0.0f;
        nextSceneName = string.Empty;
    }
}
