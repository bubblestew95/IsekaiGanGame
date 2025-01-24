using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StartCoolTime", story: "Start [SkillCoolDown] and [SkillName]", category: "Action", id: "191d639c36dc6dcba4eb85cd6ec40e2e")]
public partial class StartCoolTimeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> SkillCoolDown;
    [SerializeReference] public BlackboardVariable<string> SkillName;
    protected override Status OnStart()
    {
        BossSkillCooldown[] skills = SkillCoolDown.Value.GetComponentsInChildren<BossSkillCooldown>();

        foreach (BossSkillCooldown skill in skills)
        {
            if (skill.bossSkillData.SkillName == SkillName.Value)
                skill.StartCooldown();
        }

        return Status.Running;
    }
}

