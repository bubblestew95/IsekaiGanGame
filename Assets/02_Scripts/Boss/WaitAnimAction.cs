using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitAnim", story: "Wait [Animator] until [curAnim]", category: "Action", id: "8536f680feb7735693580ebdde6da006")]
public partial class WaitAnimAction : Action
{
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<string> CurAnim;
    protected override Status OnUpdate()
    {
        if (Animator.Value.GetCurrentAnimatorStateInfo(0).IsName(CurAnim.Value) && Animator.Value.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
        {
            return Status.Success;
        }
        return Status.Running;
    }
}

