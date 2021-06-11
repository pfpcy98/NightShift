using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Transform tr_flashLight;

    private Rigidbody rb_Player;
    private Animator ani_Player;
    private PlayerStatus player_Status;
    private PlayerSoundController soundController;

    private float speed_Multiplier = 1.0f;
    private Vector3 vec3_speed = Vector3.zero;

    private float keydownTime_upArrow = 0.0f;
    private float keydownTime_downArrow = 0.0f;
    private float keydownTime_leftArrow = 0.0f;
    private float keydownTime_rightArrow = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb_Player = GetComponent<Rigidbody>();
        ani_Player = GetComponent<Animator>();
        player_Status = GetComponent<PlayerStatus>();
        soundController = GetComponent<PlayerSoundController>();

        // 1.4.1 버전 추가
        // 마우스 커서를 숨김 & 화면 밖으로 나가지 않음
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 시야 회전(1.4.1 버전 기능 추가)
        //* 상하 회전(시야만 회전함)
        mainCamera.transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y"));
        //* 상하 회전각 제한
        if (mainCamera.transform.rotation.eulerAngles.x > 180 && mainCamera.transform.rotation.eulerAngles.x < 330)
        {
            mainCamera.transform.rotation = Quaternion.Euler(330,
                mainCamera.transform.rotation.eulerAngles.y,
                mainCamera.transform.rotation.eulerAngles.z);
        }
        else if (mainCamera.transform.rotation.eulerAngles.x < 180 && mainCamera.transform.rotation.eulerAngles.x > 30)
        {
            mainCamera.transform.rotation = Quaternion.Euler(30,
                mainCamera.transform.rotation.eulerAngles.y,
                mainCamera.transform.rotation.eulerAngles.z);
        }

        //* 좌우 회전(신체가 회전함)
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X"));

        /*
        if(tr_flashLight != null)
        {
            Vector3 rot = tr_flashLight.rotation.eulerAngles;
            rot.x = 90.0f + mainCamera.transform.rotation.eulerAngles.x;

            Quaternion newrot = Quaternion.Slerp(tr_flashLight.rotation, Quaternion.Euler(rot), 0.3f);
            tr_flashLight.rotation = newrot;
        }
        */

        // 속도 초기화
        vec3_speed = Vector3.zero;

        if (!player_Status.IsCannotControl())
        {
            // 달리기 키 입력 처리
            if (Input.GetKeyUp(KeyCode.LeftShift)) { player_Status.CancelRun(); }
            if (Input.GetKey(KeyCode.LeftShift) && !player_Status.is_Run_Cooldown) { player_Status.Run(); }

            // 달리기 상태 적용
            if (player_Status.is_Run)
            {
                ani_Player.SetBool("is_Run", true);
                speed_Multiplier = 2.0f;
            }
            else
            {
                ani_Player.SetBool("is_Run", false);
                speed_Multiplier = 1.0f;
            }

            // 키 입력 타임스탬프
            // 1.4.1 버전 수정사항 : 키코드 변경(화살표 -> WSAD)
            if (Input.GetKeyDown(KeyCode.W))
            {
                keydownTime_upArrow = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                keydownTime_downArrow = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                keydownTime_leftArrow = Time.time;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                keydownTime_rightArrow = Time.time;
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                keydownTime_upArrow = 0.0f;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                keydownTime_downArrow = 0.0f;
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                keydownTime_leftArrow = 0.0f;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                keydownTime_rightArrow = 0.0f;
            }

            // 키 입력에 따른 방향 벡터 추가
            // 1.4.1 버전 수정사항 : 월드 기준이 아닌 플레이어 기준 벡터로 변경(Vector3 -> transform)
            // 전방키 반응
            if (Input.GetKey(KeyCode.W) && (keydownTime_upArrow > keydownTime_downArrow))
            {
                // 전방 벡터 추가
                vec3_speed += transform.forward;

                // 측방키 동시 반응
                if (Input.GetKey(KeyCode.D) && (keydownTime_rightArrow > keydownTime_leftArrow))
                {
                    vec3_speed += transform.right;
                }
                else if (Input.GetKey(KeyCode.A) && (keydownTime_leftArrow > keydownTime_rightArrow))
                {
                    vec3_speed -= transform.right;
                }
            }

            // 측방키에만 반응
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)))
            {
                if (Input.GetKey(KeyCode.D) && (keydownTime_rightArrow > keydownTime_leftArrow))
                {
                    vec3_speed += transform.right;
                }
                else if (Input.GetKey(KeyCode.A) && (keydownTime_leftArrow > keydownTime_rightArrow))
                {
                    vec3_speed -= transform.right;
                }
            }

            // 후방키 반응(측방키 포함)
            if (Input.GetKey(KeyCode.S) && (keydownTime_downArrow > keydownTime_upArrow))
            {
                vec3_speed -= transform.forward;

                // 측방키 동시 반응
                if (Input.GetKey(KeyCode.D) && (keydownTime_rightArrow > keydownTime_leftArrow))
                {
                    vec3_speed += transform.right;
                }
                else if (Input.GetKey(KeyCode.A) && (keydownTime_leftArrow > keydownTime_rightArrow))
                {
                    vec3_speed -= transform.right;
                }
            }
        }

        // 산출 방향이 0이 아닐 경우(입력이 있을 경우)의 처리
        if (vec3_speed != Vector3.zero)
        {
            // 플레이어 상태를 Move로 변경
            player_Status.SetPlayerCondition(Player_Condition.Move);

            // 산출 방향으로 캐릭터 회전 -> 1.4.1 버전부터 기능 제외
            /*
            // 카메라 방향에 따른 조작방향 반대
            vec3_speed *= -1.0f;


            // 문워크 방지
            if (vec3_speed.z == -transform.forward.z)
            {
                if (vec3_speed.x == 0.0f)
                {
                    vec3_speed.x += 0.1f;
                }
            }
            if(vec3_speed.x == -transform.right.z)
            {
                if (vec3_speed.z == 0.0f)
                {
                    vec3_speed.z += 0.1f;
                }
            }

            transform.forward = Vector3.Lerp(transform.forward, vec3_speed.normalized, 0.05f);
            */
        }
        else
        {
            // 행동 불가능 상태로 인한 상태 통제가 아닐 경우에만 상태를 중립(Idle)으로 전환
            if (!player_Status.IsCannotControl())
            {
                player_Status.SetPlayerCondition(Player_Condition.Idle);
            }
        }
        // 중력을 반영한 속력 계산
        vec3_speed.Normalize();
        vec3_speed *= player_Status.GetMovementSpeed() * speed_Multiplier;
        vec3_speed.y = rb_Player.velocity.y;

        // 계산 속도 반영
        rb_Player.velocity = vec3_speed;

        // 걷기 모션 적용
        if(player_Status.player_Condition == Player_Condition.Move) { ani_Player.SetBool("is_Move", true); }
        else 
        { 
            ani_Player.SetBool("is_Move", false);
            // 걷기 인덱스 초기화
            if(soundController != null)
            {
                soundController.walkSoundIndex = 0;
            }
        }
    }
}
