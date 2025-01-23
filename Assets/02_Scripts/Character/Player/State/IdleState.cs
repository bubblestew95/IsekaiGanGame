using EnumTypes;
using UnityEngine;

public class IdleState : BasePlayerState
{
    public IdleState(PlayerManager playerMng) : base(playerMng)
    {
    }

    public override void OnEnterState()
    {
        
    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        SkillType skillType = playerMng.GetNextSkill();

        if(skillType != SkillType.None)
        {
            // 이따 바꿔야 함.
            playerMng.UseSkill(skillType);
        }
    }
}
