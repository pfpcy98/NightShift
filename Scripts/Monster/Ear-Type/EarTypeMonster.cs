using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EarTypeSoundIndex
{
    // 1.5.8 변경 : Explosion -> AttackStart/Hit 분화
    AttackStart,
    AttackHit,
    Walk,
    Run, // 1.5.8 추가 변수
}

public class EarTypeMonster : MonoBehaviour
{
    // Recon 상태 관련 변수
    [SerializeField]
    private float walkSpeed = 3.5f;
    [SerializeField]
    private Transform[] recon_waypoints;
    private bool recon_is_Inversed = false;
    private int recon_index = 0;
    private const float recon_checkDistance = 1.0f; // 도착 판정 획득거리

    // Chase 상태 관련 변수
    [SerializeField]
    private float rushSpeed = 5.0f;
    private const string chase_noiseTag = "Noise";
    private const float chase_checkDistance = 0.5f; // 도착 판정 획득거리

    private bool is_noiseReached = true; // 마지막으로 들린 소음에 위치한 적이 있는지 체크
    private Vector3 noisePos = Vector3.zero; // 가장 최근에 들은 소음 위치

    private bool chaseFirstCheck = false; // 1.6.0 추가 : 플레이어에 의한 Chase 상태 돌입 시 최초 1회 실행 체크

    // Attack 상태 관련 변수
    [SerializeField]
    private float attack_delay = 1.0f;
    [SerializeField]
    private int attackPoint = 10;
    [SerializeField]
    private float attackRange = 2.0f;

    private MonsterAttackRange attack_range = null;
    private float attack_currentDelay = 0.0f;

    // Hold 상태 관련 변수
    [SerializeField]
    private float holdTime = 5.0f; // 대기 시간
    private float currentHoldTime = 0.0f;

    // 1.5.7 추가
    // Possessed 상태 관련 변수
    private GameObject possessHost = null; // 유혹 상태를 건 원숭이
    private bool lastPossessHit = false; // 유혹 상태의 마지막 타격인지(무적 효과 부여)

    // Agent 자체 변수
    private Transform m_transform = null;
    private NavMeshAgent m_navAgent = null;
    private Animator m_animator = null;
    private AudioSource m_audioSource = null;
    // 1.5.7 변경 사항 : public get 프로퍼티 추가
    public AgentStatus m_status { get; private set; } = AgentStatus.Recon; // 현재 상태 변수
    private Vector3 m_posOffset = new Vector3(0, 1, 0); // 1.5.3 추가 변수, 에이전트 좌표값에 보정을 더하는 벡터

    // 시야 관련 변수
    private EyeTypeAroundSight m_aroundSight = null;
    [SerializeField]
    private float sphereSightWidth = 3.5f;
    //* 1.5.3 추가 변수
    private const string m_playerTag = "Player";
    private int m_ignoreLayerMask = -1; // 1.5.3 추가 변수, 투시가능한 오브젝트를 위한 레이어
    private int m_monsterLayerMask = -1;
    private int m_sightLayerMask = -1;
    private int m_noiseLayerMask = -1;

    //// 소리
    [SerializeField]
    private AudioClip attackStartSound; // 1.5.8 추가, 공격 시작 사운드
    [SerializeField]
    private AudioClip attackHitSound; // 1.5.8 추가, 공격 타격 사운드
    [SerializeField]
    private AudioClip walkSound;
    [SerializeField]
    private AudioClip runSound; // 1.5.8 추가, 추격 상태 발 사운드
    [SerializeField]
    private AudioClip chaseStartSound = null; // 1.5.5 추가 변수, 정찰-추격 상태전환 시 출력되는 사운드

