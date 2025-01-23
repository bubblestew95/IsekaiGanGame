using UnityEngine;

public class ResetTrigger : StateMachineBehaviour
{
    [SerializeField] private string TriggerName;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(TriggerName);
    }
}
