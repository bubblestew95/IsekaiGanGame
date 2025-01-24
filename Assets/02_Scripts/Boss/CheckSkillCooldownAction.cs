using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckSkillCooldown", story: "Check Skills Cooldown in [skills] and set [RandomState]", category: "Action", id: "74ae8706ee42c0a639a8b3c0c870e0fb")]
public partial class CheckSkillCooldownAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Skills;
    [SerializeReference] public BlackboardVariable<BossState> RandomState;

    protected override Status OnStart()
    {
        CheckCooldownRandom();

        Debug.Log(RandomState.Value + "실행된 현재 상태");

        return Status.Running;
    }

    private void CheckCooldownRandom()
    {
        BossSkillCooldown[] skills = Skills.Value.GetComponentsInChildren<BossSkillCooldown>();

        List<BossSkillCooldown> coolOnSkills = new List<BossSkillCooldown>();

        foreach (BossSkillCooldown skill in skills)
        {
            if (skill.IsCooldownOver())
            {
                coolOnSkills.Add(skill);
            }
        }

        if (coolOnSkills.Count == 0)
        {
            RandomState.Value = BossState.Idle;
        }
        else
        {
            int randomIndex = UnityEngine.Random.Range(0, coolOnSkills.Count);
            string selectSkillName = coolOnSkills[randomIndex].bossSkillData.SkillName;

            RandomState.Value = (BossState)Enum.Parse(typeof(BossState), selectSkillName);
        }
    }
}