    // Start is called before the first frame update
    void Start()
    {        
        // 오류 탐지 및 초기화
        m_transform = GetComponent<Transform>();
        if (m_transform == null)
        {
            Debug.LogError(name + " : Failed to load own transform!");
            Destroy(this.gameObject);
        }

        // 네비게이션 컴포넌트 획득
        m_navAgent = GetComponent<NavMeshAgent>();
        if (m_navAgent == null)
        {
            Debug.LogError(name + " : Failed to load own navAgent!");
            Destroy(this.gameObject);
        }

        // 정찰 웨이포인트 획득
        if (recon_waypoints == null)
        {
            Debug.LogError(name + " : This agent doesn't have any waypoints!");
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
            Debug.LogError(name + " : Failed to load own animator!");
        }

        // 주변시야 컴포넌트 획득
        m_aroundSight = GetComponentInChildren<EyeTypeAroundSight>();
        if (m_aroundSight == null)
        {
            Debug.LogError(name + " : Failed to activate around sight!");
            Destroy(this.gameObject);
        }
        else
        {
            SphereCollider collider = m_aroundSight.GetComponent<SphereCollider>();
            if (collider != null)
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
            if (collider != null)
            {
                collider.radius = attackRange;
            }
        }

        // 오디오 컴포넌트 획득
        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null)
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
        }

        // 1.5.3 추가 코드
        // 레이캐스트 레이어 마스크 획득
        m_monsterLayerMask = LayerMask.NameToLayer("Monster");
        m_sightLayerMask = LayerMask.NameToLayer("Monster_Sight");
        m_noiseLayerMask = LayerMask.NameToLayer("Noise");
        m_ignoreLayerMask = LayerMask.NameToLayer("Ignore Raycast");
        if (m_monsterLayerMask == -1 ||
            m_sightLayerMask == -1 ||
            m_noiseLayerMask == -1 ||
            m_ignoreLayerMask == -1)
        {
            Debug.LogError(name + ": Failed to load layermask!");
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 플레이어 발견 판정
        if (!(m_status == AgentStatus.Chase || m_status == AgentStatus.Attack))
        {
            if (SightJudge())
            {
                // 1.5.5 추가 코드
                // 정찰-추격 상태 전환 시 사운드 1회 출력
                if(m_status == AgentStatus.Recon &&
                    chaseStartSound != null)
                {
                    m_audioSource.PlayOneShot(chaseStartSound);
                }

                m_status = AgentStatus.Chase;
            }
        }

        switch (m_status)
        {
            case AgentStatus.Recon:
                Recon();
                break;

            case AgentStatus.Chase:
                Chase();
                break;

            case AgentStatus.Attack:
                Attack();
                break;

            case AgentStatus.Hold:
                Hold();
                break;

            case AgentStatus.Possessed:
                Possessed();
                break;

            default:
                break;
        }

        if (attack_currentDelay >= 0.0f)
        {
            attack_currentDelay -= Time.deltaTime;
        }
    }

    private void Recon()
    {
        // 웨이포인트가 있을 경우에만 로직 수행
        if (recon_waypoints != null)
        {
            m_animator.SetBool("is_move", true);

            // 웨이포인트까지의 거리가 일정값 이하일 경우 다음 웨이포인트로 목표를 재설정
            float distance = (m_navAgent.destination - m_transform.position).magnitude;
            if (distance <= recon_checkDistance)
            {
                // 순회 방향에 따라 로직 수행
                if (!recon_is_Inversed)
                {
                    recon_index++;
                    if (recon_index + 1 >= recon_waypoints.Length)
                    {
                        recon_is_Inversed = !recon_is_Inversed;
                    }
                }
                else
                {
                    recon_index--;
                    if (recon_index - 1 < 0)
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
        if (!m_animator.GetBool("is_attack") &&
                m_navAgent.isStopped)
        {
            // 이동 애니메이션 재생
            m_animator.SetBool("is_move", true);
            m_navAgent.isStopped = false;
        }

        m_animator.SetBool("is_chase", true);

        // 플레이어가 시야 내일경우 지속해서 추적
        if (SightJudge())
        {
            // 1.6.0 추가 : 플레이어가 직접적인 시야 내로 들어왔을 때만 추격 체크
            if(!chaseFirstCheck)
            {
                chaseFirstCheck = true;
                PlayerStatusManager.GetInstance().IncreaseChasedStack();
            }
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

        // 플레이어가 시야에 없을 경우 가장 마지막에 들은 소음에 닿은 적 있는지 체크
        else if (!is_noiseReached)
        {
            // 1.6.0 추가 : 플레이어가 시야에 들어왔던 적이 있다면 추격 체크 해제
            if(chaseFirstCheck)
            {
                OnChaseExit();
            }
            // 목적지에 닿았으면 대기로 전환
            float distance = (noisePos - m_transform.position).magnitude;
            if (distance <= chase_checkDistance)
            {
                is_noiseReached = true;
                m_navAgent.speed = walkSpeed;
                currentHoldTime = holdTime;
                m_animator.SetBool("is_chase", false);
                m_status = AgentStatus.Hold;
            }
            // 닿지 않았다면 마지막에 들은 소음을 향해 이동
            else
            {
                m_navAgent.speed = rushSpeed;
                m_navAgent.SetDestination(noisePos);
            }
        }

        // 소음을 들은 적 없거나 마지막에 들었던 소음에 이미 도달한 적이 있다면 즉시 대기
        else
        {
            // 1.6.0 추가 : 플레이어가 시야에 들어왔던 적이 있다면 추격 체크 해제
            if (chaseFirstCheck)
            {
                OnChaseExit();
            }

            m_navAgent.speed = walkSpeed;
            currentHoldTime = holdTime;
            m_animator.SetBool("is_chase", false);
            m_status = AgentStatus.Hold;
        }
    }

    private void Attack()
    {
        if(!m_animator.GetBool("is_attack"))
        {
            m_navAgent.isStopped = true;
            m_animator.SetBool("is_move", false);
            m_animator.SetBool("is_attack", true);

            attack_currentDelay = attack_delay;
        }
    }

    private void Hold()
    {
        m_navAgent.isStopped = true;
        m_animator.SetBool("is_move", false);
        currentHoldTime -= Time.deltaTime;

        if(currentHoldTime <= 0.0f)
        {
            m_navAgent.isStopped = false;
            m_status = AgentStatus.Recon;
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
        // 1.5.3 수정 사항
        // 주변시야에 감지되어도 장애물이 가로막는지 체크
        if (m_aroundSight.is_detectedPlayer)
        {
            int layerMask = ~((1 << m_monsterLayerMask) | (1 << m_sightLayerMask) | (1 << m_noiseLayerMask) | (1 << m_ignoreLayerMask));
            Vector3 toDir = (PlayerStatusManager.GetInstance().GetPlayerPosition() - m_transform.position).normalized;

            RaycastHit hitInfo;
            if(Physics.Raycast(m_transform.position + m_posOffset, toDir, out hitInfo, Mathf.Infinity, layerMask))
            {
                if(hitInfo.transform.CompareTag(m_playerTag))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // 소음 판정
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag(chase_noiseTag))
        {
            noisePos = other.transform.position;
            is_noiseReached = false;

            // 1.5.5 추가 코드
            // 정찰-추격 상태 전환 시 사운드 1회 출력
            if(m_status == AgentStatus.Recon &&
                m_audioSource != null &&
                chaseStartSound != null)
            {
                m_audioSource.PlayOneShot(chaseStartSound);
            }

            m_status = AgentStatus.Chase;
        }
    }

    public void StopAttackCall()
    {
        m_animator.SetBool("is_attack", false);
        m_navAgent.isStopped = false;

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
        if (lastPossessHit)
        {
            lastPossessHit = false;
            m_status = AgentStatus.Recon;
        }
    }

    public void StartSound(EarTypeSoundIndex index)
    {
        switch(index)
        {
            // 1.5.8 추가
            case EarTypeSoundIndex.AttackStart:
                if(attackStartSound != null)
                {
                    m_audioSource.PlayOneShot(attackStartSound);
                }
                break;

            // 1.5.8 변경 : Explosion -> AttackHit
            case EarTypeSoundIndex.AttackHit:
                if(attackHitSound != null)
                {
                    m_audioSource.PlayOneShot(attackHitSound);
                    // 1.5.7 추가 사항 : 유혹 상태 체크
                    if (possessHost == null && !lastPossessHit)
                    {
                        PlayerStatusManager.GetInstance().SetHP(PlayerStatusManager.GetInstance().GetHP() - attackPoint);
                    }
                }
                break;

            case EarTypeSoundIndex.Walk:
                if (walkSound != null)
                {
                    m_audioSource.PlayOneShot(walkSound);
                }
                break;

            // 1.5.8 추가
            case EarTypeSoundIndex.Run:
                if(runSound != null)
                {
                    m_audioSource.PlayOneShot(runSound);
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
        // 1.6.0 추가 : 직전에 플레이어에 의한 추격 상태였을 경우 해제 함수 실행
        if(m_status == AgentStatus.Chase &&
            chaseFirstCheck)
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
        chaseFirstCheck = false;
        PlayerStatusManager.GetInstance().DecreaseChasedStack();
    }
}
