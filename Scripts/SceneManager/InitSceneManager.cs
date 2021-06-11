using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 1.6.0 스크립트 : 시작 씬 관리 스크립트
public class InitSceneManager : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField]
    private AudioClip logoSound = null;

    [Space(10f)]
    [SerializeField]
    private Image logoImage = null;
    [SerializeField]
    private Text teamText = null;
    [SerializeField]
    private AudioSource m_audioSource = null;

    [Space(10f)]
    [SerializeField]
    private float fadeSpeed = 0.5f;

    private Color color = Color.white;
    private bool fadeInComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        color.a = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(!fadeInComplete)
        {
            logoImage.color = color;
            teamText.color = color;

            color.a += fadeSpeed * Time.deltaTime;
            
            if(color.a >= 1.0f)
            {
                fadeInComplete = true;

                if(m_audioSource != null &&
                    logoSound != null)
                {
                    m_audioSource.PlayOneShot(logoSound);
                }

                StartCoroutine(FadeOutAndChangeScene());
            }
        }
    }

    IEnumerator FadeOutAndChangeScene()
    {
        yield return new WaitForSeconds(1.0f);

        while(true)
        {
            logoImage.color = color;
            teamText.color = color;

            color.a -= fadeSpeed * Time.deltaTime;

            if (color.a <= 0.0f)
            {
                SceneLoadManager.GetInstance().LoadScene("MainMenu");
                break;
            }

            yield return null;
        }
    }
}
