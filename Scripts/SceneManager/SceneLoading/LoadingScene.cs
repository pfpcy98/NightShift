using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField]
    private Image loadingGauge = null;
    [SerializeField]
    private Image loadingCircle = null;
    [SerializeField]
    private Text tipText = null;

    [Header("Settings")]
    [Tooltip("�ϴܿ� ǥ���� �޽��� �����Դϴ�. �������� �ϳ��� �����Ǿ� ǥ�õ˴ϴ�.")]
    [Multiline(3)]
    [SerializeField]
    private string[] tipMessages = null;

    [Tooltip("���� ȸ�� �ӵ��� ���մϴ�.")]
    [SerializeField]
    private float circleRotationSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(tipMessages != null &&
            tipMessages.Length > 0 &&
            tipText != null)
        {
            int index = Random.Range(0, tipMessages.Length - 1);
            tipText.text = tipMessages[index];
        }

        SceneLoadManager.GetInstance().StartLoad();
    }

    // Update is called once per frame
    void Update()
    {
        if(loadingCircle != null)
        {
            loadingCircle.transform.Rotate(Vector3.back * circleRotationSpeed * Time.deltaTime);
        }

        if(loadingGauge != null &&
            SceneLoadManager.GetInstance() != null)
        {
            loadingGauge.fillAmount = SceneLoadManager.GetInstance().loadProgress;
        }
    }
}
