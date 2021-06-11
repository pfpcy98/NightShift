using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public enum AgentStatus
{
    Recon,
    Chase,
    Detect,
    Attack,
    Hold,
    Possessed, // 1.5.7 추가 상태 : 원숭이로 인한 유혹 상태
}

public enum EyeTypeSoundIndex
{
    AttackStart,
    AttackHit,
    Walk,
    Run
}

public class EyeTypeMonster : MonoBehaviour
{
    // Recon 상태 관련 변수
    [Header ("Recon")]
    [SerializeField]
    private float walkSpeed = 3.5f;
    [SerializeField]
    private Transform[] recon_waypoints;
    private bool recon_is_Inversed = false;
    private int recon_index = 0;
    private const float recon_checkDistance = 1.0f; // 도착 판정 획득거리

    // Detect 상태 관련 변수
    [Space (10f)]
    [Header ("Detect")]
    [SerializeField]
    private float rushSpeed = 5.0f;
    // private DetectionWaypoint detect_Waypoint; // 1.5.1 버전 삭제
    private const float detect_checkDistance = 0.5f; // 도착 판정 획득거리

    //* 1.5.1 버전 추가 변수
    private const float detect_correctionRange = 5.0f; // 마지막 탐색 좌표 이후 좌표에 보정하는 값
    private const string detect_floorTag = "Floor"; // 바닥의 태그
    private Vector3 detect_lastDetectionPoint = Vector3.zero; // 보정된 최종 재탐지 좌표
    private bool detect_IsLastReachable = false; // 최종 재탐지 좌표가 도달 가능한지(바닥이 있는지?)
    private bool detect_IsReached = false; // 재탐지 중 첫번째 좌표에 도달했는지 체크하는 값

    // Attack 상태 관련 변수
    [Space(10f)]
    [Header("Attack")]
    [SerializeField]
    private float attack_delay = 1.0f;
    [SerializeField]
    private int attackPoint = 10;
    [SerializeField]
    private float attackRange = 2.0f;

    private MonsterAttackRange attack_range = null;
    private float attack_currentDelay = 0.0f;

    // 1.5.7 추가
    // Possessed 상태 관련 변수
    private GameObject possessHost = null; // 유혹 상태를 건 원숭이
    private bool lastPossessHit = false; // 유혹 상태의 마지막 타격인지(무적 효과 부여)

    // 1.6.0 추가
    // Chase 상태 관련 변수
    private bool chaseFirstCheck = false; // Chase 상태 돌입 시 최초 1회 실행 체크

    // Agent 자체 변수
    private Transform m_transform = null;
    private NavMeshAgent m_navAgent = null;
    private Animator m_animator = null;
    private AudioSource m_audio = null;
    // 1.5.7 변경 사항 : public get 프로퍼티 추가
    public AgentStatus m_status { get; private set; } = AgentStatus.Recon; // 현재 상태 변수
    private Vector3 m_posOffset = new Vector3(0, 1, 0); // 1.5.3 추가 변수, 에이전트 좌표값에 보정을 더하는 벡터

    //// 시야 관련 변수
    [Space (10f)]
    [Header("Sight")]
    private EyeTypeAroundSight m_aroundSight = null;
    [SerializeField]
    private float sphereSightWidth = 3.5f;
    [SerializeField]
    private double m_coneAngle = 30.0; // 부채꼴 탐지시야 각도
    [SerializeField]
    private float m_coneLength = 10.0f; // 부채꼴 탐지시야 범위
    private const string m_playerTag = "Player";
    private int m_ignoreLayerMask = -1; // 1.5.3 추가 변수, 투시가능한 오브젝트를 위한 레이어
    private int m_monsterLayerMask = -1;
    private int m_sightLayerMask = -1;
    private int m_noiseLayerMask = -1;

    //// 소리 관련 변수
    [Space (10f)]
    [Header("Sound")]
    [SerializeField]
    private AudioClip attackStartSound = null;
    [SerializeField]
    private AudioClip attackHitSound = null;
    [SerializeField]
    private AudioClip walkSound = null;
    [SerializeField]
    private AudioClip runSound = null;
    [SerializeField]
    private AudioClip chaseStartSound = null; // 1.5.5 추가 변수, 정찰-추격 전환 시 발생하는 소음


