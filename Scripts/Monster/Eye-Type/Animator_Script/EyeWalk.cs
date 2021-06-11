using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

// 1.5.5 수정 : 프레임 입력 외부 노출
public class EyeWalk : StateMachineBehaviour
{
    private EyeTypeMonster agent = null;

    [Header ("Walk Frames")]
    [SerializeField]
    private float animationTotalFrame = 30.0f;
    [SerializeField]
    private float firstStepFrame = 5.0f;
    [SerializeField]
    private float secondStepFrame = 25.0f;

    private bool is_firstPlayed = false;
    private bool is_secondPlayed = false;

    private int replayedCount = 0;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(agent == null)
        {
            agent = animator.GetComponent<EyeTypeMonster>();
        }

        is_firstPlayed = false;
        is_secondPlayed = false;

        replayedCount = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null)
        {
            if (!is_firstPlayed)
            {
                if (stateInfo.normalizedTime >= replayedCount + (firstStepFrame / animationTotalFrame))
                {
                    agent.StartSound(EyeTypeSoundIndex.Walk);
                    is_firstPlayed = true;
                }
            }

            if(is_firstPlayed && !is_secondPlayed)
            {
                if (stateInfo.normalizedTime >= replayedCount + (secondStepFrame / animationTotalFrame))
                {
                    agent.StartSound(EyeTypeSoundIndex.Walk);
                    is_secondPlayed = true;
                }
            }
        }

        if(stateInfo.normalizedTime >= replayedCount + 1)
        {
            replayedCount++;
            is_firstPlayed = false;
            is_secondPlayed = false;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
