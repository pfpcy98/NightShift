using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.5.7 제작 스크립트 : 투척 원숭이 스크립트
public class MonkeyActivation : MonoBehaviour
{
    private Rigidbody m_rigidbody = null;
    private AudioSource m_audioSource = null;
    private Light m_light = null;

    [Header ("Monkey stats")]
    [SerializeField]
    private float monkeyActiveTime = 5.0f;
    [Tooltip ("원숭이의 작동 범위 : 이 범위값 내에선 모든 어그로가 원숭이로 향합니다.")]
    public float monkeyActiveRange = 10.0f;
    [SerializeField]
    [Tooltip ("원숭이의 소음 범위 : 작동 시작, 정지 시에 발생하는 소음의 범위 값입니다.")]
    private float monkeySoundRange = 15.0f;
    [SerializeField]
    [Tooltip ("원숭이의 소음 지속시간 : 작동 시작, 정지 시에 발생하는 소음의 지속시간입니다.")]
    private float monkeySoundTime = 1.0f;

    [Space(10f)]
    [Header("Monkey Resources")]
    [SerializeField]
    private AudioClip monkeyActiveSound = null;
    [SerializeField]
    private GameObject monkeyActiveArea = null;

    private const string m_floorTag = "Floor";
    private bool is_activationStart = false;
    private float remainActiveTime = 0.0f;
    private Queue<GameObject> possessedObjects = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null ||
            monkeyActiveSound == null)
        {
            Debug.LogError(name + " : This agent cannot play own sound!");
        }
        else
        {
            AnimationCurve ac = m_audioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            Keyframe[] keys = new Keyframe[1];

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value = 1f;
            }

            ac.keys = keys;
            m_audioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, ac);

            m_audioSource.clip = monkeyActiveSound;
            m_audioSource.loop = true;
        }

        m_light = GetComponent<Light>();

        remainActiveTime = monkeyActiveTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(is_activationStart)
        {
            remainActiveTime -= Time.deltaTime;
            if(remainActiveTime <= 0.0f)
            {
                NoiseManager.GetInstance().CreateObjectNoise(transform.position, monkeySoundRange, monkeySoundTime);
                m_audioSource.Stop();
                for(int i = 0; i < possessedObjects.Count; i++)
                {
                    FreeFromPossess(possessedObjects.Dequeue());
                }
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!is_activationStart)
        {
            if (collision.gameObject.CompareTag(m_floorTag))
            {
                if (m_rigidbody != null)
                {
                    m_rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    is_activationStart = true;
                    if (monkeyActiveArea != null)
                    {
                        monkeyActiveArea.SetActive(true);
                    }
                    if(m_light != null)
                    {
                        m_light.enabled = true;
                    }
                    NoiseManager.GetInstance().CreateObjectNoise(transform.position, monkeySoundRange, monkeySoundTime);
                    m_audioSource.Play();
                }
            }
        }
    }

    private void FreeFromPossess(GameObject agent)
    {
        EyeTypeMonster eye = agent.GetComponent<EyeTypeMonster>();
        EarTypeMonster ear = agent.GetComponent<EarTypeMonster>();

        if(eye != null)
        {
            eye.OnFreeFromPossessed(gameObject);
        }
        if(ear != null)
        {
            ear.OnFreeFromPossessed(gameObject);
        }
    }

    public void SetPossessedMonster(GameObject agent)
    {
        possessedObjects.Enqueue(agent);

        EyeTypeMonster eye = agent.GetComponent<EyeTypeMonster>();
        EarTypeMonster ear = agent.GetComponent<EarTypeMonster>();

        if (eye != null)
        {
            eye.OnPossessed(gameObject);
        }

        if (ear != null)
        {
            ear.OnPossessed(gameObject);
        }
    }
}