    // Start is called before the first frame update
    void Start()
    {
        // 오류 탐지 및 초기화
        m_transform = GetComponent<Transform>();
        if (m_transform == null)
        {
            Debug.LogError(name + ": Failed to load own transform!");
            Destroy(this.gameObject);
        }

        // 네비게이션 컴포넌트 획득
        m_navAgent = GetComponent<NavMeshAgent>();
        if(m_navAgent == null)
        {
            Debug.LogError(name + ": Failed to load own navAgent!");
            Destroy(this.gameObject);
        }
        else
        {
            m_navAgent.isStopped = true;
            m_navAgent.speed = walkSpeed;
        }

        // 정찰 웨이포인트 획득
        if(recon_waypoints == null)
        {
            Debug.LogError(name + ": This agent doesn't have any waypoints!");
            Destroy(this.gameObject);
        }
        else
        {
            m_navAgent.SetDestination(recon_waypoints[0].position);
        }

        // 애니메이터 컴포넌트 획득
        m_animator = GetComponent<Animator>();
        if(m_animator == null)
        {
            Debug.LogError(name + ": Failed to load own animator!");
            Destroy(this.gameObject);
        }

        // 레이캐스트 레이어 마스크 획득
        m_monsterLayerMask = LayerMask.NameToLayer("Monster");
        m_sightLayerMask = LayerMask.NameToLayer("Monster_Sight");
        m_noiseLayerMask = LayerMask.NameToLayer("Noise");
        m_ignoreLayerMask = LayerMask.NameToLayer("Ignore Raycast");
        if(m_monsterLayerMask == -1 ||
            m_sightLayerMask == -1 ||
            m_noiseLayerMask == -1 ||
            m_ignoreLayerMask == -1)
        {
            Debug.LogError(name + ": Failed to load layermask!");
            Destroy(this.gameObject);
        }

        // 주변시야 컴포넌트 획득
        m_aroundSight = GetComponentInChildren<EyeTypeAroundSight>();
        if(m_aroundSight == null)
        {
            Debug.LogError(name + " : Failed to activate around sight!");
            Destroy(this.gameObject);
        }
        else
        {
            SphereCollider collider = m_aroundSight.GetComponent<SphereCollider>();
            if(collider != null)
            {
                collider.radius = sphereSightWidth;
            }
        }
        
        // 공격 범위 컴포넌트 획득
        attack_range = GetComponentInChildren<MonsterAttackRange>();
        if(attack_range == null)
        {
            Debug.LogError(name + " : Failed to find own attack range!");
        }
        else
        {
            SphereCollider collider = attack_range.GetComponent<SphereCollider>();
            if(collider != null)
            {
                collider.radius = attackRange;
            }
        }

        // 오디오 컴포넌트 획득
        m_audio = GetComponent<AudioSource>();
        if(m_audio == null)
        {
            Debug.LogError(name + " : This agent cannot play own sound!");
        }
        else
        {
            AnimationCurve ac = m_audio.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            Keyframe[] keys = new Keyframe[1];

            for(int i = 0; i< keys.Length; i++)
            {
                keys[i].value = 1f;
            }

            ac.keys = keys;
            m_audio.SetCustomCurve(AudioSourceCurveType.SpatialBlend, ac);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 플레이어 발견 판정
        if (!(m_status == AgentStatus.Chase || m_status == AgentStatus.Attack || m_status == AgentStatus.Possessed))
        {
            if (SightJudge())
            {
                // 1.5.5 추가 코드
                // 상태가 정찰에서 전환되는 경우 발견용 사운드를 1회 출력
                if(m_status == AgentStatus.Recon &&
                    chaseStartSound != null)
                {
                    m_audio.PlayOneShot(chaseStartSound);
                }

                m_status = AgentStatus.Chase;
            }
        }

        // 현 상태에 따른 조종행동 실행
        switch(m_status)
        {
            case AgentStatus.Recon:
                Recon();
                break;

            case AgentStatus.Chase:
                Chase();
                break;

            case AgentStatus.Detect:
                Detect();
                break;

            case AgentStatus.Attack:
                Attack();
                break;

            case AgentStatus.Possessed: // 1.5.7 추가 : 유혹 상태 처리
                Possessed();
                break;

            default:
                break;
        }

        if(attack_currentDelay >= 0.0f)
        {
            attack_currentDelay -= Time.deltaTime;
        }
    }

    private void Recon()
    {
        // 웨이포인트가 있을 경우에만 로직 수행
        if (recon_waypoints != null)
        {
            if (!m_animator.GetBool("is_attack") &&
                m_navAgent.isStopped)
            {
                // 이동 애니메이션 재생
                m_animator.SetBool("is_move", true);
                m_navAgent.isStopped = false;
            }

            // 웨이포인트까지의 거리가 일정값 이하일 경우 다음 웨이포인트로 목표를 재설정
            float distance = (m_navAgent.destination - m_transform.position).magnitude;
            if (distance <= recon_checkDistance)
            {
                // 순회 방향에 따라 로직 수행
                if (!recon_is_Inversed)
                {
                    recon_index++;
                    if(recon_index + 1 >= recon_waypoints.Length)
                    {
                        recon_is_Inversed = !recon_is_Inversed;
                    }
                }
                else
                {
                    recon_index--;
                    if(recon_index - 1 < 0)
                    {
                        recon_is_Inversed = !recon_is_Inversed;
                    }
                }

                if (recon_index >= 0 && recon_index < recon_waypoints.Length)
                {
                    m_navAgent.SetDestination(recon_waypoints[recon_index].position);
                }
            }
        }
    }

    private void Chase()
    {
        // 1.6.0 추가 : 최초 1회 실행 체크
        if(!chaseFirstCheck)
        {
            PlayerStatusManager.GetInstance().IncreaseChasedStack();
            chaseFirstCheck = true;
        }

        if (!m_animator.GetBool("is_attack") &&
                m_navAgent.isStopped)
        {
            // 이동 애니메이션 재생
            m_animator.SetBool("is_move", true);
            m_navAgent.isStopped = false;
        }

        // 플레이어가 시야 내일경우 지속해서 추적
        if (SightJudge())
        {
            m_animator.SetBool("is_chase", true); // 1.5.5 추가

            m_navAgent.speed = rushSpeed;
            m_navAgent.SetDestination(PlayerStatusManager.GetInstance().GetPlayerPosition());

            // 시야 내에서 공격범위 이내 및 쿨타임 미적용일 경우 공격 상태로 전환
            if (attack_range.is_playerInRange)
            {
                if (attack_currentDelay <= 0.0f)
                {
                    m_status = AgentStatus.Attack;
                }
            }
        }

        // 시야에서 벗어날 경우 마지막으로 플레이어를 발견한 좌표에서 최단거리의 웨이포인트를 목표로 설정
        // 1.5.1 수정 : 재탐지 좌표 설정 로직 변경(마지막으로 플레이어를 발견한 좌표 - 마지막으로 확인한 플레이어의 이동방향 선 상의 특정 좌표)
        else
        {
            /* 1.5.1 이전 코드
            DetectionWaypoint[] detectionWaypoints = FindObjectsOfType<DetectionWaypoint>();
            if(detectionWaypoints != null)
            {
                Vector3 last_chasePoint = m_navAgent.destination;

                DetectionWaypoint temp = null;
                float shortest_Distance = float.MaxValue;

                for(int i = 0; i < detectionWaypoints.Length; i++)
                {
                    if(detectionWaypoints[i].IsMotherWaypoint())
                    {
                        float distance = Vector3.Distance(detectionWaypoints[i].transform.position, last_chasePoint);
                        if (distance < shortest_Distance)
                        {
                            temp = detectionWaypoints[i];
                            shortest_Distance = distance;
                        }
                    }
                }

                if (temp != null)
                {
                    detect_Waypoint = temp;
                    m_navAgent.SetDestination(detect_Waypoint.transform.position);
                }
            } */ // 1.5.1 이전 버전 코드 종료

            // 마지막으로 플레이어를 발견한 위치를 목표로 지정
            Vector3 lastPos = PlayerStatusManager.GetInstance().GetPlayerPosition();
            m_navAgent.SetDestination(lastPos);

            // 플레이어가 마지막으로 향하고 있던 방향에 보정값을 곱한 최종 재탐지 좌표를 계산
            detect_lastDetectionPoint = lastPos + (PlayerStatusManager.GetInstance().GetPlayerForward() * (detect_correctionRange + UnityEngine.Random.Range(0.0f, detect_correctionRange)));
            detect_lastDetectionPoint.y = detect_lastDetectionPoint.y + 1.0f;

            // 최종 재탐지 좌표의 수직 아래방향으로 Raycast 실행, 도달 가능한 바닥인지 체크
            int layerMask = ~((1 << m_monsterLayerMask) | (1 << m_sightLayerMask) | (1 << m_noiseLayerMask) | (1 << m_ignoreLayerMask));
            RaycastHit hitInfo;
            if (Physics.Raycast(detect_lastDetectionPoint, Vector3.down, out hitInfo, Mathf.Infinity, layerMask))
            {
                if (hitInfo.transform.CompareTag(detect_floorTag))
                {
                    detect_IsLastReachable = true;
                    detect_lastDetectionPoint = hitInfo.point;
                }
            }

            detect_IsReached = false;
            // 목표 설정 후 에이전트의 상태를 변경
            OnChaseExit();
            m_status = AgentStatus.Detect;
        }
    }

    private void Detect()
    {
        /* 1.5.1 버전 이전 코드
        // 웨이포인트가 있을 경우에만 로직 수행
        if (detect_Waypoint != null)
        {
            if (!m_animator.GetBool("is_attack") &&
                m_navAgent.isStopped)
            {
                // 이동 애니메이션 재생
                m_animator.SetBool("is_move", true);
                m_navAgent.isStopped = false;
            }

            // 웨이포인트까지의 거리가 일정값 이하일 경우 다음 웨이포인트로 목표를 재설정
            float distance = (m_navAgent.destination - m_transform.position).magnitude;
            if (distance <= detect_checkDistance)
            {
                m_navAgent.speed = walkSpeed;

                if(detect_Waypoint.GetNextWaypoint() != null)
                {
                    detect_Waypoint = detect_Waypoint.GetNextWaypoint();
                    m_navAgent.SetDestination(detect_Waypoint.transform.position);
                }

                // 다음 웨이포인트가 없을 경우 상태를 정찰로 변경
                else
                {
                    m_navAgent.SetDestination(recon_waypoints[recon_index].position);
                    m_status = AgentStatus.Recon;
                }
            }
        }
        // 재탐지 웨이포인트를 발견 못했을 경우(사실상 오류), 다시 정찰상태로 변경
        else
        {
            m_navAgent.speed = walkSpeed;
            m_navAgent.SetDestination(recon_waypoints[recon_index].position);
            m_status = AgentStatus.Recon;
        }
        */ // 1.5.1 버전 이전 코드 종료

        if(Vector3.Distance(m_transform.position, m_navAgent.destination) <= detect_checkDistance)
        {
            if (!detect_IsReached)
            {
                detect_IsReached = true;

                if (detect_IsLastReachable)
                {
                    m_navAgent.SetDestination(detect_lastDetectionPoint);
                }
                else
                {
                    m_navAgent.speed = walkSpeed;
                    m_navAgent.SetDestination(recon_waypoints[recon_index].position);
                    m_status = AgentStatus.Recon;
                }
            }
            else
            {
                m_navAgent.speed = walkSpeed;
                m_navAgent.SetDestination(recon_waypoints[recon_index].position);
                m_status = AgentStatus.Recon;
            }
        }
    }

    private void Attack()
    {
        if (!m_animator.GetBool("is_attack"))
        {
            m_animator.SetBool("is_move", false);
            m_animator.SetBool("is_attack", true);
            m_navAgent.isStopped = true;

            attack_currentDelay = attack_delay;
        }
    }

    // 1.5.7 추가 함수 : 유혹 상태 처리
    private void Possessed()
    {
        if (!m_animator.GetBool("is_attack") &&
            m_navAgent.isStopped)
        {
            // 이동 애니메이션 재생
            m_animator.SetBool("is_move", true);
            m_navAgent.isStopped = false;
        }

        // 유혹을 건 인형을 향해 지속해서 추적
        m_animator.SetBool("is_chase", true); // 1.5.5 추가

        m_navAgent.speed = rushSpeed;
        m_navAgent.SetDestination(possessHost.transform.position);

        // 공격범위 이내 및 쿨타임 미적용일 경우 공격 상태로 전환
        if (attack_range.is_playerInRange)
        {
            if (attack_currentDelay <= 0.0f)
            {
                m_status = AgentStatus.Attack;
            }
        }
    }

    // 시야 판단함수
    private bool SightJudge()
    {
        int layerMask = ~((1 << m_monsterLayerMask) | (1 << m_sightLayerMask) | (1 << m_noiseLayerMask) | (1 << m_ignoreLayerMask));
        Vector3 toDir = (PlayerStatusManager.GetInstance().GetPlayerPosition() - m_transform.position).normalized;

        // 주변 시야 감지 판정
        RaycastHit aroundHitInfo;
        if (m_aroundSight.is_detectedPlayer)
        {
            // 1.5.3 버전 수정사항
            // 주변시야에 감지되었어도 벽에 관통하는지 여부를 파악
            if(Physics.Raycast(m_transform.position + m_posOffset, toDir, out aroundHitInfo, sphereSightWidth, layerMask))
            {
                if(aroundHitInfo.transform.CompareTag(m_playerTag))
                {
                    return true;
                }
            }
        }


        // 1.5.3 버전 이전 코드
        /*
        // 아닐 경우, 광원 탐지 판정
        RaycastHit lightHitInfo;
        // Raycast 실행
        if (Physics.Raycast(m_transform.position, transform.forward, out lightHitInfo, Mathf.Infinity, layerMask))
        {
            if(lightHitInfo.transform.CompareTag(m_playerTag))
            {
                // 광원 내일 경우 최종적으로 주변 시야에 보이는 것으로 판단함.
                if (PlayerStatusManager.GetInstance().IsPlayerInLight())
                {
                    return true;
                }
            }
        }
        */
        // 1.5.3 버전 이전 코드 종료

        // 아닐 경우, 탐지 시야 판정
        // Raycast 정보 생성
        RaycastHit ConeHitInfo;

        // Raycast 실행
        //* 1.5.3 수정 사항
        //* 광원시야를 시야각 내로 넣기 위해 1차적인 탐지는 거리 무한대로 설정,
        //* 2차적으로 거리를 비교하여 탐지 판정을 내림으로 설정함.
        if(Physics.Raycast(m_transform.position + m_posOffset, toDir, out ConeHitInfo, Mathf.Infinity, layerMask))
        {
            if(ConeHitInfo.transform.CompareTag(m_playerTag))
            {
                // 플레이어일 경우 내적 계산을 통해 사잇각 산출
                float dotproduct = Vector3.Dot(m_transform.forward, toDir);
                double radian = Math.Acos(dotproduct);
                double degree = radian * (180.0 / Math.PI);

                // 사잇각이 시야각 내일 경우
                if (degree <= (m_coneAngle / 2))
                {
                    // 1차적으로 광원 적용중인지 체크
                    if(PlayerStatusManager.GetInstance().IsPlayerInLight())
                    {
                        return true;
                    }
                    
                    // 2차로 원뿔시야 거리 내인지 체크
                    if(Vector3.Distance(PlayerStatusManager.GetInstance().GetPlayerPosition(), m_transform.position) <= m_coneLength)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void StopAttackCall()
    {
        m_animator.SetBool("is_attack", false);
        // 1.5.7 추가 코드 : 유혹 상태 처리
        if (possessHost != null)
        {
            m_status = AgentStatus.Possessed;
        }
        else
        {
            m_status = AgentStatus.Chase;
        }

        // 1.5.7 추가 코드 : 유혹 상태에서의 마지막 공격 처리 후 무적 해제
        if(lastPossessHit)
        {
            lastPossessHit = false;
            m_status = AgentStatus.Recon;
        }
    }

    public void StartSound(EyeTypeSoundIndex index)
    {
        switch(index)
        {
            case EyeTypeSoundIndex.AttackStart:
                if (attackStartSound != null)
                {
                    m_audio.PlayOneShot(attackStartSound);
                }
                break;

            case EyeTypeSoundIndex.AttackHit:
                if (attackHitSound != null)
                {
                    m_audio.PlayOneShot(attackHitSound);
                    // 1.5.7 변경 사항 : 유혹 상태 체크
                    if (possessHost == null && !lastPossessHit)
                    {
                        PlayerStatusManager.GetInstance().SetHP(PlayerStatusManager.GetInstance().GetHP() - attackPoint);
                    }
                }
                break;

            case EyeTypeSoundIndex.Walk:
                if (walkSound != null)
                {
                    m_audio.PlayOneShot(walkSound);
                }
                break;

            case EyeTypeSoundIndex.Run:
                if (runSound != null)
                {
                    m_audio.PlayOneShot(runSound);
                }
                break;

            default:
                break;
        }
    }

    // 1.5.7 추가 함수 : 원숭이의 효과를 받는 함수
    public void OnPossessed(GameObject host)
    {
        possessHost = host;
        // 1.6.0 추가 : 직전에 추격 상태였을 경우 해제 함수 실행
        if(m_status == AgentStatus.Chase)
        {
            OnChaseExit();
        }
        m_status = AgentStatus.Possessed;
    }

    // 1.5.7 추가 함수 : 원숭이의 효과 종료 시 받는 함수
    public void OnFreeFromPossessed(GameObject host)
    {
        if (possessHost == host)
        {
            m_navAgent.speed = walkSpeed;
            m_navAgent.SetDestination(recon_waypoints[recon_index].position);
            m_status = AgentStatus.Recon;
            possessHost = null;

            attack_range.is_playerInRange = false;
            m_animator.SetBool("is_chase", false);
            if (m_animator.GetBool("is_attack"))
            {
                lastPossessHit = true;
            }
        }
    }

    // 1.6.0 추가 함수 : 추격 상태 해제 시 적용하는 함수
    private void OnChaseExit()
    {
        m_animator.SetBool("is_chase", false);
        chaseFirstCheck = false;
        PlayerStatusManager.GetInstance().DecreaseChasedStack();
    }
}
