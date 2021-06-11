using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkSound : StateMachineBehaviour
{
    [SerializeField]
    private float soundStartFrame = 0;
    [SerializeField]
    private float soundDelay = 0;
    [SerializeField]
    private float frameLength = 0;

    private PlayerSoundController soundController = null;

    private bool isFirstStepPlayed = false;
    private bool isSecondStepPlayed = false;
    private int replayedCount = 0;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(soundDelay != 0 &&
            frameLength != 0)
        {
            soundController = animator.GetComponent<PlayerSoundController>();
        }

        isFirstStepPlayed = false;
        isSecondStepPlayed = false;
        replayedCount = 0;
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(soundController != null)
        {
            if (!isFirstStepPlayed)
            {
                if (stateInfo.normalizedTime >= replayedCount + (soundStartFrame / frameLength))
                {
                    soundController.StartFXSound(PlayerFXIndex.Walk);
                    isFirstStepPlayed = true;
                }
            }

            if (isFirstStepPlayed && !isSecondStepPlayed)
            {
                if (stateInfo.normalizedTime >= replayedCount + ((soundStartFrame + soundDelay) / frameLength))
                {
                    soundController.StartFXSound(PlayerFXIndex.Walk);
                    isSecondStepPlayed = true;
                }
            }

            if(stateInfo.normalizedTime >= replayedCount + 1)
            {
                replayedCount++;
                isFirstStepPlayed = false;
                isSecondStepPlayed = false;
            }
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
