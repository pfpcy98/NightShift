using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1.5.8 : Eye-type 스크립트 붙여넣기
public class EarEnableAttackJudge : StateMachineBehaviour
{
    private EarTypeMonster agent = null;
    private bool is_playedAttackSound = false;
    private const int animationFrame = 30;
    private const int attackHitFrame = 25;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<EarTypeMonster>();

        if (agent != null)
        {
            is_playedAttackSound = false;
            agent.StartSound(EarTypeSoundIndex.AttackStart);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= (attackHitFrame / (stateInfo.length * animationFrame)) &&
            !is_playedAttackSound)
        {
            agent.StartSound(EarTypeSoundIndex.AttackHit);
            is_playedAttackSound = true;
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(agent != null)
        {
            agent.StopAttackCall();
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
